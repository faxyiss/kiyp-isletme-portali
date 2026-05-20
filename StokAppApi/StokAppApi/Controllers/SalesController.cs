using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StokAppApi.Entities;
using StokAppApi.Entities.Dto.Sales;
using StokAppApi.Services;
using System.Security.Claims;

namespace StokAppApi.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	[Authorize] // Token ile giriş zorunlu
	public class SalesController : ControllerBase
	{
		private readonly ISaleService _saleService;

		public SalesController(ISaleService saleService)
		{
			_saleService = saleService;
		}

		// JWT içinden UserId'yi çeken yardımcı metod
		private Guid GetUserId()
		{
			var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
			if (Guid.TryParse(userIdStr, out Guid userId)) return userId;
			throw new UnauthorizedAccessException("Geçersiz kullanıcı token'ı.");
		}

		[HttpPost]
		public async Task<IActionResult> CreateSale([FromBody] CreateSaleDto dto)
		{
			try
			{
				var userId = GetUserId();
				var result = await _saleService.CreateSaleAsync(userId, dto);
				return Ok(new { Message = "Satış işlemi başarıyla gerçekleşti." });
			}
			catch (Exception ex)
			{
				return BadRequest(new { Message = ex.Message });
			}
		}

		[HttpGet]
		public async Task<IActionResult> GetSales()
		{
			try
			{
				var userId = GetUserId();
				var sales = await _saleService.GetSalesAsync(userId);
				return Ok(sales);
			}
			catch (Exception ex)
			{
				return BadRequest(new { Message = ex.Message });
			}
		}

		[HttpPost("bulk")]
		public async Task<IActionResult> CreateBulkSale([FromBody] List<CreateSaleDto> dtos)
		{
			try
			{
				var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
				await _saleService.CreateBulkSaleAsync(userId, dtos);
				return Ok(new { Message = "Toplu satış işlemi başarıyla gerçekleşti." });
			}
			catch (Exception ex)
			{
				return BadRequest(new { Message = ex.Message });
			}
		}

		[HttpGet("history")]
		public async Task<IActionResult> GetSalesHistory([FromQuery] SaleQueryDto query)
		{
			try
			{
				var userId = GetUserId();
				var result = await _saleService.GetSalesHistoryAsync(userId, query);
				return Ok(result);
			}
			catch (Exception ex)
			{
				return BadRequest(new { Message = ex.Message });
			}
		}

		[HttpGet("customer/{customerId}")]
		public async Task<IActionResult> GetCustomerPurchaseHistory(Guid customerId, [FromQuery] int page = 1, [FromQuery] int pageSize = 8)
		{
			try
			{
				// Token'dan giriş yapmış olan kullanıcının ID'sini alıyoruz
				var userId = GetUserId();

				var result = await _saleService.GetCustomerPurchaseHistoryAsync(userId, customerId, page, pageSize);
				return Ok(result);
			}
			catch (Exception ex)
			{
				return BadRequest(new { Message = ex.Message });
			}
		}

		// YENİ: Geçmiş Tarihli Satış (Dışarıdan tarih kabul eder)
		[HttpPost("historical")]
		public async Task<IActionResult> CreateHistoricalSale([FromBody] CreateHistoricalSaleDto dto)
		{
			try
			{
				var userId = GetUserId();
				await _saleService.CreateHistoricalSaleAsync(userId, dto);
				return Ok(new { Message = "Geçmiş tarihli satış işlemi başarıyla gerçekleşti." });
			}
			catch (Exception ex)
			{
				return BadRequest(new { Message = ex.Message });
			}
		}

		[HttpGet("category-summary")]
		public async Task<IActionResult> GetMonthlyCategorySales([FromQuery] int month, [FromQuery] int year)
		{
			try
			{
				var userId = GetUserId();
				var result = await _saleService.GetMonthlyCategorySalesAsync(userId, month, year);
				return Ok(result);
			}
			catch (Exception ex)
			{
				return BadRequest(new { Message = ex.Message });
			}
		}
	}
}
