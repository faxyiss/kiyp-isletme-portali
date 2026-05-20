using Microsoft.EntityFrameworkCore;
using StokAppApi.Entities;
using StokAppApi.Entities.Dto.Product;

namespace StokAppApi.Services
{
	public interface IProductService
	{
		Task<Guid> CreateProductAsync(Guid userId, ProductCreateDto dto);
		Task<bool> UpdateProductAsync(Guid userId, Guid productId, ProductUpdateDto dto);
		Task<(bool Success, string Message)> DeleteProductAsync(Guid userId, Guid productId);
		Task<PagedResultDto<ProductResponseDto>> SearchProductsAsync(Guid userId, ProductQueryDto query);
		Task<PagedResultDto<ProductResponseDto>> GetRawMaterialsAsync(Guid userId, ProductQueryDto query);
		Task<PagedResultDto<ProductResponseDto>> GetSalesTerminalProductsAsync(Guid userId, ProductSalesQueryDto query);
	}
	public class ProductService : IProductService
	{
		private readonly AppDbContext _context;

		public ProductService(AppDbContext context)
		{
			_context = context;
		}

		// 1. YENİ ÜRÜN OLUŞTURMA
		public async Task<Guid> CreateProductAsync(Guid userId, ProductCreateDto dto)
		{
			// Sadece bu kullanıcının ürünlerine bak ve en yüksek numarayı bul
			var maxProductNo = await _context.Products
				.Where(p => p.UserId == userId) // BU SATIRI GERİ GETİRDİK
				.Select(p => p.ProductNo)
				.OrderByDescending(no => no)
				.FirstOrDefaultAsync();

			// Hiç ürünü yoksa 1'den başlar, varsa en sonuncunun 1 fazlası olur
			var nextProductNo = maxProductNo == 0 ? 1 : maxProductNo + 1;

			var product = new Product
			{
				UserId = userId,
				CategoryId = dto.CategoryId,
				Name = dto.Name,
				Description = dto.Description,
				UnitPrice = dto.UnitPrice,
				ProductNo = nextProductNo,
				CreatedAt = DateTime.UtcNow,
				UpdatedAt = DateTime.UtcNow
			};

			await _context.Products.AddAsync(product);
			await _context.SaveChangesAsync();

			return product.Id;
		}

		// Diğer Update, Delete, Search metotların aynen aşağıda devam ediyor...

		// 2. ÜRÜN GÜNCELLEME
		public async Task<bool> UpdateProductAsync(Guid userId, Guid productId, ProductUpdateDto dto)
		{
			var product = await _context.Products
				.FirstOrDefaultAsync(p => p.UserId == userId && p.Id == productId);

			if (product == null) return false;

			product.Name = dto.Name;
			product.Description = dto.Description;
			product.CategoryId = dto.CategoryId;
			product.UnitPrice = dto.UnitPrice;
			product.UpdatedAt = DateTime.UtcNow;

			_context.Products.Update(product);
			await _context.SaveChangesAsync();
			return true;
		}

		// 3. ÜRÜN SİLME (GÜVENLİ KONTROLÜ DAHİL)
		public async Task<(bool Success, string Message)> DeleteProductAsync(Guid userId, Guid productId)
		{
			var product = await _context.Products
				.FirstOrDefaultAsync(p => p.UserId == userId && p.Id == productId);

			if (product == null)
				return (false, "Ürün bulunamadı.");

			// Veri ve maliyet analizi güvenliği için: Satış tablosunda kaydı varsa silmeyi engelle!
			var hasSales = await _context.Sales.AnyAsync(s => s.ProductId == productId);
			if (hasSales)
				return (false, "Bu ürünün geçmiş satış kaydı/faturası bulunduğu için sistemden silinemez.");

			// Satış kaydı yoksa silebiliriz. (İlişkili stoklar CASCADE kuralıyla otomatik uçar)
			_context.Products.Remove(product);
			await _context.SaveChangesAsync();

			return (true, "Ürün ve ilişkili stok tanımları başarıyla silindi.");
		}
		public async Task<PagedResultDto<ProductResponseDto>> SearchProductsAsync(Guid userId, ProductQueryDto query)
		{
			// 1. Kullanıcıya ait ham sorguyu oluştur (Byte cast eklendi)
			var queryable = _context.Products
				.Where(p => p.UserId == userId && (byte)p.Category.Type == (byte)CategoryType.Urun)
				.Include(p => p.Category)
				.Include(p => p.Stocks)
				.AsQueryable();

			// 2. FİLTRE: Kelime Arama
			if (!string.IsNullOrWhiteSpace(query.SearchText))
			{
				string search = query.SearchText.Trim().ToLower();
				queryable = queryable.Where(p =>
					p.Name.ToLower().Contains(search) ||
					p.ProductNo.ToString() == search);
			}

			// 3. FİLTRE: Kategori Seçilmişse
			if (query.CategoryId.HasValue)
			{
				queryable = queryable.Where(p => p.CategoryId == query.CategoryId.Value);
			}

			var totalCount = await queryable.CountAsync();

			// 4. DİNAMİK SIRALAMA
			string sortBy = string.IsNullOrWhiteSpace(query.SortBy) ? "productno" : query.SortBy.ToLower();

			queryable = sortBy switch
			{
				"productno" => query.IsDescending ? queryable.OrderByDescending(p => p.ProductNo) : queryable.OrderBy(p => p.ProductNo),
				"price" => query.IsDescending ? queryable.OrderByDescending(p => p.UnitPrice) : queryable.OrderBy(p => p.UnitPrice),
				"date" => query.IsDescending ? queryable.OrderByDescending(p => p.UpdatedAt) : queryable.OrderBy(p => p.UpdatedAt),
				"quantity" => query.IsDescending ? queryable.OrderByDescending(p => p.Stocks.Sum(s => s.RemainingQuantity)) : queryable.OrderBy(p => p.Stocks.Sum(s => s.RemainingQuantity)),
				_ => queryable.OrderByDescending(p => p.ProductNo)
			};

			// 5. GÜVENLİ SEÇİM
			var rawItems = await queryable
				.Skip((query.Page - 1) * query.PageSize)
				.Take(query.PageSize)
				.Select(p => new
				{
					p.Id,
					p.ProductNo,
					p.Name,
					p.Description,
					p.UnitPrice,
					CategoryName = p.Category != null ? p.Category.Name : "Genel",
					RemainingQuantity = p.Stocks.Sum(s => s.RemainingQuantity),
					TotalCostValue = p.Stocks.Sum(s => s.RemainingQuantity * s.UnitCost),
					p.UpdatedAt
				})
				.ToListAsync();

			// 6. HAFIZADA DTO DÖNÜŞÜMÜ
			var pagedItems = rawItems.Select(p => new ProductResponseDto
			{
				Id = p.Id,
				ProductNo = p.ProductNo,
				Name = p.Name,
				Description = p.Description,
				UnitPrice = p.UnitPrice,
				CategoryName = p.CategoryName,
				RemainingQuantity = p.RemainingQuantity,
				TotalCostValue = p.TotalCostValue,
				UpdatedAt = p.UpdatedAt.ToString("dd.MM.yyyy HH:mm")
			}).ToList();

			return new PagedResultDto<ProductResponseDto>
			{
				Items = pagedItems,
				TotalCount = totalCount,
				Page = query.Page,
				TotalPages = (int)Math.Ceiling((double)totalCount / query.PageSize)
			};
		}

		public async Task<PagedResultDto<ProductResponseDto>> GetSalesTerminalProductsAsync(Guid userId, ProductSalesQueryDto query)
		{
			// 1. Kullanıcıya ait ham sorguyu oluştur (Veritabanına henüz istek atılmadı)
			var queryable = _context.Products
				.Where(p => p.UserId == userId && p.Category.Type == 0)
				.Include(p => p.Category)
				.Include(p => p.Stocks)
				.AsQueryable();

			// 2. FİLTRE: Kelime Arama (Ürün Adı veya Ürün No)
			if (!string.IsNullOrWhiteSpace(query.SearchText))
			{
				string search = query.SearchText.Trim().ToLower();
				queryable = queryable.Where(p =>
					p.Name.ToLower().Contains(search) ||
					p.ProductNo.ToString() == search);
			}

			// 3. FİLTRE: Kategoriye Göre
			if (query.CategoryId.HasValue)
			{
				queryable = queryable.Where(p => p.CategoryId == query.CategoryId.Value);
			}

			// 4. FİLTRE: Fiyat Aralığına Göre
			if (query.MinPrice.HasValue)
			{
				queryable = queryable.Where(p => p.UnitPrice >= query.MinPrice.Value);
			}
			if (query.MaxPrice.HasValue)
			{
				queryable = queryable.Where(p => p.UnitPrice <= query.MaxPrice.Value);
			}

			// 5. FİLTRE: Stokta Olup Olmamasına Göre
			if (query.InStock.HasValue)
			{
				if (query.InStock.Value)
				{
					// Sadece toplam kalan stoğu 0'dan büyük olanlar
					queryable = queryable.Where(p => p.Stocks.Sum(s => s.RemainingQuantity) > 0);
				}
				else
				{
					// Stokta kalmayanlar (0 veya daha az olanlar)
					queryable = queryable.Where(p => p.Stocks.Sum(s => s.RemainingQuantity) <= 0);
				}
			}

			// 6. SAYI TESPİTİ: Filtrelere uyan TOPLAM ürün sayısını sayfalama butonları için alıyoruz
			var totalCount = await queryable.CountAsync();

			// 7. DİNAMİK SIRALAMA
			queryable = query.SortBy.ToLower() switch
			{
				"productno" => query.IsDescending ? queryable.OrderByDescending(p => p.ProductNo) : queryable.OrderBy(p => p.ProductNo),
				"price" => query.IsDescending ? queryable.OrderByDescending(p => p.UnitPrice) : queryable.OrderBy(p => p.UnitPrice),
				"name" => query.IsDescending ? queryable.OrderByDescending(p => p.Name) : queryable.OrderBy(p => p.Name),
				"quantity" => query.IsDescending ? queryable.OrderByDescending(p => p.Stocks.Sum(s => s.RemainingQuantity)) : queryable.OrderBy(p => p.Stocks.Sum(s => s.RemainingQuantity)),
				_ => queryable.OrderByDescending(p => p.ProductNo) // Varsayılan No'ya göre
			};

			// 8. VERİYİ PARÇALAMA (PAGINATION) VE DTO'YA ÇEVİRME
			var pagedItems = await queryable
				.Skip((query.Page - 1) * query.PageSize)
				.Take(query.PageSize)
				.Select(p => new ProductResponseDto
				{
					Id = p.Id,
					ProductNo = p.ProductNo,
					Name = p.Name,
					Description = p.Description,
					UnitPrice = p.UnitPrice,
					CategoryName = p.Category != null ? p.Category.Name : "Genel",
					RemainingQuantity = p.Stocks.Sum(s => s.RemainingQuantity),
					TotalCostValue = p.Stocks.Sum(s => s.RemainingQuantity * s.UnitCost),
					UpdatedAt = p.UpdatedAt.ToString("dd.MM.yyyy HH:mm")
				})
				.ToListAsync();

			// 9. Paketi Sayfalama Detaylarıyla Kapatıp Dönüyoruz
			return new PagedResultDto<ProductResponseDto>
			{
				Items = pagedItems,
				TotalCount = totalCount,
				Page = query.Page,
				TotalPages = (int)Math.Ceiling((double)totalCount / query.PageSize)
			};
		}

		public async Task<PagedResultDto<ProductResponseDto>> GetRawMaterialsAsync(Guid userId, ProductQueryDto query)
		{
			// 1. Ana Filtre: Her iki tarafı da byte cast ederek EF Core / MySQL uyumsuzluğunu çözüyoruz
			var queryable = _context.Products
				.Where(p => p.UserId == userId && (byte)p.Category.Type == (byte)CategoryType.Hammadde)
				.Include(p => p.Category)
				.Include(p => p.Stocks)
				.AsQueryable();

			// 2. FİLTRE: Kelime Arama
			if (!string.IsNullOrWhiteSpace(query.SearchText))
			{
				string search = query.SearchText.Trim().ToLower();
				queryable = queryable.Where(p =>
					p.Name.ToLower().Contains(search) ||
					p.ProductNo.ToString() == search);
			}

			// 3. FİLTRE: Kategori
			if (query.CategoryId.HasValue)
			{
				queryable = queryable.Where(p => p.CategoryId == query.CategoryId.Value);
			}

			var totalCount = await queryable.CountAsync();

			// 4. SIRALAMA (Null Reference Hatası Engellendi)
			string sortBy = string.IsNullOrWhiteSpace(query.SortBy) ? "productno" : query.SortBy.ToLower();

			queryable = sortBy switch
			{
				"productno" => query.IsDescending ? queryable.OrderByDescending(p => p.ProductNo) : queryable.OrderBy(p => p.ProductNo),
				"price" => query.IsDescending ? queryable.OrderByDescending(p => p.UnitPrice) : queryable.OrderBy(p => p.UnitPrice),
				"date" => query.IsDescending ? queryable.OrderByDescending(p => p.UpdatedAt) : queryable.OrderBy(p => p.UpdatedAt),
				"quantity" => query.IsDescending ? queryable.OrderByDescending(p => p.Stocks.Sum(s => s.RemainingQuantity)) : queryable.OrderBy(p => p.Stocks.Sum(s => s.RemainingQuantity)),
				_ => queryable.OrderByDescending(p => p.ProductNo)
			};

			// 5. GÜVENLİ SEÇİM: DateTime formatlama hatasını önlemek için veriyi ham (anonim) çekiyoruz
			var rawItems = await queryable
				.Skip((query.Page - 1) * query.PageSize)
				.Take(query.PageSize)
				.Select(p => new
				{
					p.Id,
					p.ProductNo,
					p.Name,
					p.Description,
					p.UnitPrice,
					CategoryName = p.Category != null ? p.Category.Name : "Genel",
					RemainingQuantity = p.Stocks.Sum(s => s.RemainingQuantity),
					TotalCostValue = p.Stocks.Sum(s => s.RemainingQuantity * s.UnitCost),
					p.UpdatedAt
				})
				.ToListAsync();

			// 6. HAFIZADA DTO DÖNÜŞÜMÜ: .ToString() işlemi artık SQL'e gitmediği için asla patlamaz
			var pagedItems = rawItems.Select(p => new ProductResponseDto
			{
				Id = p.Id,
				ProductNo = p.ProductNo,
				Name = p.Name,
				Description = p.Description,
				UnitPrice = p.UnitPrice,
				CategoryName = p.CategoryName,
				RemainingQuantity = p.RemainingQuantity,
				TotalCostValue = p.TotalCostValue,
				UpdatedAt = p.UpdatedAt.ToString("dd.MM.yyyy HH:mm")
			}).ToList();

			return new PagedResultDto<ProductResponseDto>
			{
				Items = pagedItems,
				TotalCount = totalCount,
				Page = query.Page,
				TotalPages = (int)Math.Ceiling((double)totalCount / query.PageSize)
			};
		}
	}
}

