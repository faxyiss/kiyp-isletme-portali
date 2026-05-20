using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StokAppApi.Entities.Dto.Stock;
using StokAppApi.Services;
using System.Security.Claims;

namespace StokAppApi.Controllers
{
	[Authorize]
	[ApiController]
	[Route("api/[controller]")]
	public class StocksController : ControllerBase
	{
		private readonly IStockService _stockService;

		public StocksController(IStockService stockService)
		{
			_stockService = stockService;
		}

		[HttpPost("inflow")]
		public async Task<IActionResult> StockInflow([FromBody] StockInflowDto dto)
		{
			var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
			var result = await _stockService.AddStockInflowAsync(userId, dto);

			if (!result) return BadRequest(new { message = "Stok girişi başarısız. Ürün bulunamadı veya yetkiniz yok." });
			return Ok(new { message = "Stok partisi başarıyla depoya kabul edildi moruq." });
		}

		// YENİ: Sayfalı Listeleme Endpoint'i (Varsayılan 1. sayfa, 10'ar adet)
		[HttpGet("product/{productId}")]
		public async Task<IActionResult> GetProductStocks(Guid productId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
		{
			var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
			var result = await _stockService.GetStockHistoryAsync(userId, productId, page, pageSize);
			return Ok(result);
		}

		// YENİ: Stok Silme Endpoint'i
		[HttpDelete("delete/{stockId}")]
		public async Task<IActionResult> DeleteStock(Guid stockId)
		{
			var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
			var result = await _stockService.DeleteStockAsync(userId, stockId);

			if (!result.Success) return BadRequest(new { message = result.Message });

			return Ok(new { message = result.Message });
		}

		[HttpPost("deduct")]
		public async Task<IActionResult> DeductStock([FromBody] DeductStockDto dto)
		{
			// Token'dan giriş yapan kullanıcının ID'sini alıyoruz
			var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

			// Servise yollayıp işlemi yaptırıyoruz
			var result = await _stockService.DeductStockAsync(userId, dto);

			// İşlem başarısızsa BadRequest (400) dönüyoruz ki frontend tarafında hata uyarısı versin
			if (!result.Success) return BadRequest(new { message = result.Message });

			// İşlem başarılıysa Ok (200) dönüyoruz
			return Ok(new { message = result.Message });
		}
	}
}
