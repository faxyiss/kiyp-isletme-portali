using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StokAppApi.Entities;
using StokAppApi.Entities.Dto.Product;
using StokAppApi.Services;
using System.Security.Claims;

namespace StokAppApi.Controllers
{
	[Authorize]
	[ApiController]
	[Route("api/[controller]")]
	public class ProductsController : ControllerBase
	{
		private readonly IProductService _productService;

		public ProductsController(IProductService productService)
		{
			_productService = productService;
		}

		[HttpGet("search")]
		public async Task<IActionResult> SearchProducts([FromQuery] ProductQueryDto query)
		{
			// Token içerisinden giriş yapmış olan kullanıcının ID'sini çekiyoruz
			var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

			// Servis katmanındaki o güçlü LINQ zincirini tetikliyoruz
			var result = await _productService.SearchProductsAsync(userId, query);

			// Sonucu 200 OK ile frontend'e paket halinde teslim ediyoruz
			return Ok(result);
		}
		// Ürün Kartı Tanımlama
		[HttpPost("create")]
		public async Task<IActionResult> CreateProduct([FromBody] ProductCreateDto dto)
		{
			var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
			var productId = await _productService.CreateProductAsync(userId, dto);

			return Ok(new { message = "Ürün kartı başarıyla oluşturuldu.", id = productId });
		}

		// Ürün Kartı Güncelleme
		[HttpPut("update/{id}")]
		public async Task<IActionResult> UpdateProduct(Guid id, [FromBody] ProductUpdateDto dto)
		{
			var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
			var result = await _productService.UpdateProductAsync(userId, id, dto);

			if (!result) return NotFound(new { message = "Güncellenmek istenen ürün bulunamadı veya yetkiniz yok." });

			return Ok(new { message = "Ürün bilgileri başarıyla güncellendi." });
		}

		// Ürün Kartı Silme
		[HttpDelete("delete/{id}")]
		public async Task<IActionResult> DeleteProduct(Guid id)
		{
			var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
			var result = await _productService.DeleteProductAsync(userId, id);

			if (!result.Success)
			{
				return BadRequest(new { message = result.Message });
			}

			return Ok(new { message = result.Message });
		}

		[HttpGet("sales-terminal")]
		public async Task<IActionResult> GetSalesTerminalProducts([FromQuery] ProductSalesQueryDto query)
		{
			// Giriş yapan kullanıcının ID'sini token'dan çek
			var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

			// Filtreli ve sayfalamalı ürün listesini getir
			var result = await _productService.GetSalesTerminalProductsAsync(userId, query);

			return Ok(result);
		}

		[HttpGet("raw-materials")]
		public async Task<IActionResult> GetRawMaterials([FromQuery] ProductQueryDto query)
		{
			// Giriş yapan kullanıcının ID'sini çek
			var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

			// Hammadde listesini servisten al
			var result = await _productService.GetRawMaterialsAsync(userId, query);

			return Ok(result);
		}
	}
}
