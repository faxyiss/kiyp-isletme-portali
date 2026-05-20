namespace StokAppApi.Entities.Dto.Product
{
	public class ProductQueryDto
	{
		// 1. Arama ve Filtreleme Parametreleri
		public string? SearchText { get; set; } // Ad veya Ürün No için arama metni
		public Guid? CategoryId { get; set; }   // Sadece bu kategoriye ait olanlar (NULL ise hepsi)

		// 2. Sıralama Parametreleri
		// "ProductNo", "Quantity", "Price", "Date" değerlerini alabilir
		public string SortBy { get; set; } = "ProductNo";
		public bool IsDescending { get; set; } = true; // true: Büyükten küçüğe, false: Küçükten büyüğe

		// 3. Sayfalama (Pagination) Parametreleri
		public int Page { get; set; } = 1; // Kaçıncı sayfadayız?
		public int PageSize { get; set; } = 10; // Sayfa başına kaç ürün? (Senin isteğin üzere sabit 10)
	}
}
