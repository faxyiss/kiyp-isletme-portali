using Microsoft.EntityFrameworkCore;
using StokAppApi.Entities;
using StokAppApi.Entities.Dto;

namespace StokAppApi.Services
{
	public interface ICategoryService
	{
		Task<CategoryResponseDto> CreateCategoryAsync(Guid userId, CategoryCreateDto dto);
		Task<List<CategoryResponseDto>> GetMyCategoriesAsync(Guid userId, string? searchText = null);
	}
	public class CategoryService : ICategoryService
	{
		private readonly AppDbContext _context;

		public CategoryService(AppDbContext context)
		{
			_context = context;
		}

		public async Task<CategoryResponseDto> CreateCategoryAsync(Guid userId, CategoryCreateDto dto)
		{
			var category = new Category
			{
				UserId = userId,
				Name = dto.Name,
				Type = dto.Type, // Cuk oturdu! İki taraf da CategoryType enum'ı oldu
				CreatedAt = DateTime.UtcNow,
				UpdatedAt = DateTime.UtcNow
			};

			await _context.Categories.AddAsync(category);
			await _context.SaveChangesAsync();

			return new CategoryResponseDto
			{
				Id = category.Id,
				Name = category.Name,
				Type = category.Type
			};
		}

		public async Task<List<CategoryResponseDto>> GetMyCategoriesAsync(Guid userId, string? searchText = null)
		{
			// 1. Sadece giriş yapan kullanıcıya ait kategorileri hedef alıyoruz
			var queryable = _context.Categories
				.Where(c => c.UserId == userId)
				.AsQueryable();

			// 2. Eğer arama kelimesi gönderildiyse isme göre filtre çakıyoruz
			if (!string.IsNullOrWhiteSpace(searchText))
			{
				string search = searchText.Trim().ToLower();
				queryable = queryable.Where(c => c.Name.ToLower().Contains(search));
			}

			// 3. Kategorileri alfabetik sıraya dizip DTO formatında listeliyoruz
			return await queryable
				.OrderBy(c => c.Name)
				.Select(c => new CategoryResponseDto
				{
					Id = c.Id,
					Name = c.Name,
					Type = c.Type
				})
				.ToListAsync();
		}
	}
}
