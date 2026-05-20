using System.ComponentModel.DataAnnotations;

namespace StokAppApi.Entities.Dto.Sales
{
	public class CreateSaleDto
	{
		[Required]
		public Guid ProductId { get; set; }

		[Required]
		[Range(0.01, double.MaxValue, ErrorMessage = "Miktar 0'dan büyük olmalıdır.")]
		public decimal Quantity { get; set; }

		[Required]
		public decimal SalePrice { get; set; } // Birim Satış Fiyatı

		[Required]
		public PaymentType PaymentType { get; set; }

		public Guid? CustomerId { get; set; } // Müşteri seçilmeyebilir (perakende nakit satış vs.)
	}

	public class SaleResponseDto
	{
		public Guid Id { get; set; }
		public string ProductName { get; set; } = string.Empty;
		public string? CustomerName { get; set; }
		public decimal Quantity { get; set; }
		public decimal SalePrice { get; set; }
		public decimal TotalAmount { get; set; }
		public string PaymentType { get; set; } = string.Empty;
		public DateTime SaleDate { get; set; }
	}

	public class SaleQueryDto
	{
		public string? Search { get; set; }
		public DateTime? StartDate { get; set; }
		public DateTime? EndDate { get; set; }
		public PaymentType? PaymentType { get; set; }
		public Guid? CategoryId { get; set; }
		public Guid? CustomerId { get; set; }
		public decimal? MinAmount { get; set; }
		public decimal? MaxAmount { get; set; }

		public string? SortBy { get; set; } = "date_desc"; // date_desc, date_asc, amount_desc, amount_asc
		public int Page { get; set; } = 1;
		public int PageSize { get; set; } = 15;
	}

	public class SalePagedResponseDto
	{
		public IEnumerable<SaleResponseDto> Items { get; set; } = new List<SaleResponseDto>();
		public int TotalItems { get; set; }
		public int TotalPages { get; set; }
		public int CurrentPage { get; set; }
		public decimal TotalRevenue { get; set; } // Filtrelenen sonuçların toplam cirosu
	}

	public class CreateHistoricalSaleDto
	{
		[Required] public Guid ProductId { get; set; }
		[Required][Range(0.01, double.MaxValue)] public decimal Quantity { get; set; }
		[Required] public decimal SalePrice { get; set; }
		[Required] public PaymentType PaymentType { get; set; }
		public Guid? CustomerId { get; set; }

		// Bu modelde tarih zorunludur
		[Required] public DateTime SaleDate { get; set; }
	}
	public class CategorySalesSummaryDto
	{
		public string CategoryName { get; set; } = string.Empty;
		public decimal TotalRevenue { get; set; }
		public decimal TotalQuantity { get; set; }
	}
}
