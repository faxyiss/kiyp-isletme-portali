namespace StokAppApi.Entities
{
	public enum ExpenseType
	{
		OneTime = 0,    // Tek seferlik (tadilat, ekipman)
		Monthly = 1,    // Aylık sabit (kira - ayın X. günü)
		Periodic = 2    // Dönemsel (ayın 1-15 arası gibi)
	}

	public class ExpenseCategory
	{
		public Guid Id { get; set; } = Guid.NewGuid();
		public Guid UserId { get; set; }
		public string Name { get; set; } = string.Empty;
		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

		// Navigation
		public User? User { get; set; }
		public List<Expense> Expenses { get; set; } = new();
	}

	public class Expense
	{
		public Guid Id { get; set; } = Guid.NewGuid();
		public Guid UserId { get; set; }
		public Guid CategoryId { get; set; }

		public string Title { get; set; } = string.Empty;
		public decimal Amount { get; set; }
		public ExpenseType ExpenseType { get; set; }

		// Tek seferlik ve dönemsel için başlangıç tarihi
		public DateTime StartDate { get; set; }

		// Dönemsel için bitiş tarihi (tek seferlik ve aylık için NULL)
		public DateTime? EndDate { get; set; }

		// Aylık sabit için ayın kaçıncı günü (1-31), diğerleri için NULL
		public int? RecurringDay { get; set; }

		public string? Notes { get; set; }
		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
		public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

		// Navigation
		public User? User { get; set; }
		public ExpenseCategory? Category { get; set; }
	}
}
