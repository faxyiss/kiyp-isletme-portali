namespace StokAppApi.Entities.Dto.Product
{
	public class ProductResponseDto
	{
		public Guid Id { get; set; }
		public int ProductNo { get; set; } // Otomatik artan temiz ürün numarası
		public string Name { get; set; } = string.Empty;
		public string? Description { get; set; }
		public string CategoryName { get; set; } = "Genel";
		public decimal UnitPrice { get; set; }
		public decimal RemainingQuantity { get; set; } // Tüm partilerin kalan stok toplamı
		public string UpdatedAt { get; set; } = string.Empty;

		public decimal TotalCostValue { get; set; }
	}
}
