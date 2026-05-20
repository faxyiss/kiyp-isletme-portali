using System.ComponentModel.DataAnnotations;

namespace StokAppApi.Entities.Dto.Stock
{
	public class DeductStockDto
	{
		[Required(ErrorMessage = "Stok ID bilgisi zorunludur.")]
		public Guid StockId { get; set; }

		[Range(0.01, double.MaxValue, ErrorMessage = "Eksiltilecek miktar 0'dan büyük olmalıdır.")]
		public decimal Quantity { get; set; }
	}
}
