using Microsoft.EntityFrameworkCore;
using StokAppApi.Entities;
using StokAppApi.Entities.Dto.Product;
using StokAppApi.Entities.Dto.Stock;

namespace StokAppApi.Services
{
	public interface IStockService
	{
		Task<bool> AddStockInflowAsync(Guid userId, StockInflowDto dto);
		Task<PagedResultDto<StockHistoryResponseDto>> GetStockHistoryAsync(Guid userId, Guid productId, int page, int pageSize);
		Task<(bool Success, string Message)> DeleteStockAsync(Guid userId, Guid stockId);

		Task<(bool Success, string Message)> DeductStockAsync(Guid userId, DeductStockDto dto);
	}
	public class StockService : IStockService
	{
		private readonly AppDbContext _context;

		public StockService(AppDbContext context)
		{
			_context = context;
		}

		public async Task<bool> AddStockInflowAsync(Guid userId, StockInflowDto dto)
		{
			// Güvenlik ve Hak Doğrulaması: Kullanıcı gerçekten kendi listesindeki bir ürüne mi stok ekliyor?
			var productExists = await _context.Products.AnyAsync(p => p.Id == dto.ProductId && p.UserId == userId);
			if (!productExists) return false;

			var stock = new Stock
			{
				Id = Guid.NewGuid(),
				UserId = userId,
				ProductId = dto.ProductId,
				InflowQuantity = dto.InflowQuantity,
				RemainingQuantity = dto.InflowQuantity, // Satışlarda partisel maliyet düşebilmek için kalan miktar başlangıçta giriş kadardır
				UnitCost = dto.UnitCost,
				// Eğer dışarıdan tarih gönderilmişse onu evrensel zamana (UTC) çevirip kaydet, gönderilmemişse UtcNow kullan
				InflowDate = dto.InflowDate.HasValue ? dto.InflowDate.Value.ToUniversalTime() : DateTime.UtcNow,
				CreatedAt = DateTime.UtcNow,
				UpdatedAt = DateTime.UtcNow
			};

			await _context.Stocks.AddAsync(stock);
			await _context.SaveChangesAsync();
			return true;
		}
		public async Task<PagedResultDto<StockHistoryResponseDto>> GetStockHistoryAsync(Guid userId, Guid productId, int page, int pageSize)
		{
			var query = _context.Stocks
				.Where(s => s.UserId == userId && s.ProductId == productId)
				.OrderByDescending(s => s.InflowDate); // Tarihe göre en yeniler en üstte!

			var totalCount = await query.CountAsync();

			var items = await query
				.Skip((page - 1) * pageSize)
				.Take(pageSize)
				.Select(s => new StockHistoryResponseDto
				{
					Id = s.Id,
					InflowQuantity = s.InflowQuantity,
					RemainingQuantity = s.RemainingQuantity,
					UnitCost = s.UnitCost,
					InflowDate = s.InflowDate
				})
				.ToListAsync();

			return new PagedResultDto<StockHistoryResponseDto>
			{
				Items = items,
				TotalCount = totalCount,
				Page = page,
				TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
			};
		}

		// 2. STOK SİLME (GÜVENLİK KONTROLLÜ)
		public async Task<(bool Success, string Message)> DeleteStockAsync(Guid userId, Guid stockId)
		{
			var stock = await _context.Stocks.FirstOrDefaultAsync(s => s.Id == stockId && s.UserId == userId);

			if (stock == null)
				return (false, "Stok partisi bulunamadı.");

			// GÜVENLİK KONTROLÜ: Eğer İlk giriş miktarı, kalan miktara eşit değilse (yani satış/eksiltme yapılmışsa) silmeyi engelle!
			if (stock.InflowQuantity != stock.RemainingQuantity)
			{
				return (false, "Bu stok partisinden ürün satılmış veya düşülmüş. Geçmiş maliyetlerin bozulmaması için tamamen silinemez. Dilerseniz 'Stok Eksilt' ile kalanı sıfırlayabilirsiniz.");
			}

			_context.Stocks.Remove(stock);
			await _context.SaveChangesAsync();

			return (true, "Stok kaydı başarıyla silindi.");
		}

		public async Task<(bool Success, string Message)> DeductStockAsync(Guid userId, DeductStockDto dto)
		{
			// Güvenlik: Stok partisi gerçekten bu kullanıcıya mı ait?
			var stock = await _context.Stocks.FirstOrDefaultAsync(s => s.Id == dto.StockId && s.UserId == userId);

			if (stock == null)
				return (false, "İşlem yapılmak istenen stok partisi bulunamadı.");

			// Mantık Kontrolü: Eksiltilecek miktar sıfır veya negatif olamaz
			if (dto.Quantity <= 0)
				return (false, "Eksiltilecek miktar 0'dan büyük olmalıdır.");

			// Mantık Kontrolü: Kalan miktardan fazlası düşülemez
			if (dto.Quantity > stock.RemainingQuantity)
				return (false, $"Bu partide yalnızca {stock.RemainingQuantity} adet ürün kaldı. Daha fazlasını eksiltemezsiniz.");

			// Miktarı düşür ve güncellenme tarihini ayarla
			stock.RemainingQuantity -= dto.Quantity;
			stock.UpdatedAt = DateTime.UtcNow;

			await _context.SaveChangesAsync();

			return (true, "Stok başarıyla eksiltildi moruq.");
		}
	}
}

