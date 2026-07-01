using AutoMapper;
using System;
using System.Linq;
using System.Threading.Tasks;
using VAM.DTOs;
using VAM.Entities;
using VAM.Repositories;

namespace VAM.Services
{
    public class ProfileService : IProfileService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IEmailService _emailService;
        private readonly IFirebaseStorageService _firebaseStorageService;

        public ProfileService(IUnitOfWork unitOfWork, IMapper mapper, IEmailService emailService, IFirebaseStorageService firebaseStorageService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _emailService = emailService;
            _firebaseStorageService = firebaseStorageService;
        }

        public async Task<SellerProfileDto> CreateSellerProfileAsync(int userId, CreateSellerProfileDto dto)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null) throw new Exception("User not found");

            var existingProfiles = await _unitOfWork.SellerProfiles.FindAsync(p => p.UserId == userId);
            if (existingProfiles.Any()) throw new Exception("Seller profile already exists");

            string? certificateUrl = null;
            if (dto.Certificate != null)
            {
                certificateUrl = await _firebaseStorageService.UploadFileAsync(dto.Certificate, "certificates");
            }

            var profile = new SellerProfile
            {
                UserId = userId,
                FarmName = dto.FarmName,
                FarmAddress = dto.FarmAddress,
                AquacultureType = dto.AquacultureType,
                Certificate = certificateUrl,
                Note = dto.Note,
                Status = ProfileStatus.PENDING
            };

            await _unitOfWork.SellerProfiles.CreateAsync(profile);
            await _unitOfWork.CompleteAsync();

            return _mapper.Map<SellerProfileDto>(profile);
        }

        public async Task<BusinessProfileDto> CreateBusinessProfileAsync(int userId, CreateBusinessProfileDto dto)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null) throw new Exception("User not found");

            var existingProfiles = await _unitOfWork.BusinessProfiles.FindAsync(p => p.UserId == userId);
            if (existingProfiles.Any()) throw new Exception("Business profile already exists");

            string? businessLicenseUrl = null;
            if (dto.BusinessLicense != null)
            {
                businessLicenseUrl = await _firebaseStorageService.UploadFileAsync(dto.BusinessLicense, "business_licenses");
            }

            var profile = new BusinessProfile
            {
                UserId = userId,
                CompanyName = dto.CompanyName,
                TaxCode = dto.TaxCode,
                BusinessLicense = businessLicenseUrl,
                Address = dto.Address,
                Note = dto.Note,
                Status = ProfileStatus.PENDING
            };

            await _unitOfWork.BusinessProfiles.CreateAsync(profile);
            await _unitOfWork.CompleteAsync();

            return _mapper.Map<BusinessProfileDto>(profile);
        }

        public async Task<SellerProfileDto?> GetMySellerProfileAsync(int userId)
        {
            var profiles = await _unitOfWork.SellerProfiles.FindAsync(p => p.UserId == userId);
            var profile = profiles.FirstOrDefault();
            return profile == null ? null : _mapper.Map<SellerProfileDto>(profile);
        }

        public async Task<BusinessProfileDto?> GetMyBusinessProfileAsync(int userId)
        {
            var profiles = await _unitOfWork.BusinessProfiles.FindAsync(p => p.UserId == userId);
            var profile = profiles.FirstOrDefault();
            return profile == null ? null : _mapper.Map<BusinessProfileDto>(profile);
        }

        public async Task<PaginatedResult<SellerProfileDto>> GetPendingSellerProfilesAsync(int page, int pageSize)
        {
            var query = await _unitOfWork.SellerProfiles.FindAsync(p => p.Status == ProfileStatus.PENDING);
            var totalCount = query.Count();
            var items = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            return new PaginatedResult<SellerProfileDto>
            {
                Items = items.Select(i => _mapper.Map<SellerProfileDto>(i)).ToList(),
                TotalCount = totalCount,
                PageNumber = page,
                PageSize = pageSize
            };
        }

        public async Task<PaginatedResult<BusinessProfileDto>> GetPendingBusinessProfilesAsync(int page, int pageSize)
        {
            var query = await _unitOfWork.BusinessProfiles.FindAsync(p => p.Status == ProfileStatus.PENDING);
            var totalCount = query.Count();
            var items = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            return new PaginatedResult<BusinessProfileDto>
            {
                Items = items.Select(i => _mapper.Map<BusinessProfileDto>(i)).ToList(),
                TotalCount = totalCount,
                PageNumber = page,
                PageSize = pageSize
            };
        }

        public async Task ApproveSellerProfileAsync(int profileId, int adminId, ApproveProfileDto dto)
        {
            var profile = await _unitOfWork.SellerProfiles.GetByIdAsync(profileId);
            if (profile == null) throw new Exception("Profile not found");

            var user = await _unitOfWork.Users.GetByIdAsync(profile.UserId);
            if (user == null) throw new Exception("User not found");

            profile.Status = dto.IsApproved ? ProfileStatus.APPROVED : ProfileStatus.REJECTED;
            profile.VerifiedById = adminId;
            profile.VerifiedAt = DateTimeOffset.UtcNow;

            _unitOfWork.SellerProfiles.Update(profile);

            if (dto.IsApproved)
            {
                user.Role = UserRole.seller.ToString();
                user.Status = UserStatus.active.ToString();
                _unitOfWork.Users.Update(user);
                await _unitOfWork.CompleteAsync();
                var htmlContent = BuildEmailTemplate(user.Name ?? user.Email, "Hồ sơ Người bán", true, profile.FarmName);
                await _emailService.SendEmailAsync(user.Email, "[VAM Marketplace] Hồ sơ Người bán đã được phê duyệt 🎉", htmlContent);
            }
            else
            {
                user.Status = UserStatus.inactive.ToString();
                _unitOfWork.Users.Update(user);
                await _unitOfWork.CompleteAsync();
                var htmlContent = BuildEmailTemplate(user.Name ?? user.Email, "Hồ sơ Người bán", false, profile.FarmName);
                await _emailService.SendEmailAsync(user.Email, "[VAM Marketplace] Thông báo về Hồ sơ Người bán ⚠️", htmlContent);
            }
        }

        public async Task ApproveBusinessProfileAsync(int profileId, int adminId, ApproveProfileDto dto)
        {
            var profile = await _unitOfWork.BusinessProfiles.GetByIdAsync(profileId);
            if (profile == null) throw new Exception("Profile not found");

            var user = await _unitOfWork.Users.GetByIdAsync(profile.UserId);
            if (user == null) throw new Exception("User not found");

            profile.Status = dto.IsApproved ? ProfileStatus.APPROVED : ProfileStatus.REJECTED;
            profile.VerifiedById = adminId;
            profile.VerifiedAt = DateTimeOffset.UtcNow;

            _unitOfWork.BusinessProfiles.Update(profile);

            if (dto.IsApproved)
            {
                user.Role = UserRole.buyer.ToString();
                user.Status = UserStatus.active.ToString();
                _unitOfWork.Users.Update(user);
                await _unitOfWork.CompleteAsync();
                var htmlContent = BuildEmailTemplate(user.Name ?? user.Email, "Hồ sơ Doanh nghiệp", true, profile.CompanyName);
                await _emailService.SendEmailAsync(user.Email, "[VAM Marketplace] Hồ sơ Doanh nghiệp đã được phê duyệt 🎉", htmlContent);
            }
            else
            {
                user.Status = UserStatus.inactive.ToString();
                _unitOfWork.Users.Update(user);
                await _unitOfWork.CompleteAsync();
                var htmlContent = BuildEmailTemplate(user.Name ?? user.Email, "Hồ sơ Doanh nghiệp", false, profile.CompanyName);
                await _emailService.SendEmailAsync(user.Email, "[VAM Marketplace] Thông báo về Hồ sơ Doanh nghiệp ⚠️", htmlContent);
            }
        }

        private static string BuildEmailTemplate(string recipientName, string profileType, bool isApproved, string entityName)
        {
            string statusColor = isApproved ? "#10b981" : "#ef4444";
            string statusBg = isApproved ? "#ecfdf5" : "#fef2f2";
            string statusBadge = isApproved ? "ĐÃ ĐƯỢC PHÊ DUYỆT" : "CHƯA ĐƯỢC PHÊ DUYỆT";
            string icon = isApproved ? "✓" : "✕";
            string title = isApproved
                ? $"Chúc mừng! {profileType} của bạn đã được duyệt thành công."
                : $"Thông báo về việc xét duyệt {profileType} của bạn.";

            string bodyText = isApproved
                ? $"Chúng tôi xin trân trọng thông báo {profileType.ToLower()} <strong>{entityName}</strong> của bạn trên hệ thống VAM Marketplace đã vượt qua kiểm duyệt. Bạn hiện đã có đầy đủ quyền hạn để đăng tải sản phẩm, thực hiện giao dịch và tiếp cận mạng lưới khách hàng thủy hải sản trên toàn quốc."
                : $"Rất tiếc, {profileType.ToLower()} <strong>{entityName}</strong> của bạn hiện chưa đáp ứng đủ các tiêu chuẩn kiểm duyệt của hệ thống. Bạn vui lòng kiểm tra lại thông tin, giấy tờ chứng nhận và thực hiện cập nhật lại trên trang cá nhân.";

            string ctaText = isApproved ? "Truy Cập Hệ Thống Ngay" : "Cập Nhật Hồ Sơ";
            string ctaUrl = "http://localhost:5173"; // React App frontend URL

            return $@"
<!DOCTYPE html>
<html lang=""vi"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>VAM Marketplace Notification</title>
</head>
<body style=""margin: 0; padding: 0; font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; background-color: #f1f5f9; color: #334155;"">
    <table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"" style=""padding: 40px 10px;"">
        <tr>
            <td align=""center"">
                <table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"" style=""max-width: 600px; background-color: #ffffff; border-radius: 16px; overflow: hidden; box-shadow: 0 10px 25px -5px rgba(0, 0, 0, 0.05), 0 8px 10px -6px rgba(0, 0, 0, 0.05);"">
                    
                    <!-- Header -->
                    <tr>
                        <td style=""background: linear-gradient(135deg, #0f172a 0%, #1e3a8a 50%, #0284c7 100%); padding: 36px 30px; text-align: center;"">
                            <h1 style=""margin: 0; color: #ffffff; font-size: 28px; font-weight: 800; letter-spacing: 1px; text-transform: uppercase;"">
                                🌊 VAM MARKETPLACE
                            </h1>
                            <p style=""margin: 6px 0 0 0; color: #93c5fd; font-size: 14px; font-weight: 500;"">
                                Sàn Giao Dịch Thủy Hải Sản Việt Nam
                            </p>
                        </td>
                    </tr>

                    <!-- Status Banner -->
                    <tr>
                        <td style=""padding: 30px 40px 10px 40px; text-align: center;"">
                            <div style=""display: inline-block; background-color: {statusBg}; border: 2px solid {statusColor}; border-radius: 50px; padding: 8px 24px; margin-bottom: 20px;"">
                                <span style=""color: {statusColor}; font-weight: 800; font-size: 14px; letter-spacing: 0.5px;"">
                                    {icon} {statusBadge}
                                </span>
                            </div>
                            <h2 style=""margin: 0 0 16px 0; color: #0f172a; font-size: 20px; font-weight: 700; line-height: 1.4;"">
                                {title}
                            </h2>
                        </td>
                    </tr>

                    <!-- Content Body -->
                    <tr>
                        <td style=""padding: 0 40px 30px 40px; line-height: 1.7; color: #475569; font-size: 15px;"">
                            <p style=""margin-top: 0;"">Xin chào <strong>{recipientName}</strong>,</p>
                            <p>{bodyText}</p>

                            <!-- Highlight Card -->
                            <div style=""background-color: #f8fafc; border-left: 4px solid {statusColor}; border-radius: 6px; padding: 16px 20px; margin: 24px 0;"">
                                <p style=""margin: 0; font-size: 14px; color: #334155;"">
                                    <strong>Đối tượng xét duyệt:</strong> {profileType}<br>
                                    <strong>Tên đơn vị / Trang trại:</strong> {entityName}<br>
                                    <strong>Thời gian:</strong> {DateTime.Now:dd/MM/yyyy HH:mm}
                                </p>
                            </div>

                            <!-- CTA Button -->
                            <div style=""text-align: center; margin-top: 32px; margin-bottom: 20px;"">
                                <a href=""{ctaUrl}"" target=""_blank"" style=""background: linear-gradient(135deg, #0284c7 0%, #0369a1 100%); color: #ffffff; text-decoration: none; padding: 14px 32px; border-radius: 8px; font-weight: 700; font-size: 15px; display: inline-block; box-shadow: 0 4px 12px rgba(2, 132, 199, 0.35);"">
                                    {ctaText}
                                </a>
                            </div>
                        </td>
                    </tr>

                    <!-- Divider -->
                    <tr>
                        <td style=""padding: 0 40px;"">
                            <hr style=""border: none; border-top: 1px solid #e2e8f0; margin: 0;"">
                        </td>
                    </tr>

                    <!-- Footer -->
                    <tr>
                        <td style=""padding: 30px 40px; text-align: center; background-color: #fafafa; color: #94a3b8; font-size: 13px; line-height: 1.6;"">
                            <p style=""margin: 0 0 8px 0;"">Nếu bạn có bất kỳ thắc mắc nào, vui lòng liên hệ bộ phận hỗ trợ của chúng tôi.</p>
                            <p style=""margin: 0; font-weight: 600; color: #64748b;"">&copy; {DateTime.Now.Year} VAM System. All rights reserved.</p>
                        </td>
                    </tr>

                </table>
            </td>
        </tr>
    </table>
</body>
</html>";
        }
    }
}
