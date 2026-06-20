using AutoMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using VAM.DTOs;
using VAM.Entities;
using VAM.Repositories;
using BCrypt.Net;
using Google.Apis.Auth;

namespace VAM.Services
{
    public class UserService : ServiceBase<User, UserDto, CreateUserDto, UpdateUserDto>, IUserService
    {
        private readonly IConfiguration _config;
        private readonly IEmailService _emailService;

        public UserService(IUnitOfWork unitOfWork, IMapper mapper, IConfiguration config, IEmailService emailService)
            : base(unitOfWork, unitOfWork.Users, mapper)
        {
            _config = config;
            _emailService = emailService;
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
        {
            var users = await _unitOfWork.Users.FindAsync(u => u.Email == dto.Email);
            var user = users.FirstOrDefault();

            bool isValid = false;
            try
            {
                if (user != null)
                {
                    isValid = BCrypt.Net.BCrypt.Verify(dto.Password, user.Password);
                }
            }
            catch
            {
                isValid = false;
            }

            if (user == null || !isValid)
                throw new Exception("Invalid email or password");

            var token = GenerateJwtToken(user, "access", TimeSpan.FromHours(2));
            return new AuthResponseDto
            {
                Token = token,
                User = _mapper.Map<UserDto>(user)
            };
        }

        public async Task<AuthResponseDto> LoginWithGoogleAsync(GoogleLoginDto dto)
        {
            GoogleJsonWebSignature.Payload payload;
            try
            {
                var googleClientId = _config["Google:ClientId"];
                payload = await GoogleJsonWebSignature.ValidateAsync(dto.IdToken, new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = string.IsNullOrEmpty(googleClientId) ? null : new[] { googleClientId }
                });
            }
            catch (InvalidJwtException e)
            {
                throw new Exception("Invalid Google ID token", e);
            }

            var users = await _unitOfWork.Users.FindAsync(u => u.Email == payload.Email);
            var user = users.FirstOrDefault();

            if (user == null)
            {
                user = new User
                {
                    Name = payload.Name ?? "Google User",
                    Email = payload.Email,
                    Password = BCrypt.Net.BCrypt.HashPassword(Guid.NewGuid().ToString()),
                    Role = UserRole.customer.ToString(),
                    Status = UserStatus.active.ToString()
                };

                await _unitOfWork.Users.CreateAsync(user);
                await _unitOfWork.CompleteAsync();
            }
            else
            {
                if (user.Status == UserStatus.inactive.ToString() || user.Status == UserStatus.banned.ToString())
                {
                    throw new Exception("Account is inactive or banned");
                }
            }

            var token = GenerateJwtToken(user, "access", TimeSpan.FromHours(2));
            return new AuthResponseDto
            {
                Token = token,
                User = _mapper.Map<UserDto>(user)
            };
        }

        public async Task<UserDto> RegisterAsync(RegisterDto dto)
        {
            var existingUsers = await _unitOfWork.Users.FindAsync(u => u.Email == dto.Email);
            var existingUser = existingUsers.FirstOrDefault();
            if (existingUser != null)
                throw new Exception("Email already exists");

            var user = new User
            {
                Name = dto.Name,
                Email = dto.Email,
                Password = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Phone = dto.Phone,
                Address = dto.Address,
                Role = UserRole.customer.ToString(),
                Status = UserStatus.pending.ToString()
            };

            await _unitOfWork.Users.CreateAsync(user);
            await _unitOfWork.CompleteAsync();

            return _mapper.Map<UserDto>(user);
        }

        public async Task ForgotPasswordAsync(ForgotPasswordDto dto)
        {
            var users = await _unitOfWork.Users.FindAsync(u => u.Email == dto.Email);
            var user = users.FirstOrDefault();
            if (user == null)
                return; // Do not reveal if email exists

            var resetToken = GenerateJwtToken(user, "reset_password", TimeSpan.FromMinutes(15));
            var resetLink = $"http://localhost:3000/reset-password?token={resetToken}";

            var emailBody = $"<h1>Reset Password</h1><p>Click <a href='{resetLink}'>here</a> to reset your password. It expires in 15 minutes.</p>";
            await _emailService.SendEmailAsync(user.Email, "Reset Password Request", emailBody);
        }

        public async Task ResetPasswordAsync(ResetPasswordDto dto)
        {
            var handler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_config["Jwt:Key"] ?? "");

            try
            {
                handler.ValidateToken(dto.Token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _config["Jwt:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = _config["Jwt:Audience"],
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;
                var purpose = jwtToken.Claims.FirstOrDefault(x => x.Type == "purpose")?.Value;
                if (purpose != "reset_password")
                    throw new Exception("Invalid token purpose");

                var userIdStr = jwtToken.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
                    throw new Exception("Invalid token claims");

                var user = await _unitOfWork.Users.GetByIdAsync(userId);
                if (user == null)
                    throw new Exception("User not found");

                user.Password = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
                _unitOfWork.Users.Update(user);
                await _unitOfWork.CompleteAsync();
            }
            catch
            {
                throw new Exception("Invalid or expired reset token");
            }
        }

        public async Task ChangePasswordAsync(int userId, ChangePasswordDto dto)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
                throw new Exception("User not found");

            if (!BCrypt.Net.BCrypt.Verify(dto.OldPassword, user.Password))
                throw new Exception("Incorrect old password");

            user.Password = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            _unitOfWork.Users.Update(user);
            await _unitOfWork.CompleteAsync();

            var emailBody = "<h1>Password Changed</h1><p>Your password was recently changed. If this wasn't you, please contact support immediately.</p>";
            await _emailService.SendEmailAsync(user.Email, "Security Alert: Password Changed", emailBody);
        }

        public Task LogoutAsync(int userId)
        {
            // Stateless JWT. Client deletes the token. Can log the event if needed.
            return Task.CompletedTask;
        }

        private string GenerateJwtToken(User user, string purpose, TimeSpan expiry)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_config["Jwt:Key"] ?? "");
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, user.Role),
                    new Claim("purpose", purpose)
                }),
                Expires = DateTime.UtcNow.Add(expiry),
                Issuer = _config["Jwt:Issuer"],
                Audience = _config["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}