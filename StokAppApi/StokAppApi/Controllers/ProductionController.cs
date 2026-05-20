using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StokAppApi.Entities;
using StokAppApi.Entities.Dto.Production;
using StokAppApi.Services;
using System.Security.Claims;

namespace StokAppApi.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	[Authorize]
	public class ProductionController : ControllerBase
	{
		private readonly ProductionService _productionService;

		public ProductionController(ProductionService productionService)
		{
			_productionService = productionService;
		}

		[HttpPost("produce")]
		public async Task<IActionResult> ProduceProduct([FromBody] ProduceRequestDto request)
		{
			try
			{
				// JWT Token'dan aktif kullanıcının Guid'sini alıyoruz
				var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
				if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out Guid userId))
				{
					return Unauthorized(new { message = "Kullanıcı doğrulanamadı." });
				}

				await _productionService.ProduceAsync(request, userId);

				return Ok(new { message = "Üretim başarıyla tamamlandı ve stoklara eklendi." });
			}
			catch (Exception ex)
			{
				// Hata mesajını frontend'e döndür (Örn: "Yetersiz stok! Gerekli hammadde: Ahşap")
				return BadRequest(new { message = ex.Message });
			}
		}
		[HttpGet("recipes")]
		public async Task<IActionResult> GetRecipes()
		{
			try
			{
				var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
				if (!Guid.TryParse(userIdString, out Guid userId)) return Unauthorized();

				var recipes = await _productionService.GetRecipesAsync(userId);
				return Ok(recipes);
			}
			catch (Exception ex)
			{
				return BadRequest(new { message = ex.Message });
			}
		}

		[HttpPost("recipe")]
		public async Task<IActionResult> CreateRecipe([FromBody] RecipeCreateDto request)
		{
			try
			{
				var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
				if (!Guid.TryParse(userIdString, out Guid userId)) return Unauthorized();

				var createdRecipe = await _productionService.CreateRecipeAsync(request, userId);
				return Ok(new { message = "Reçete başarıyla oluşturuldu.", recipe = createdRecipe });
			}
			catch (Exception ex)
			{
				return BadRequest(new { message = ex.Message });
			}
		}

		[HttpDelete("recipe/{id}")]
		public async Task<IActionResult> DeleteRecipe(Guid id)
		{
			try
			{
				var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
				if (!Guid.TryParse(userIdString, out Guid userId)) return Unauthorized();

				await _productionService.DeleteRecipeAsync(id, userId);
				return Ok(new { message = "Reçete başarıyla silindi." });
			}
			catch (Exception ex)
			{
				return BadRequest(new { message = ex.Message });
			}
		}

		[HttpGet("logs")]
		public async Task<IActionResult> GetProductionLogs([FromQuery] ProductionLogQueryDto query)
		{
			try
			{
				var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
				if (!Guid.TryParse(userIdString, out Guid userId)) return Unauthorized();

				var result = await _productionService.GetProductionLogsAsync(userId, query);
				return Ok(result);
			}
			catch (Exception ex)
			{
				return BadRequest(new { message = ex.Message });
			}
		}
	}
}
