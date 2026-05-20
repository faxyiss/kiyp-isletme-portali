using Microsoft.EntityFrameworkCore;
using StokAppApi.Entities;
using StokAppApi.Entities.Dto;

namespace StokAppApi.Services
{
	public interface IBusinessProfileService
	{
		Task<bool> CheckIfExistsAsync(Guid userId);
		Task CreateProfileAsync(Guid userId, BusinessProfileCreateDto dto);
		Task<BusinessProfile?> GetProfileAsync(Guid userId);
		Task<bool> UpdateProfileAsync(Guid userId, ProfileUpdateDto dto);
	}

	public class BusinessProfileService : IBusinessProfileService
	{
		private readonly AppDbContext _context;

		public BusinessProfileService(AppDbContext context)
		{
			_context = context;
		}

		public async Task<bool> CheckIfExistsAsync(Guid userId)
		{
			return await _context.BusinessProfiles.AnyAsync(b => b.UserId == userId);
		}

		public async Task CreateProfileAsync(Guid userId, BusinessProfileCreateDto dto)
		{
			if (await CheckIfExistsAsync(userId))
				throw new Exception("Bu kullanıcının zaten bir işletme profili var.");

			var profile = new BusinessProfile
			{
				Id = Guid.NewGuid(),
				UserId = userId,
				CompanyName = dto.CompanyName,
				Phone = dto.Phone,
				ContactEmail = dto.ContactEmail,
				Address = dto.Address,
				TaxOffice = dto.TaxOffice,
				TaxNumber = dto.TaxNumber,
				CreatedAt = DateTime.UtcNow,
				UpdatedAt = DateTime.UtcNow
			};

			_context.BusinessProfiles.Add(profile);
			await _context.SaveChangesAsync();
		}
		public async Task<BusinessProfile?> GetProfileAsync(Guid userId)
		{
			// Include(b => b.User) sayesinde profile ait kullanıcının Ad, Rol gibi bilgilerini de tek sorguda çekiyoruz.
			return await _context.BusinessProfiles
				.Include(b => b.User)
				.FirstOrDefaultAsync(b => b.UserId == userId);
		}

		public async Task<bool> UpdateProfileAsync(Guid userId, ProfileUpdateDto dto)
		{
			// Include(b => b.User) ile tek sorguda hem profil kartını hem kullanıcı kartını kilitliyoruz moruq
			var profile = await _context.BusinessProfiles
				.Include(b => b.User)
				.FirstOrDefaultAsync(b => b.UserId == userId);

			if (profile == null) return false;

			// 1. İşletme Adını Güncelle
			profile.CompanyName = dto.CompanyName;
			profile.UpdatedAt = DateTime.UtcNow;

			// 2. Kullanıcının Ad Soyadını Güncelle
			if (profile.User != null)
			{
				profile.User.FullName = dto.FullName;
				profile.User.UpdatedAt = DateTime.UtcNow;
			}

			await _context.SaveChangesAsync();
			return true;
		}
	}
}
