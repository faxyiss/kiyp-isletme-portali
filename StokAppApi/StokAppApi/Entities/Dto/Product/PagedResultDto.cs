namespace StokAppApi.Entities.Dto.Product
{
	public class PagedResultDto<T>
	{
		public List<T> Items { get; set; } = new();
		public int TotalCount { get; set; } // Toplam kayıt sayısı (Örn: Filtrelere uyan toplam 45 ürün var)
		public int Page { get; set; }
		public int TotalPages { get; set; } // Toplam kaç sayfa ediyor (Örn: 45 ürün / 10 = 5 sayfa)
	}
}
