using System.ComponentModel.DataAnnotations;

namespace StokAppApi.Entities.Dto.Stock
{
	public class StockInflowDto
	{
		[Required(ErrorMessage = "Ürün seçimi zorunludur.")]
		public Guid ProductId { get; set; }

		[Range(1, double.MaxValue, ErrorMessage = "Giriş adedi en az 1 olmalıdır.")]
		public decimal InflowQuantity { get; set; }

		[Range(0.01, double.MaxValue, ErrorMessage = "Birim maliyet 0'dan büyük olmalıdır.")]
		public decimal UnitCost { get; set; }

		public DateTime? InflowDate { get; set; }
	}
}
