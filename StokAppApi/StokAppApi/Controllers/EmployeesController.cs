using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StokAppApi.Entities.Dto.Employee;
using StokAppApi.Services;
using System.Security.Claims;

namespace StokAppApi.Controllers
{
	[Authorize]
	[ApiController]
	[Route("api/[controller]")]
	public class EmployeesController : ControllerBase
	{
		private readonly IEmployeeService _employeeService;

		public EmployeesController(IEmployeeService employeeService)
		{
			_employeeService = employeeService;
		}

		private Guid GetUserId()
		{
			var str = User.FindFirstValue(ClaimTypes.NameIdentifier);
			if (string.IsNullOrEmpty(str)) throw new UnauthorizedAccessException();
			return Guid.Parse(str);
		}

		// ── PERSONEL ──────────────────────────────────────────────────

		// GET /api/Employees
		[HttpGet]
		public async Task<IActionResult> GetEmployees()
		{
			try
			{
				var userId = GetUserId();
				var result = await _employeeService.GetEmployeesAsync(userId);
				return Ok(result);
			}
			catch (Exception ex)
			{
				return BadRequest(new { message = ex.Message });
			}
		}

		// POST /api/Employees
		[HttpPost]
		public async Task<IActionResult> CreateEmployee([FromBody] EmployeeCreateDto dto)
		{
			try
			{
				var userId = GetUserId();
				var result = await _employeeService.CreateEmployeeAsync(userId, dto);
				return Ok(new { message = "Personel başarıyla eklendi.", employee = result });
			}
			catch (Exception ex)
			{
				return BadRequest(new { message = ex.Message });
			}
		}

		// PUT /api/Employees/{id}
		[HttpPut("{id}")]
		public async Task<IActionResult> UpdateEmployee(Guid id, [FromBody] EmployeeUpdateDto dto)
		{
			try
			{
				var userId = GetUserId();
				var result = await _employeeService.UpdateEmployeeAsync(userId, id, dto);
				return Ok(new { message = "Personel bilgileri güncellendi.", employee = result });
			}
			catch (Exception ex)
			{
				return BadRequest(new { message = ex.Message });
			}
		}

		// DELETE /api/Employees/{id}
		[HttpDelete("{id}")]
		public async Task<IActionResult> DeleteEmployee(Guid id)
		{
			try
			{
				var userId = GetUserId();
				var result = await _employeeService.DeleteEmployeeAsync(userId, id);
				if (!result) return NotFound(new { message = "Personel bulunamadı." });
				return Ok(new { message = "Personel silindi." });
			}
			catch (Exception ex)
			{
				return BadRequest(new { message = ex.Message });
			}
		}

		// ── MAAŞ ──────────────────────────────────────────────────────

		// GET /api/Employees/calculate?gross=10000
		// Anlık hesaplama — kayıt yapmaz, sadece preview için
		[HttpGet("calculate")]
		public IActionResult Calculate([FromQuery] decimal gross)
		{
			try
			{
				if (gross <= 0) return BadRequest(new { message = "Geçerli bir brüt maaş girin." });
				var result = _employeeService.CalculateSalary(gross);
				return Ok(result);
			}
			catch (Exception ex)
			{
				return BadRequest(new { message = ex.Message });
			}
		}

		// GET /api/Employees/{id}/payments
		[HttpGet("{id}/payments")]
		public async Task<IActionResult> GetPayments(Guid id)
		{
			try
			{
				var userId = GetUserId();
				var result = await _employeeService.GetSalaryPaymentsAsync(userId, id);
				return Ok(result);
			}
			catch (Exception ex)
			{
				return BadRequest(new { message = ex.Message });
			}
		}

		// POST /api/Employees/{id}/payments
		[HttpPost("{id}/payments")]
		public async Task<IActionResult> CreatePayment(Guid id, [FromBody] SalaryPaymentCreateDto dto)
		{
			try
			{
				var userId = GetUserId();
				var result = await _employeeService.CreateSalaryPaymentAsync(userId, id, dto);
				return Ok(new { message = "Maaş ödemesi kaydedildi.", payment = result });
			}
			catch (Exception ex)
			{
				return BadRequest(new { message = ex.Message });
			}
		}

		// DELETE /api/Employees/payments/{paymentId}
		[HttpDelete("payments/{paymentId}")]
		public async Task<IActionResult> DeletePayment(Guid paymentId)
		{
			try
			{
				var userId = GetUserId();
				var result = await _employeeService.DeleteSalaryPaymentAsync(userId, paymentId);
				if (!result) return NotFound(new { message = "Ödeme kaydı bulunamadı." });
				return Ok(new { message = "Ödeme kaydı silindi." });
			}
			catch (Exception ex)
			{
				return BadRequest(new { message = ex.Message });
			}
		}

		// ── İZİN ──────────────────────────────────────────────────────

		// GET /api/Employees/{id}/leaves
		[HttpGet("{id}/leaves")]
		public async Task<IActionResult> GetLeaves(Guid id)
		{
			try
			{
				var userId = GetUserId();
				var result = await _employeeService.GetLeavesAsync(userId, id);
				return Ok(result);
			}
			catch (Exception ex)
			{
				return BadRequest(new { message = ex.Message });
			}
		}

		// POST /api/Employees/{id}/leaves
		[HttpPost("{id}/leaves")]
		public async Task<IActionResult> CreateLeave(Guid id, [FromBody] LeaveCreateDto dto)
		{
			try
			{
				var userId = GetUserId();
				var result = await _employeeService.CreateLeaveAsync(userId, id, dto);
				return Ok(new { message = "İzin kaydı oluşturuldu.", leave = result });
			}
			catch (Exception ex)
			{
				return BadRequest(new { message = ex.Message });
			}
		}

		// DELETE /api/Employees/leaves/{leaveId}
		[HttpDelete("leaves/{leaveId}")]
		public async Task<IActionResult> DeleteLeave(Guid leaveId)
		{
			try
			{
				var userId = GetUserId();
				var result = await _employeeService.DeleteLeaveAsync(userId, leaveId);
				if (!result) return NotFound(new { message = "İzin kaydı bulunamadı." });
				return Ok(new { message = "İzin kaydı silindi." });
			}
			catch (Exception ex)
			{
				return BadRequest(new { message = ex.Message });
			}
		}

		// ── ÖZET ──────────────────────────────────────────────────────

		// GET /api/Employees/summary?month=5&year=2026
		[HttpGet("summary")]
		public async Task<IActionResult> GetSummary([FromQuery] int? month, [FromQuery] int? year)
		{
			try
			{
				var userId = GetUserId();
				var now = DateTime.UtcNow;
				var result = await _employeeService.GetSummaryAsync(
					userId,
					month ?? now.Month,
					year ?? now.Year
				);
				return Ok(result);
			}
			catch (Exception ex)
			{
				return BadRequest(new { message = ex.Message });
			}
		}
	}
}