using Microsoft.EntityFrameworkCore;
using StokAppApi.Entities;
using StokAppApi.Entities.Dto.Customers;
using StokAppApi.Entities.Dto.Sales; // Senin klasör ismine göre güncellendi

namespace StokAppApi.Services
{
	public interface ISaleService
	{
		Task<bool> CreateSaleAsync(Guid userId, CreateSaleDto dto);
		Task<bool> CreateBulkSaleAsync(Guid userId, List<CreateSaleDto> dtos); // YENİ: Toplu Satış
		Task<List<SaleResponseDto>> GetSalesAsync(Guid userId);
		Task<SalePagedResponseDto> GetSalesHistoryAsync(Guid userId, SaleQueryDto query);
		Task<CustomerPurchasePagedResponseDto> GetCustomerPurchaseHistoryAsync(Guid userId, Guid customerId, int page, int pageSize);
		Task<bool> CreateHistoricalSaleAsync(Guid userId, CreateHistoricalSaleDto dto);
		Task<List<CategorySalesSummaryDto>> GetMonthlyCategorySalesAsync(Guid userId, int month, int year);
	}

	public class SaleService : ISaleService
	{
		private readonly AppDbContext _context;

		public SaleService(AppDbContext context)
		{
			_context = context;
		}

		public async Task<bool> CreateSaleAsync(Guid userId, CreateSaleDto dto)
		{
			// 1. Veresiye kontrolünü en başta yapıp sistemi yormadan engelliyoruz
			// Not: Enum adın 'Veresiye' değilse (örn: Debt ise) burayı kendi Enum ismine göre düzelt!
			if (dto.PaymentType == PaymentType.Veresiye && dto.CustomerId == null)
			{
				throw new Exception("Veresiye satışlarda müşteri seçilmesi zorunludur.");
			}

			var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == dto.ProductId && p.UserId == userId);
			if (product == null) throw new Exception("Ürün bulunamadı.");

			var availableStocks = await _context.Stocks
				.Where(s => s.ProductId == dto.ProductId && s.UserId == userId && s.RemainingQuantity > 0)
				.OrderBy(s => s.InflowDate)
				.ToListAsync();

			var totalAvailableQuantity = availableStocks.Sum(s => s.RemainingQuantity);
			if (totalAvailableQuantity < dto.Quantity)
				throw new Exception($"Yetersiz stok! Mevcut stok: {totalAvailableQuantity}");

			decimal remainingQuantityToSell = dto.Quantity;
			decimal totalSaleAmountToDebt = 0;

			foreach (var stock in availableStocks)
			{
				if (remainingQuantityToSell <= 0) break;

				decimal quantityToDeduct = Math.Min(remainingQuantityToSell, stock.RemainingQuantity);

				stock.RemainingQuantity -= quantityToDeduct;
				stock.UpdatedAt = DateTime.UtcNow;

				var sale = new Sale
				{
					UserId = userId,
					ProductId = dto.ProductId,
					StockId = stock.Id,
					CustomerId = dto.CustomerId,
					Quantity = quantityToDeduct,
					SalePrice = dto.SalePrice,
					UnitCostAtSale = stock.UnitCost,
					TotalAmount = quantityToDeduct * dto.SalePrice,
					PaymentType = dto.PaymentType,
					SaleDate = DateTime.UtcNow
				};

				_context.Sales.Add(sale);
				totalSaleAmountToDebt += sale.TotalAmount;
				remainingQuantityToSell -= quantityToDeduct;
			}

			// Müşteri Borç Güncellemesi
			if (dto.PaymentType == PaymentType.Veresiye && dto.CustomerId != null)
			{
				var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Id == dto.CustomerId && c.UserId == userId);
				if (customer != null)
				{
					customer.CurrentBalance += totalSaleAmountToDebt;
					customer.UpdatedAt = DateTime.UtcNow;
				}
				else throw new Exception("Seçilen müşteri bulunamadı.");
			}

			await _context.SaveChangesAsync();
			return true;
		}

		// YENİ EKLENEN METOD: Sepetteki Tüm Ürünleri Tek Seferde İşler
		public async Task<bool> CreateBulkSaleAsync(Guid userId, List<CreateSaleDto> dtos)
		{
			// Transaction başlatıyoruz (Eğer 5 üründen 4'ü satılıp 1'inde stok hatası çıkarsa, TÜM işlemi geri alır, veritabanını bozmaz!)
			using var transaction = await _context.Database.BeginTransactionAsync();
			try
			{
				foreach (var dto in dtos)
				{
					// Yukarıdaki güvenli tekil satış metodumuzu döngüye sokuyoruz
					await CreateSaleAsync(userId, dto);
				}

				await transaction.CommitAsync(); // Her şey başarılıysa onayla
				return true;
			}
			catch (Exception)
			{
				await transaction.RollbackAsync(); // Hata varsa hiçbirini kaydetme (Geri al)
				throw; // Hatayı frontend'e fırlat
			}
		}

		public async Task<List<SaleResponseDto>> GetSalesAsync(Guid userId)
		{
			return await _context.Sales
				.Include(s => s.Product)
				.Include(s => s.Customer)
				.Where(s => s.UserId == userId)
				.OrderByDescending(s => s.SaleDate)
				.Select(s => new SaleResponseDto
				{
					Id = s.Id,
					ProductName = s.Product != null ? s.Product.Name : "Bilinmeyen Ürün",
					CustomerName = s.Customer != null ? s.Customer.FirstName + " " + s.Customer.LastName : "Perakende",
					Quantity = s.Quantity,
					SalePrice = s.SalePrice,
					TotalAmount = s.TotalAmount,
					PaymentType = s.PaymentType.ToString(),
					SaleDate = s.SaleDate
				})
				.ToListAsync();
		}

		public async Task<SalePagedResponseDto> GetSalesHistoryAsync(Guid userId, SaleQueryDto query)
		{
			// 1. Temel sorguyu oluştur
			var q = _context.Sales
				.Include(s => s.Product)
				.Include(s => s.Customer)
				.Where(s => s.UserId == userId)
				.AsQueryable();

			// 2. Filtreleri Uygula
			if (!string.IsNullOrWhiteSpace(query.Search))
			{
				var search = query.Search.ToLower();
				q = q.Where(s =>
					(s.Product != null && s.Product.Name.ToLower().Contains(search)) ||
					(s.Customer != null && (s.Customer.FirstName.ToLower() + " " + s.Customer.LastName.ToLower()).Contains(search))
				);
			}

			if (query.StartDate.HasValue)
				q = q.Where(s => s.SaleDate >= query.StartDate.Value);

			if (query.EndDate.HasValue)
				q = q.Where(s => s.SaleDate <= query.EndDate.Value);

			if (query.PaymentType.HasValue)
				q = q.Where(s => s.PaymentType == query.PaymentType.Value);

			if (query.CategoryId.HasValue)
				q = q.Where(s => s.Product!.CategoryId == query.CategoryId.Value);

			if (query.CustomerId.HasValue)
				q = q.Where(s => s.CustomerId == query.CustomerId.Value);

			if (query.MinAmount.HasValue)
				q = q.Where(s => (s.Quantity * s.SalePrice) >= query.MinAmount.Value);

			if (query.MaxAmount.HasValue)
				q = q.Where(s => (s.Quantity * s.SalePrice) <= query.MaxAmount.Value);

			// 3. İstatistikleri (Toplam Kayıt ve Toplam Ciro) hesapla
			var totalItems = await q.CountAsync();
			var totalRevenue = await q.SumAsync(s => s.Quantity * s.SalePrice);

			// 4. Sıralama (Order By)
			q = query.SortBy?.ToLower() switch
			{
				"date_asc" => q.OrderBy(s => s.SaleDate),
				"amount_desc" => q.OrderByDescending(s => s.Quantity * s.SalePrice),
				"amount_asc" => q.OrderBy(s => s.Quantity * s.SalePrice),
				_ => q.OrderByDescending(s => s.SaleDate), // Varsayılan: En yeniler ilk
			};

			// 5. Sayfalama ve DTO'ya dönüştürme (Sadece o sayfanın verisi veritabanından çekilir)
			var sales = await q
				.Skip((query.Page - 1) * query.PageSize)
				.Take(query.PageSize)
				.Select(s => new SaleResponseDto
				{
					Id = s.Id,
					ProductName = s.Product!.Name,
					CustomerName = s.Customer != null ? s.Customer.FirstName + " " + s.Customer.LastName : "Genel Müşteri",
					Quantity = s.Quantity,
					SalePrice = s.SalePrice,
					TotalAmount = s.Quantity * s.SalePrice,
					PaymentType = s.PaymentType.ToString(),
					SaleDate = s.SaleDate
				})
				.ToListAsync();

			// 6. Yanıtı dön
			return new SalePagedResponseDto
			{
				Items = sales,
				TotalItems = totalItems,
				TotalPages = (int)Math.Ceiling(totalItems / (double)query.PageSize),
				CurrentPage = query.Page,
				TotalRevenue = totalRevenue
			};
		}

		public async Task<CustomerPurchasePagedResponseDto> GetCustomerPurchaseHistoryAsync(Guid userId, Guid customerId, int page, int pageSize)
		{
			var query = _context.Sales
				.Include(s => s.Product)
				.Where(s => s.UserId == userId && s.CustomerId == customerId)
				.AsQueryable();

			var totalItems = await query.CountAsync();

			// double olan s.Quantity alanları decimal'e güvenle cast edilerek çarpıldı
			var grandTotalSpent = await query.SumAsync(s => (decimal)s.Quantity * s.SalePrice);

			var purchases = await query
				.OrderByDescending(s => s.SaleDate)
				.Skip((page - 1) * pageSize)
				.Take(pageSize)
				.Select(s => new CustomerPurchaseHistoryDto
				{
					SaleId = s.Id,
					ProductName = s.Product != null ? s.Product.Name : "Silinmiş Ürün",
					Quantity = s.Quantity,
					SalePrice = s.SalePrice,
					TotalAmount = (decimal)s.Quantity * s.SalePrice,
					PaymentType = s.PaymentType.ToString(),
					SaleDate = s.SaleDate
				})
				.ToListAsync();

			return new CustomerPurchasePagedResponseDto
			{
				Items = purchases,
				TotalItems = totalItems,
				TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize),
				CurrentPage = page,
				GrandTotalSpent = grandTotalSpent
			};
		}

		public async Task<bool> CreateHistoricalSaleAsync(Guid userId, CreateHistoricalSaleDto dto)
		{
			if (dto.PaymentType == PaymentType.Veresiye && dto.CustomerId == null)
				throw new Exception("Veresiye satışlarda müşteri seçilmesi zorunludur.");

			var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == dto.ProductId && p.UserId == userId);
			if (product == null) throw new Exception("Ürün bulunamadı.");

			var availableStocks = await _context.Stocks
				.Where(s => s.ProductId == dto.ProductId && s.UserId == userId && s.RemainingQuantity > 0)
				.OrderBy(s => s.InflowDate)
				.ToListAsync();

			var totalAvailableQuantity = availableStocks.Sum(s => s.RemainingQuantity);
			if (totalAvailableQuantity < dto.Quantity)
				throw new Exception($"Yetersiz stok! Mevcut stok: {totalAvailableQuantity}");

			decimal remainingQuantityToSell = dto.Quantity;
			decimal totalSaleAmountToDebt = 0;

			foreach (var stock in availableStocks)
			{
				if (remainingQuantityToSell <= 0) break;

				decimal quantityToDeduct = Math.Min(remainingQuantityToSell, stock.RemainingQuantity);
				stock.RemainingQuantity -= quantityToDeduct;
				stock.UpdatedAt = DateTime.UtcNow;

				var sale = new Sale
				{
					UserId = userId,
					ProductId = dto.ProductId,
					StockId = stock.Id,
					CustomerId = dto.CustomerId,
					Quantity = quantityToDeduct,
					SalePrice = dto.SalePrice,
					UnitCostAtSale = stock.UnitCost,
					TotalAmount = quantityToDeduct * dto.SalePrice,
					PaymentType = dto.PaymentType,
					SaleDate = dto.SaleDate // Dışarıdan gelen geçmiş tarihi kullanıyoruz
				};

				_context.Sales.Add(sale);
				totalSaleAmountToDebt += sale.TotalAmount;
				remainingQuantityToSell -= quantityToDeduct;
			}

			if (dto.PaymentType == PaymentType.Veresiye && dto.CustomerId != null)
			{
				var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Id == dto.CustomerId && c.UserId == userId);
				if (customer != null)
				{
					customer.CurrentBalance += totalSaleAmountToDebt;
					customer.UpdatedAt = DateTime.UtcNow;
				}
				else throw new Exception("Seçilen müşteri bulunamadı.");
			}

			await _context.SaveChangesAsync();
			return true;
		}

		public async Task<List<CategorySalesSummaryDto>> GetMonthlyCategorySalesAsync(Guid userId, int month, int year)
		{
			var startDate = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
			var endDate = startDate.AddMonths(1).AddTicks(-1);

			return await _context.Sales
				.Include(s => s.Product)
					.ThenInclude(p => p!.Category)
				.Where(s => s.UserId == userId
						 && s.SaleDate >= startDate
						 && s.SaleDate <= endDate)
				.GroupBy(s => s.Product!.Category != null
							? s.Product.Category.Name
							: "Kategorisiz")
				.Select(g => new CategorySalesSummaryDto
				{
					CategoryName = g.Key,
					TotalRevenue = g.Sum(s => s.TotalAmount),
					TotalQuantity = g.Sum(s => s.Quantity)
				})
				.OrderByDescending(x => x.TotalRevenue)
				.ToListAsync();
		}
	}
}