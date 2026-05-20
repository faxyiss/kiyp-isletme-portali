using System.ComponentModel.DataAnnotations;
namespace StokAppApi.Entities.Dto.Customers
{
	public class CustomerCreateDto
	{
		[Required(ErrorMessage = "Müşteri adı zorunludur.")]
		[MaxLength(50)]
		public string FirstName { get; set; } = string.Empty;

		[Required(ErrorMessage = "Müşteri soyadı zorunludur.")]
		[MaxLength(50)]
		public string LastName { get; set; } = string.Empty;

		[MaxLength(20)]
		public string? PhoneNumber { get; set; }

		public decimal InitialDebt { get; set; } = 0; // Başlangıç borcu (Arayüzdeki CurrentBalance)
	}

	public class CustomerUpdateDto
	{
		[Required(ErrorMessage = "Müşteri adı zorunludur.")]
		[MaxLength(50)]
		public string FirstName { get; set; } = string.Empty;

		[Required(ErrorMessage = "Müşteri soyadı zorunludur.")]
		[MaxLength(50)]
		public string LastName { get; set; } = string.Empty;

		[MaxLength(20)]
		public string? PhoneNumber { get; set; }
	}

	public class CustomerPaymentDto
	{
		[Required]
		[Range(0.01, double.MaxValue, ErrorMessage = "Tahsilat tutarı 0'dan büyük olmalıdır.")]
		public decimal Amount { get; set; }

		public string? Description { get; set; } // Opsiyonel açıklama
	}

	public class CustomerResponseDto
	{
		public Guid Id { get; set; }
		public string FirstName { get; set; } = string.Empty;
		public string LastName { get; set; } = string.Empty;
		public string? PhoneNumber { get; set; }
		public decimal CurrentBalance { get; set; }
		public DateTime CreatedAt { get; set; }
	}

	public class CustomerQueryDto
	{
		public string? Search { get; set; }
		public string? DebtFilter { get; set; } = "all"; // all, has_debt, no_debt
		public int Page { get; set; } = 1;
		public int PageSize { get; set; } = 10;
	}

	public class CustomerPagedResponseDto
	{
		public IEnumerable<CustomerResponseDto> Items { get; set; } = new List<CustomerResponseDto>();
		public int TotalItems { get; set; }
		public int TotalPages { get; set; }
		public int CurrentPage { get; set; }
	}

	public class CustomerPurchaseHistoryDto
	{
		public Guid SaleId { get; set; }
		public string ProductName { get; set; } = string.Empty;
		public decimal Quantity { get; set; }
		public decimal SalePrice { get; set; }
		public decimal TotalAmount { get; set; }
		public string PaymentType { get; set; } = string.Empty;
		public DateTime SaleDate { get; set; }
	}

	public class CustomerPurchasePagedResponseDto
	{
		public IEnumerable<CustomerPurchaseHistoryDto> Items { get; set; } = new List<CustomerPurchaseHistoryDto>();
		public int TotalItems { get; set; }
		public int TotalPages { get; set; }
		public int CurrentPage { get; set; }
		public decimal GrandTotalSpent { get; set; } // Müşterinin bugüne kadar yaptığı toplam harcama tutarı
	}
}
