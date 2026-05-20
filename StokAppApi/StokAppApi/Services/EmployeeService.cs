using Microsoft.EntityFrameworkCore;
using StokAppApi.Entities;
using StokAppApi.Entities.Dto.Employee;

namespace StokAppApi.Services
{
	public interface IEmployeeService
	{
		// Personel
		Task<List<EmployeeResponseDto>> GetEmployeesAsync(Guid userId);
		Task<EmployeeResponseDto> CreateEmployeeAsync(Guid userId, EmployeeCreateDto dto);
		Task<EmployeeResponseDto> UpdateEmployeeAsync(Guid userId, Guid employeeId, EmployeeUpdateDto dto);
		Task<bool> DeleteEmployeeAsync(Guid userId, Guid employeeId);

		// Maaş
		Task<SalaryPaymentResponseDto> CreateSalaryPaymentAsync(Guid userId, Guid employeeId, SalaryPaymentCreateDto dto);
		Task<List<SalaryPaymentResponseDto>> GetSalaryPaymentsAsync(Guid userId, Guid employeeId);
		Task<bool> DeleteSalaryPaymentAsync(Guid userId, Guid paymentId);
		SalaryCalculationDto CalculateSalary(decimal grossSalary);

		// İzin
		Task<LeaveResponseDto> CreateLeaveAsync(Guid userId, Guid employeeId, LeaveCreateDto dto);
		Task<List<LeaveResponseDto>> GetLeavesAsync(Guid userId, Guid employeeId);
		Task<bool> DeleteLeaveAsync(Guid userId, Guid leaveId);

		// Özet
		Task<EmployeeSummaryDto> GetSummaryAsync(Guid userId, int month, int year);
	}

	public class EmployeeService : IEmployeeService
	{
		private readonly AppDbContext _context;

		// ── Türkiye 2025 SGK / Vergi Oranları ─────────────────
		private const decimal EmployeeSGKRate = 0.14m;          // Çalışan SGK primi
		private const decimal EmployeeUnemploymentRate = 0.01m; // Çalışan işsizlik
		private const decimal IncomeTaxRate = 0.15m;            // Gelir vergisi (1. dilim)
		private const decimal StampTaxRate = 0.00759m;          // Damga vergisi
		private const decimal EmployerSGKRate = 0.205m;         // İşveren SGK primi
		private const decimal EmployerUnemploymentRate = 0.02m; // İşveren işsizlik

		public EmployeeService(AppDbContext context)
		{
			_context = context;
		}

		// ── PERSONEL ──────────────────────────────────────────

		public async Task<List<EmployeeResponseDto>> GetEmployeesAsync(Guid userId)
		{
			var employees = await _context.Employees
				.Where(e => e.UserId == userId)
				.OrderBy(e => e.FullName)
				.ToListAsync();

			return employees.Select(e => MapToEmployeeDto(e)).ToList();
		}

		public async Task<EmployeeResponseDto> CreateEmployeeAsync(Guid userId, EmployeeCreateDto dto)
		{
			var employee = new Employee
			{
				UserId = userId,
				FullName = dto.FullName,
				Position = dto.Position,
				Phone = dto.Phone,
				StartDate = dto.StartDate.ToUniversalTime(),
				GrossSalary = dto.GrossSalary,
				IsActive = dto.IsActive
			};

			_context.Employees.Add(employee);
			await _context.SaveChangesAsync();
			return MapToEmployeeDto(employee);
		}

		public async Task<EmployeeResponseDto> UpdateEmployeeAsync(Guid userId, Guid employeeId, EmployeeUpdateDto dto)
		{
			var employee = await _context.Employees
				.FirstOrDefaultAsync(e => e.Id == employeeId && e.UserId == userId);

			if (employee == null)
				throw new Exception("Personel bulunamadı.");

			employee.FullName = dto.FullName;
			employee.Position = dto.Position;
			employee.Phone = dto.Phone;
			employee.StartDate = dto.StartDate.ToUniversalTime();
			employee.GrossSalary = dto.GrossSalary;
			employee.IsActive = dto.IsActive;
			employee.UpdatedAt = DateTime.UtcNow;

			await _context.SaveChangesAsync();
			return MapToEmployeeDto(employee);
		}

		public async Task<bool> DeleteEmployeeAsync(Guid userId, Guid employeeId)
		{
			var employee = await _context.Employees
				.FirstOrDefaultAsync(e => e.Id == employeeId && e.UserId == userId);

			if (employee == null) return false;

			_context.Employees.Remove(employee);
			await _context.SaveChangesAsync();
			return true;
		}

		// ── MAAŞ ──────────────────────────────────────────────

		public SalaryCalculationDto CalculateSalary(decimal grossSalary)
		{
			// Çalışan kesintileri
			var employeeSGK = Math.Round(grossSalary * EmployeeSGKRate, 2);
			var employeeUnemployment = Math.Round(grossSalary * EmployeeUnemploymentRate, 2);

			// Gelir vergisi matrahı = Brüt - SGK - İşsizlik
			var incomeTaxBase = grossSalary - employeeSGK - employeeUnemployment;
			var incomeTax = Math.Round(incomeTaxBase * IncomeTaxRate, 2);

			// Damga vergisi brüt üzerinden
			var stampTax = Math.Round(grossSalary * StampTaxRate, 2);

			var totalEmployeeDeductions = employeeSGK + employeeUnemployment + incomeTax + stampTax;
			var netSalary = Math.Round(grossSalary - totalEmployeeDeductions, 2);

			// İşveren ek maliyetleri
			var employerSGK = Math.Round(grossSalary * EmployerSGKRate, 2);
			var employerUnemployment = Math.Round(grossSalary * EmployerUnemploymentRate, 2);

			// Toplam işveren maliyeti = Brüt + İşveren SGK + İşveren İşsizlik
			var totalEmployerCost = Math.Round(grossSalary + employerSGK + employerUnemployment, 2);

			return new SalaryCalculationDto
			{
				GrossSalary = grossSalary,
				EmployeeSGK = employeeSGK,
				EmployeeUnemployment = employeeUnemployment,
				IncomeTax = incomeTax,
				StampTax = stampTax,
				TotalEmployeeDeductions = totalEmployeeDeductions,
				NetSalary = netSalary,
				EmployerSGK = employerSGK,
				EmployerUnemployment = employerUnemployment,
				TotalEmployerCost = totalEmployerCost
			};
		}

		public async Task<SalaryPaymentResponseDto> CreateSalaryPaymentAsync(Guid userId, Guid employeeId, SalaryPaymentCreateDto dto)
		{
			var employee = await _context.Employees
				.FirstOrDefaultAsync(e => e.Id == employeeId && e.UserId == userId);

			if (employee == null)
				throw new Exception("Personel bulunamadı.");

			// Aynı ay tekrar ödeme yapılmasın
			var alreadyPaid = await _context.SalaryPayments
				.AnyAsync(p => p.EmployeeId == employeeId && p.Month == dto.Month && p.Year == dto.Year);

			if (alreadyPaid)
				throw new Exception($"Bu personele {dto.Month}/{dto.Year} dönemi için zaten maaş ödemesi yapılmış.");

			// Brüt girilmemişse personelin mevcut maaşını kullan
			var gross = dto.GrossSalary ?? employee.GrossSalary;
			var calc = CalculateSalary(gross);

			var payment = new SalaryPayment
			{
				EmployeeId = employeeId,
				Month = dto.Month,
				Year = dto.Year,
				GrossSalary = calc.GrossSalary,
				NetSalary = calc.NetSalary,
				EmployeeSGK = calc.EmployeeSGK,
				EmployeeUnemployment = calc.EmployeeUnemployment,
				IncomeTax = calc.IncomeTax,
				StampTax = calc.StampTax,
				EmployerSGK = calc.EmployerSGK,
				EmployerUnemployment = calc.EmployerUnemployment,
				TotalEmployerCost = calc.TotalEmployerCost,
				Notes = dto.Notes
			};

			_context.SalaryPayments.Add(payment);
			await _context.SaveChangesAsync();

			return MapToPaymentDto(payment, employee.FullName);
		}

		public async Task<List<SalaryPaymentResponseDto>> GetSalaryPaymentsAsync(Guid userId, Guid employeeId)
		{
			var employee = await _context.Employees
				.FirstOrDefaultAsync(e => e.Id == employeeId && e.UserId == userId);

			if (employee == null) throw new Exception("Personel bulunamadı.");

			var payments = await _context.SalaryPayments
				.Where(p => p.EmployeeId == employeeId)
				.OrderByDescending(p => p.Year)
				.ThenByDescending(p => p.Month)
				.ToListAsync();

			return payments.Select(p => MapToPaymentDto(p, employee.FullName)).ToList();
		}

		public async Task<bool> DeleteSalaryPaymentAsync(Guid userId, Guid paymentId)
		{
			var payment = await _context.SalaryPayments
				.Include(p => p.Employee)
				.FirstOrDefaultAsync(p => p.Id == paymentId && p.Employee!.UserId == userId);

			if (payment == null) return false;

			_context.SalaryPayments.Remove(payment);
			await _context.SaveChangesAsync();
			return true;
		}

		// ── İZİN ──────────────────────────────────────────────

		public async Task<LeaveResponseDto> CreateLeaveAsync(Guid userId, Guid employeeId, LeaveCreateDto dto)
		{
			var employee = await _context.Employees
				.FirstOrDefaultAsync(e => e.Id == employeeId && e.UserId == userId);

			if (employee == null) throw new Exception("Personel bulunamadı.");

			if (dto.EndDate < dto.StartDate)
				throw new Exception("Bitiş tarihi başlangıç tarihinden önce olamaz.");

			var dayCount = (int)(dto.EndDate.Date - dto.StartDate.Date).TotalDays + 1;

			var leave = new LeaveRecord
			{
				EmployeeId = employeeId,
				StartDate = dto.StartDate.ToUniversalTime(),
				EndDate = dto.EndDate.ToUniversalTime(),
				DayCount = dayCount,
				Reason = dto.Reason
			};

			_context.LeaveRecords.Add(leave);
			await _context.SaveChangesAsync();

			return MapToLeaveDto(leave);
		}

		public async Task<List<LeaveResponseDto>> GetLeavesAsync(Guid userId, Guid employeeId)
		{
			var employee = await _context.Employees
				.FirstOrDefaultAsync(e => e.Id == employeeId && e.UserId == userId);

			if (employee == null) throw new Exception("Personel bulunamadı.");

			var leaves = await _context.LeaveRecords
				.Where(l => l.EmployeeId == employeeId)
				.OrderByDescending(l => l.StartDate)
				.ToListAsync();

			return leaves.Select(MapToLeaveDto).ToList();
		}

		public async Task<bool> DeleteLeaveAsync(Guid userId, Guid leaveId)
		{
			var leave = await _context.LeaveRecords
				.Include(l => l.Employee)
				.FirstOrDefaultAsync(l => l.Id == leaveId && l.Employee!.UserId == userId);

			if (leave == null) return false;

			_context.LeaveRecords.Remove(leave);
			await _context.SaveChangesAsync();
			return true;
		}

		// ── ÖZET ──────────────────────────────────────────────

		public async Task<EmployeeSummaryDto> GetSummaryAsync(Guid userId, int month, int year)
		{
			var employees = await _context.Employees
				.Where(e => e.UserId == userId)
				.ToListAsync();

			var payments = await _context.SalaryPayments
				.Include(p => p.Employee)
				.Where(p => p.Employee!.UserId == userId && p.Month == month && p.Year == year)
				.ToListAsync();

			var leaves = await _context.LeaveRecords
				.Include(l => l.Employee)
				.Where(l => l.Employee!.UserId == userId &&
					l.StartDate.Month == month && l.StartDate.Year == year)
				.ToListAsync();

			var totalTaxSGK = payments.Sum(p =>
				p.EmployeeSGK + p.EmployeeUnemployment + p.IncomeTax + p.StampTax +
				p.EmployerSGK + p.EmployerUnemployment);

			return new EmployeeSummaryDto
			{
				TotalEmployees = employees.Count,
				ActiveEmployees = employees.Count(e => e.IsActive),
				TotalGrossSalary = payments.Sum(p => p.GrossSalary),
				TotalNetSalary = payments.Sum(p => p.NetSalary),
				TotalEmployerCost = payments.Sum(p => p.TotalEmployerCost),
				TotalTaxAndSGK = totalTaxSGK,
				TotalLeaveDays = leaves.Sum(l => l.DayCount)
			};
		}

		// ── YARDIMCI ──────────────────────────────────────────

		private EmployeeResponseDto MapToEmployeeDto(Employee e)
		{
			var calc = CalculateSalary(e.GrossSalary);
			return new EmployeeResponseDto
			{
				Id = e.Id,
				FullName = e.FullName,
				Position = e.Position,
				Phone = e.Phone,
				StartDate = e.StartDate,
				GrossSalary = e.GrossSalary,
				NetSalary = calc.NetSalary,
				TotalEmployerCost = calc.TotalEmployerCost,
				IsActive = e.IsActive,
				CreatedAt = e.CreatedAt
			};
		}

		private static SalaryPaymentResponseDto MapToPaymentDto(SalaryPayment p, string fullName)
		{
			var turkishMonths = new[] { "", "Ocak", "Şubat", "Mart", "Nisan", "Mayıs", "Haziran",
										"Temmuz", "Ağustos", "Eylül", "Ekim", "Kasım", "Aralık" };
			return new SalaryPaymentResponseDto
			{
				Id = p.Id,
				EmployeeId = p.EmployeeId,
				EmployeeFullName = fullName,
				Month = p.Month,
				Year = p.Year,
				MonthLabel = $"{turkishMonths[p.Month]} {p.Year}",
				GrossSalary = p.GrossSalary,
				NetSalary = p.NetSalary,
				EmployeeSGK = p.EmployeeSGK,
				EmployeeUnemployment = p.EmployeeUnemployment,
				IncomeTax = p.IncomeTax,
				StampTax = p.StampTax,
				EmployerSGK = p.EmployerSGK,
				EmployerUnemployment = p.EmployerUnemployment,
				TotalEmployerCost = p.TotalEmployerCost,
				Notes = p.Notes,
				PaidAt = p.PaidAt
			};
		}

		private static LeaveResponseDto MapToLeaveDto(LeaveRecord l) => new()
		{
			Id = l.Id,
			EmployeeId = l.EmployeeId,
			StartDate = l.StartDate,
			EndDate = l.EndDate,
			DayCount = l.DayCount,
			Reason = l.Reason,
			CreatedAt = l.CreatedAt
		};
	}
}