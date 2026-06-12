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

        public ProfileService(IUnitOfWork unitOfWork, IMapper mapper, IEmailService emailService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _emailService = emailService;
        }

        public async Task<SellerProfileDto> CreateSellerProfileAsync(int userId, CreateSellerProfileDto dto)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null) throw new Exception("User not found");

            var existingProfiles = await _unitOfWork.SellerProfiles.FindAsync(p => p.UserId == userId);
            if (existingProfiles.Any()) throw new Exception("Seller profile already exists");

            var profile = new SellerProfile
            {
                UserId = userId,
                FarmName = dto.FarmName,
                FarmAddress = dto.FarmAddress,
                AquacultureType = dto.AquacultureType,
                Certificate = dto.Certificate,
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

            var profile = new BusinessProfile
            {
                UserId = userId,
                CompanyName = dto.CompanyName,
                TaxCode = dto.TaxCode,
                BusinessLicense = dto.BusinessLicense,
                Address = dto.Address,
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
                _unitOfWork.Users.Update(user);
                await _unitOfWork.CompleteAsync();
                await _emailService.SendEmailAsync(user.Email, "Seller Profile Approved", "Congratulations! Your seller profile has been approved.");
            }
            else
            {
                await _unitOfWork.CompleteAsync();
                await _emailService.SendEmailAsync(user.Email, "Seller Profile Rejected", "We are sorry, your seller profile has been rejected.");
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
                _unitOfWork.Users.Update(user);
                await _unitOfWork.CompleteAsync();
                await _emailService.SendEmailAsync(user.Email, "Business Profile Approved", "Congratulations! Your business profile has been approved.");
            }
            else
            {
                await _unitOfWork.CompleteAsync();
                await _emailService.SendEmailAsync(user.Email, "Business Profile Rejected", "We are sorry, your business profile has been rejected.");
            }
        }
    }
}
