namespace StokAppApi.Entities.Dto.Product
{
	public class ProductSalesQueryDto
	{
		// İsim veya Ürün No ile arama için
		public string? SearchText { get; set; }

		// Kategoriye göre filtreleme
		public Guid? CategoryId { get; set; }

		// Stok Durumu: true (Sadece Stokta Olanlar), false (Stokta Kalmayanlar), null (Tümü)
		public bool? InStock { get; set; }

		// Fiyat Aralığı Filtreleri
		public decimal? MinPrice { get; set; }
		public decimal? MaxPrice { get; set; }

		// Sıralama Seçenekleri: productno, price, name, quantity
		public string SortBy { get; set; } = "productno";
		public bool IsDescending { get; set; } = false;

		// Sayfalama Parametreleri (İstediğin gibi varsayılan 12 ürün)
		public int Page { get; set; } = 1;
		public int PageSize { get; set; } = 12;
	}
}
