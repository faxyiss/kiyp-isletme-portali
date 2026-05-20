using System.ComponentModel.DataAnnotations;

namespace StokAppApi.Entities.Dto.Employee
{
	// ── PERSONEL ──────────────────────────────────────────────

	public class EmployeeCreateDto
	{
		[Required]
		[MaxLength(150)]
		public string FullName { get; set; } = string.Empty;

		[Required]
		[MaxLength(100)]
		public string Position { get; set; } = string.Empty;

		[MaxLength(20)]
		public string? Phone { get; set; }

		[Required]
		public DateTime StartDate { get; set; }

		[Required]
		[Range(0.01, double.MaxValue)]
		public decimal GrossSalary { get; set; }

		public bool IsActive { get; set; } = true;
	}

	public class EmployeeUpdateDto
	{
		[Required]
		[MaxLength(150)]
		public string FullName { get; set; } = string.Empty;

		[Required]
		[MaxLength(100)]
		public string Position { get; set; } = string.Empty;

		[MaxLength(20)]
		public string? Phone { get; set; }

		[Required]
		public DateTime StartDate { get; set; }

		[Required]
		[Range(0.01, double.MaxValue)]
		public decimal GrossSalary { get; set; }

		public bool IsActive { get; set; } = true;
	}

	public class EmployeeResponseDto
	{
		public Guid Id { get; set; }
		public string FullName { get; set; } = string.Empty;
		public string Position { get; set; } = string.Empty;
		public string? Phone { get; set; }
		public DateTime StartDate { get; set; }
		public decimal GrossSalary { get; set; }

		// Hesaplanmış alanlar
		public decimal NetSalary { get; set; }
		public decimal TotalEmployerCost { get; set; }

		public bool IsActive { get; set; }
		public DateTime CreatedAt { get; set; }
	}

	// ── MAAŞ ÖDEMESİ ──────────────────────────────────────────

	public class SalaryPaymentCreateDto
	{
		[Required]
		[Range(1, 12)]
		public int Month { get; set; }

		[Required]
		[Range(2000, 2100)]
		public int Year { get; set; }

		// Opsiyonel — girilmezse personelin mevcut GrossSalary'si kullanılır
		public decimal? GrossSalary { get; set; }

		public string? Notes { get; set; }
	}

	public class SalaryPaymentResponseDto
	{
		public Guid Id { get; set; }
		public Guid EmployeeId { get; set; }
		public string EmployeeFullName { get; set; } = string.Empty;

		public int Month { get; set; }
		public int Year { get; set; }
		public string MonthLabel { get; set; } = string.Empty;

		// Brüt / Net
		public decimal GrossSalary { get; set; }
		public decimal NetSalary { get; set; }

		// Çalışan kesintileri
		public decimal EmployeeSGK { get; set; }
		public decimal EmployeeUnemployment { get; set; }
		public decimal IncomeTax { get; set; }
		public decimal StampTax { get; set; }

		// İşveren ek maliyetleri
		public decimal EmployerSGK { get; set; }
		public decimal EmployerUnemployment { get; set; }

		// Toplam
		public decimal TotalEmployerCost { get; set; }

		public string? Notes { get; set; }
		public DateTime PaidAt { get; set; }
	}

	// Brüt girilince anlık hesaplama önizlemesi için
	public class SalaryCalculationDto
	{
		public decimal GrossSalary { get; set; }
		public decimal EmployeeSGK { get; set; }
		public decimal EmployeeUnemployment { get; set; }
		public decimal IncomeTax { get; set; }
		public decimal StampTax { get; set; }
		public decimal TotalEmployeeDeductions { get; set; }
		public decimal NetSalary { get; set; }
		public decimal EmployerSGK { get; set; }
		public decimal EmployerUnemployment { get; set; }
		public decimal TotalEmployerCost { get; set; }
	}

	// ── İZİN ──────────────────────────────────────────────────

	public class LeaveCreateDto
	{
		[Required]
		public DateTime StartDate { get; set; }

		[Required]
		public DateTime EndDate { get; set; }

		public string? Reason { get; set; }
	}

	public class LeaveResponseDto
	{
		public Guid Id { get; set; }
		public Guid EmployeeId { get; set; }
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }
		public int DayCount { get; set; }
		public string? Reason { get; set; }
		public DateTime CreatedAt { get; set; }
	}

	// ── ÖZET ──────────────────────────────────────────────────

	public class EmployeeSummaryDto
	{
		public int TotalEmployees { get; set; }
		public int ActiveEmployees { get; set; }
		public decimal TotalGrossSalary { get; set; }
		public decimal TotalNetSalary { get; set; }
		public decimal TotalEmployerCost { get; set; }
		public decimal TotalTaxAndSGK { get; set; } // Toplam vergi + SGK yükü
		public int TotalLeaveDays { get; set; }      // Bu ayki toplam izin günü
	}
}
