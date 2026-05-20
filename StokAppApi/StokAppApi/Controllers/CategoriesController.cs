using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StokAppApi.Entities;
using StokAppApi.Entities.Dto;
using StokAppApi.Services;
using System.Security.Claims;

namespace StokAppApi.Controllers
{
	[Authorize] // Giriş yapmamış kullanıcılar istek atamaz
	[ApiController]
	[Route("api/[controller]")]
	public class CategoriesController : ControllerBase
	{
		private readonly ICategoryService _categoryService;

		public CategoriesController(ICategoryService categoryService)
		{
			_categoryService = categoryService;
		}

		// Yeni Kategori Ekleme Endpoint'i
		[HttpPost("create")]
		public async Task<IActionResult> CreateCategory([FromBody] CategoryCreateDto dto)
		{
			// Token'dan istek atan kullanıcının ID'sini cımbızlıyoruz
			var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

			var result = await _categoryService.CreateCategoryAsync(userId, dto);

			return Ok(new
			{
				message = "Kategori başarıyla oluşturuldu.",
				category = result
			});
		}

		[HttpGet]
		public async Task<IActionResult> GetCategories([FromQuery] string? searchText)
		{
			// Token'dan istek atan kullanıcının ID'sini çekiyoruz
			var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

			// Servisten listeyi talep ediyoruz
			var categories = await _categoryService.GetMyCategoriesAsync(userId, searchText);

			return Ok(categories);
		}
	}
}
