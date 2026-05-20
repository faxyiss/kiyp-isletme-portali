using System.ComponentModel.DataAnnotations;

namespace StokAppApi.Entities.Dto
{
	public class CategoryCreateDto
	{
		[Required(ErrorMessage = "Kategori adı zorunludur.")]
		[MaxLength(100, ErrorMessage = "Kategori adı en fazla 100 karakter olabilir.")]
		public string Name { get; set; } = string.Empty;

		// SQL şemandaki kurala göre -> 0: Ürün, 1: Hammadde
		// Varsayılan olarak 0 (Ürün) atıyoruz
		[Required(ErrorMessage = "Kategori tipi zorunludur.")]
		public CategoryType Type { get; set; } = CategoryType.Urun;
	}

	public class CategoryResponseDto
	{
		public Guid Id { get; set; }
		public string Name { get; set; } = string.Empty;
		public CategoryType Type { get; set; }
	}
}
