using System.ComponentModel.DataAnnotations;

namespace StokAppApi.Entities.Dto.Production
{
	public class RecipeCreateDto
	{
		[Required]
		public Guid ProductId { get; set; }

		[Required]
		public List<RecipeItemCreateDto> Items { get; set; } = new();
	}

	public class RecipeItemCreateDto
	{
		[Required]
		public Guid RawMaterialId { get; set; }

		[Required]
		[Range(0.01, double.MaxValue, ErrorMessage = "Miktar 0'dan büyük olmalıdır.")]
		public decimal RequiredQuantity { get; set; }
	}

	public class RecipeResponseDto
	{
		public Guid Id { get; set; }
		public Guid ProductId { get; set; }
		public string ProductName { get; set; } = string.Empty;
		public DateTime CreatedAt { get; set; }
		public List<RecipeItemResponseDto> Items { get; set; } = new();
	}

	public class RecipeItemResponseDto
	{
		public Guid RawMaterialId { get; set; }
		public string RawMaterialName { get; set; } = string.Empty;
		public decimal RequiredQuantity { get; set; }
	}

	// Tek log kaydı DTO
	public class ProductionLogResponseDto
	{
		public Guid Id { get; set; }
		public string ProductName { get; set; } = string.Empty;
		public decimal Quantity { get; set; }
		public decimal TotalCost { get; set; }
		public decimal UnitCost { get; set; }
		public DateTime ProducedAt { get; set; }
	}

	// Sayfalı response + özet istatistikler
	public class ProductionLogPagedResponseDto
	{
		public List<ProductionLogResponseDto> Items { get; set; } = new();
		public int TotalCount { get; set; }
		public int TotalPages { get; set; }
		public int CurrentPage { get; set; }

		// Tüm filtreye uyan kayıtların toplamları (sadece mevcut sayfanın değil)
		public decimal SummaryTotalQuantity { get; set; }
		public decimal SummaryTotalCost { get; set; }
		public int SummaryUniqueProducts { get; set; }
	}
	public class ProductionLogQueryDto
	{
		public int Page { get; set; } = 1;
		public int PageSize { get; set; } = 10;

		// Ürün adına göre arama
		public string? SearchText { get; set; }

		// Tarih filtresi
		public DateTime? StartDate { get; set; }
		public DateTime? EndDate { get; set; }

		// Sıralama
		public string SortBy { get; set; } = "date"; // date, quantity, totalcost
		public bool IsDescending { get; set; } = true;
	}
}
