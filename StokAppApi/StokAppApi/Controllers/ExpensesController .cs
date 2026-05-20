using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StokAppApi.Entities;
using StokAppApi.Entities.Dto.Expense;
using StokAppApi.Services;
using System.Security.Claims;

namespace StokAppApi.Controllers
{

	[Authorize]
	[ApiController]
	[Route("api/[controller]")]
	public class ExpensesController : ControllerBase
	{
		private readonly IExpenseService _expenseService;

		public ExpensesController(IExpenseService expenseService)
		{
			_expenseService = expenseService;
		}

		private Guid GetUserId()
		{
			var str = User.FindFirstValue(ClaimTypes.NameIdentifier);
			if (string.IsNullOrEmpty(str)) throw new UnauthorizedAccessException();
			return Guid.Parse(str);
		}

		// ── KATEGORİ ──────────────────────────────────────────────────

		// GET /api/Expenses/categories
		[HttpGet("categories")]
		public async Task<IActionResult> GetCategories()
		{
			try
			{
				var userId = GetUserId();
				var result = await _expenseService.GetCategoriesAsync(userId);
				return Ok(result);
			}
			catch (Exception ex)
			{
				return BadRequest(new { message = ex.Message });
			}
		}

		// POST /api/Expenses/categories
		[HttpPost("categories")]
		public async Task<IActionResult> CreateCategory([FromBody] ExpenseCategoryCreateDto dto)
		{
			try
			{
				var userId = GetUserId();
				var result = await _expenseService.CreateCategoryAsync(userId, dto);
				return Ok(new { message = "Kategori başarıyla oluşturuldu.", category = result });
			}
			catch (Exception ex)
			{
				return BadRequest(new { message = ex.Message });
			}
		}

		// DELETE /api/Expenses/categories/{id}
		[HttpDelete("categories/{id}")]
		public async Task<IActionResult> DeleteCategory(Guid id)
		{
			try
			{
				var userId = GetUserId();
				var result = await _expenseService.DeleteCategoryAsync(userId, id);
				if (!result) return NotFound(new { message = "Kategori bulunamadı." });
				return Ok(new { message = "Kategori silindi." });
			}
			catch (Exception ex)
			{
				return BadRequest(new { message = ex.Message });
			}
		}

		// ── GİDER ─────────────────────────────────────────────────────

		// GET /api/Expenses?page=1&pageSize=10&month=5&year=2026
		[HttpGet]
		public async Task<IActionResult> GetExpenses([FromQuery] ExpenseQueryDto query)
		{
			try
			{
				var userId = GetUserId();
				var result = await _expenseService.GetExpensesAsync(userId, query);
				return Ok(result);
			}
			catch (Exception ex)
			{
				return BadRequest(new { message = ex.Message });
			}
		}

		// POST /api/Expenses
		[HttpPost]
		public async Task<IActionResult> CreateExpense([FromBody] ExpenseCreateDto dto)
		{
			try
			{
				var userId = GetUserId();
				var result = await _expenseService.CreateExpenseAsync(userId, dto);
				return Ok(new { message = "Gider başarıyla kaydedildi.", expense = result });
			}
			catch (Exception ex)
			{
				return BadRequest(new { message = ex.Message });
			}
		}

		// DELETE /api/Expenses/{id}
		[HttpDelete("{id}")]
		public async Task<IActionResult> DeleteExpense(Guid id)
		{
			try
			{
				var userId = GetUserId();
				var result = await _expenseService.DeleteExpenseAsync(userId, id);
				if (!result) return NotFound(new { message = "Gider kaydı bulunamadı." });
				return Ok(new { message = "Gider kaydı silindi." });
			}
			catch (Exception ex)
			{
				return BadRequest(new { message = ex.Message });
			}
		}

		// ── GRAFİK / ÖZET ─────────────────────────────────────────────

		// GET /api/Expenses/summary?year=2026
		[HttpGet("summary")]
		public async Task<IActionResult> GetMonthlySummary([FromQuery] int? year)
		{
			try
			{
				var userId = GetUserId();
				var targetYear = year ?? DateTime.UtcNow.Year;
				var result = await _expenseService.GetMonthlySummaryAsync(userId, targetYear);
				return Ok(result);
			}
			catch (Exception ex)
			{
				return BadRequest(new { message = ex.Message });
			}
		}
	}
}
