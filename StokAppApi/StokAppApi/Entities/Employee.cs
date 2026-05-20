namespace StokAppApi.Entities
{
	public class Employee
	{
		public Guid Id { get; set; } = Guid.NewGuid();
		public Guid UserId { get; set; }

		public string FullName { get; set; } = string.Empty;
		public string Position { get; set; } = string.Empty;
		public string? Phone { get; set; }
		public DateTime StartDate { get; set; }
		public decimal GrossSalary { get; set; }
		public bool IsActive { get; set; } = true;

		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
		public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

		// Navigation
		public User? User { get; set; }
		public List<SalaryPayment> SalaryPayments { get; set; } = new();
		public List<LeaveRecord> LeaveRecords { get; set; } = new();
	}

	public class SalaryPayment
	{
		public Guid Id { get; set; } = Guid.NewGuid();
		public Guid EmployeeId { get; set; }

		public int Month { get; set; }
		public int Year { get; set; }

		// Brüt / Net
		public decimal GrossSalary { get; set; }
		public decimal NetSalary { get; set; }

		// Çalışan kesintileri
		public decimal EmployeeSGK { get; set; }       // %14
		public decimal EmployeeUnemployment { get; set; } // %1
		public decimal IncomeTax { get; set; }          // %15
		public decimal StampTax { get; set; }           // %0.759

		// İşveren ek maliyetleri
		public decimal EmployerSGK { get; set; }        // %20.5
		public decimal EmployerUnemployment { get; set; } // %2

		// Toplam işveren maliyeti
		public decimal TotalEmployerCost { get; set; }

		public string? Notes { get; set; }
		public DateTime PaidAt { get; set; } = DateTime.UtcNow;
		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

		// Navigation
		public Employee? Employee { get; set; }
	}

	public class LeaveRecord
	{
		public Guid Id { get; set; } = Guid.NewGuid();
		public Guid EmployeeId { get; set; }

		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }
		public int DayCount { get; set; }
		public string? Reason { get; set; }

		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

		// Navigation
		public Employee? Employee { get; set; }
	}
}
