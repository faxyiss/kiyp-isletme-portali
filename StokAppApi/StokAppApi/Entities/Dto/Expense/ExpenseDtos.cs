using System.ComponentModel.DataAnnotations;

namespace StokAppApi.Entities.Dto.Expense
{
	// ── Kategori ──────────────────────────────────────────
	public class ExpenseCategoryCreateDto
	{
		[Required]
		[MaxLength(100)]
		public string Name { get; set; } = string.Empty;
	}

	public class ExpenseCategoryResponseDto
	{
		public Guid Id { get; set; }
		public string Name { get; set; } = string.Empty;
	}

	// ── Gider Ekleme ──────────────────────────────────────
	public class ExpenseCreateDto
	{
		[Required]
		public Guid CategoryId { get; set; }

		[Required]
		[MaxLength(150)]
		public string Title { get; set; } = string.Empty;

		[Required]
		[Range(0.01, double.MaxValue, ErrorMessage = "Tutar 0'dan büyük olmalıdır.")]
		public decimal Amount { get; set; }

		[Required]
		public ExpenseType ExpenseType { get; set; }

		// Tüm tipler için zorunlu (tek seferlik için işlem tarihi,
		// dönemsel için dönem başlangıcı, aylık için referans ay)
		[Required]
		public DateTime StartDate { get; set; }

		// Sadece dönemsel giderler için
		public DateTime? EndDate { get; set; }

		// Sadece aylık giderler için (1-31)
		[Range(1, 31)]
		public int? RecurringDay { get; set; }

		public string? Notes { get; set; }
	}

	// ── Gider Listeleme ───────────────────────────────────
	public class ExpenseResponseDto
	{
		public Guid Id { get; set; }
		public string Title { get; set; } = string.Empty;
		public string CategoryName { get; set; } = string.Empty;
		public decimal Amount { get; set; }
		public ExpenseType ExpenseType { get; set; }
		public string ExpenseTypeLabel { get; set; } = string.Empty;
		public DateTime StartDate { get; set; }
		public DateTime? EndDate { get; set; }
		public int? RecurringDay { get; set; }
		public string? Notes { get; set; }
		public DateTime CreatedAt { get; set; }
	}

	// ── Listeleme Sorgu Parametreleri ─────────────────────
	public class ExpenseQueryDto
	{
		public int Page { get; set; } = 1;
		public int PageSize { get; set; } = 10;

		// Ay/Yıl filtresi — o aya ait giderleri getirir
		public int? Month { get; set; }
		public int? Year { get; set; }

		// Tip filtresi
		public ExpenseType? ExpenseType { get; set; }

		// Kategori filtresi
		public Guid? CategoryId { get; set; }
	}

	// ── Sayfalı Response ──────────────────────────────────
	public class ExpensePagedResponseDto
	{
		public List<ExpenseResponseDto> Items { get; set; } = new();
		public int TotalCount { get; set; }
		public int TotalPages { get; set; }
		public int CurrentPage { get; set; }
		public decimal TotalAmount { get; set; } // Filtreye uyan toplam tutar
	}

	// ── Grafik / Özet ─────────────────────────────────────
	public class ExpenseMonthlySummaryDto
	{
		public int Year { get; set; }
		public int Month { get; set; }
		public string MonthLabel { get; set; } = string.Empty; // "Ocak 2026"
		public decimal TotalAmount { get; set; }

		// Kategori bazlı dağılım
		public List<ExpenseCategoryBreakdownDto> Breakdown { get; set; } = new();
	}

	public class ExpenseCategoryBreakdownDto
	{
		public string CategoryName { get; set; } = string.Empty;
		public decimal Amount { get; set; }
		public double Percentage { get; set; }
	}
}
