using System.ComponentModel.DataAnnotations;

namespace StokAppApi.Entities.Dto.Product
{
	public class ProductUpdateDto
	{
		[Required(ErrorMessage = "Ürün adı zorunludur.")]
		[MaxLength(150)]
		public string Name { get; set; } = string.Empty;

		public string? Description { get; set; }

		[Required(ErrorMessage = "Kategori seçimi zorunludur.")]
		public Guid CategoryId { get; set; }

		[Range(0, double.MaxValue, ErrorMessage = "Satış fiyatı 0'dan küçük olamaz.")]
		public decimal UnitPrice { get; set; }
	}
}
