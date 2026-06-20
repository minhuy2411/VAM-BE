using VAM.DTOs;
using System.Threading.Tasks;

namespace VAM.Services
{
    public interface IUserService : IServiceBase<UserDto, CreateUserDto, UpdateUserDto> 
    {
        Task<AuthResponseDto> LoginAsync(LoginDto dto);
        Task<AuthResponseDto> LoginWithGoogleAsync(GoogleLoginDto dto);
        Task<UserDto> RegisterAsync(RegisterDto dto);
        Task ForgotPasswordAsync(ForgotPasswordDto dto);
        Task ResetPasswordAsync(ResetPasswordDto dto);
        Task ChangePasswordAsync(int userId, ChangePasswordDto dto);
        Task LogoutAsync(int userId);
    }
}