using System.ComponentModel.DataAnnotations;

namespace StokAppApi.Entities
{
	public class Product
	{
		public Guid Id { get; set; } = Guid.NewGuid();
		public Guid UserId { get; set; }
		public Guid CategoryId { get; set; }

		// YENİ EKLENEN: Otomatik artacak olan Ürün Numarası
		public int ProductNo { get; set; }

		[Required, MaxLength(150)]
		public string Name { get; set; } = string.Empty;
		public string? Description { get; set; }
		public decimal UnitPrice { get; set; }
		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
		public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

		// Navigation Properties
		public User? User { get; set; }
		public Category? Category { get; set; }
		public ICollection<Stock> Stocks { get; set; } = new List<Stock>();
	}
}
