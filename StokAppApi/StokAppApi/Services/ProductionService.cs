using Microsoft.EntityFrameworkCore;
using StokAppApi.Entities;
using StokAppApi.Entities.Dto.Production;
namespace StokAppApi.Services
{
	public class ProductionService
	{
		private readonly AppDbContext _context;

		public ProductionService(AppDbContext context)
		{
			_context = context;
		}

		public async Task<bool> ProduceAsync(ProduceRequestDto dto, Guid userId)
		{
			using var transaction = await _context.Database.BeginTransactionAsync();
			try
			{
				// 1. Reçeteyi ve hammaddeleri al
				var recipe = await _context.Recipes
					.Include(r => r.RecipeItems)
					.ThenInclude(ri => ri.RawMaterial)
					.FirstOrDefaultAsync(r => r.ProductId == dto.ProductId);

				if (recipe == null)
					throw new Exception("Bu ürün için reçete tanımlı değil.");

				// 2. Hammaddeleri grupla ve miktarı quantity ile çarp
				var groupedItems = recipe.RecipeItems
					.GroupBy(ri => ri.RawMaterialId)
					.Select(g => new {
						RawMaterialId = g.Key,
						RawMaterialName = g.First().RawMaterial?.Name ?? "Bilinmeyen",
						// BUG #1 DÜZELTİLDİ: TotalRequired hesabı burada değil,
						// aşağıda decimal olarak yapılmalı — küsurat kayıplarını önler
						UnitRequired = g.Sum(ri => ri.RequiredQuantity)
					}).ToList();

				decimal totalProductionCost = 0;

				// 3. Stok kontrolü ve FIFO düşümü
				foreach (var item in groupedItems)
				{
					// BUG #2 DÜZELTİLDİ: Çarpımı burada yuvarlayarak yap
					decimal neededQty = Math.Round(item.UnitRequired * dto.Quantity, 4);

					// Önce FIFO listesini çek
					var stocksToConsume = await _context.Stocks
						.Where(s => s.ProductId == item.RawMaterialId
								 && s.UserId == userId
								 && s.RemainingQuantity > 0)
						.OrderBy(s => s.InflowDate)
						.ToListAsync();

					// BUG #3 DÜZELTİLDİ: totalStock'u SumAsync ile ayrı sorgu atmak yerine
					// zaten çektiğin listeden hesapla — iki sorgu arasında tutarsızlık olmasın
					decimal totalStock = Math.Round(stocksToConsume.Sum(s => s.RemainingQuantity), 4);

					if (totalStock < neededQty)
					{
						throw new Exception(
							$"Yetersiz stok! '{item.RawMaterialName}' için " +
							$"{neededQty} adet gerekiyor, depoda {totalStock} adet var."
						);
					}

					// FIFO düşümü
					decimal remainingToConsume = neededQty;
					foreach (var stock in stocksToConsume)
					{
						if (remainingToConsume <= 0) break;

						decimal take = Math.Min(stock.RemainingQuantity, remainingToConsume);
						stock.RemainingQuantity = Math.Round(stock.RemainingQuantity - take, 4);
						stock.UpdatedAt = DateTime.UtcNow;
						remainingToConsume = Math.Round(remainingToConsume - take, 4);

						// BUG #4 DÜZELTİLDİ: Maliyet hesabı burada yapılmalı,
						// her partinin kendi UnitCost'uyla çarpılmalı
						totalProductionCost += take * stock.UnitCost;
					}
				}

				// 4. Üretilen ürünü stoğa ekle
				// BUG #5 DÜZELTİLDİ: UnitCost = 0 yerine gerçek hammadde maliyetinden hesapla
				decimal newUnitCost = dto.Quantity > 0
					? Math.Round(totalProductionCost / dto.Quantity, 4)
					: 0;

				var newStock = new Stock
				{
					UserId = userId,
					ProductId = dto.ProductId,
					InflowQuantity = dto.Quantity,
					RemainingQuantity = dto.Quantity,
					UnitCost = newUnitCost,
					InflowDate = DateTime.UtcNow,
					CreatedAt = DateTime.UtcNow,
					UpdatedAt = DateTime.UtcNow
				};

				_context.Stocks.Add(newStock);

				var log = new ProductionLog
				{
					UserId = userId,
					ProductId = dto.ProductId,
					Quantity = dto.Quantity,
					TotalCost = Math.Round(totalProductionCost, 4),
					ProducedAt = DateTime.UtcNow
				};
				_context.ProductionLogs.Add(log);

				await _context.SaveChangesAsync();
				await transaction.CommitAsync();
				return true;
			}
			catch
			{
				await transaction.RollbackAsync();
				throw;
			}
		}
		public async Task<RecipeResponseDto> CreateRecipeAsync(RecipeCreateDto dto, Guid userId)
		{
			// Kullanıcının böyle bir ürünü var mı kontrol et
			var product = await _context.Products
				.FirstOrDefaultAsync(p => p.Id == dto.ProductId && p.UserId == userId);

			if (product == null)
				throw new Exception("Nihai ürün bulunamadı veya bu ürüne işlem yapma yetkiniz yok.");

			// Bu ürün için zaten bir reçete tanımlı mı kontrol et
			var existingRecipe = await _context.Recipes
				.FirstOrDefaultAsync(r => r.ProductId == dto.ProductId);

			if (existingRecipe != null)
				throw new Exception("Bu ürün için zaten bir reçete tanımlı. Lütfen önce mevcut reçeteyi silin.");

			// Döngüsel bağımlılık kontrolü: Ürün kendi kendisinin hammaddesi olamaz
			if (dto.Items.Any(i => i.RawMaterialId == dto.ProductId))
				throw new Exception("Bir ürün kendi kendisinin hammaddesi olarak kullanılamaz.");

			// Reçeteyi oluştur
			var recipe = new Recipe
			{
				ProductId = dto.ProductId,
				CreatedAt = DateTime.UtcNow,
				UpdatedAt = DateTime.UtcNow,
				RecipeItems = dto.Items.Select(item => new RecipeItem
				{
					RawMaterialId = item.RawMaterialId,
					RequiredQuantity = item.RequiredQuantity
				}).ToList()
			};

			_context.Recipes.Add(recipe);
			await _context.SaveChangesAsync();

			return await GetRecipeByIdAsync(recipe.Id, userId);
		}

		// 2. Kullanıcıya Ait Tüm Reçeteleri Listeleme
		public async Task<List<RecipeResponseDto>> GetRecipesAsync(Guid userId)
		{
			var recipes = await _context.Recipes
				.Include(r => r.Product)
				.Include(r => r.RecipeItems)
					.ThenInclude(ri => ri.RawMaterial)
				.Where(r => r.Product!.UserId == userId)
				.OrderByDescending(r => r.CreatedAt)
				.ToListAsync();

			return recipes.Select(r => new RecipeResponseDto
			{
				Id = r.Id,
				ProductId = r.ProductId,
				ProductName = r.Product?.Name ?? "Bilinmiyor",
				CreatedAt = r.CreatedAt,
				Items = r.RecipeItems.Select(ri => new RecipeItemResponseDto
				{
					RawMaterialId = ri.RawMaterialId,
					RawMaterialName = ri.RawMaterial?.Name ?? "Bilinmiyor",
					RequiredQuantity = ri.RequiredQuantity
				}).ToList()
			}).ToList();
		}

		// 3. Tek Bir Reçeteyi Getirme (Yardımcı Metot)
		public async Task<RecipeResponseDto> GetRecipeByIdAsync(Guid recipeId, Guid userId)
		{
			var recipe = await _context.Recipes
				.Include(r => r.Product)
				.Include(r => r.RecipeItems)
					.ThenInclude(ri => ri.RawMaterial)
				.FirstOrDefaultAsync(r => r.Id == recipeId && r.Product!.UserId == userId);

			if (recipe == null)
				throw new Exception("Reçete bulunamadı.");

			return new RecipeResponseDto
			{
				Id = recipe.Id,
				ProductId = recipe.ProductId,
				ProductName = recipe.Product?.Name ?? "Bilinmiyor",
				CreatedAt = recipe.CreatedAt,
				Items = recipe.RecipeItems.Select(ri => new RecipeItemResponseDto
				{
					RawMaterialId = ri.RawMaterialId,
					RawMaterialName = ri.RawMaterial?.Name ?? "Bilinmiyor",
					RequiredQuantity = ri.RequiredQuantity
				}).ToList()
			};
		}

		// 4. Reçete Silme
		public async Task<bool> DeleteRecipeAsync(Guid recipeId, Guid userId)
		{
			var recipe = await _context.Recipes
				.Include(r => r.Product)
				.FirstOrDefaultAsync(r => r.Id == recipeId && r.Product!.UserId == userId);

			if (recipe == null)
				throw new Exception("Silinmek istenen reçete bulunamadı veya yetkiniz yok.");

			_context.Recipes.Remove(recipe);
			await _context.SaveChangesAsync();

			return true;
		}

		public async Task<ProductionLogPagedResponseDto> GetProductionLogsAsync(Guid userId, ProductionLogQueryDto query)
		{
			var q = _context.ProductionLogs
				.Include(l => l.Product)
				.Where(l => l.UserId == userId)
				.AsQueryable();

			// 1. Ürün adı araması
			if (!string.IsNullOrWhiteSpace(query.SearchText))
			{
				string search = query.SearchText.Trim().ToLower();
				q = q.Where(l => l.Product!.Name.ToLower().Contains(search));
			}

			// 2. Tarih aralığı filtresi
			if (query.StartDate.HasValue)
				q = q.Where(l => l.ProducedAt >= query.StartDate.Value.ToUniversalTime());

			if (query.EndDate.HasValue)
				q = q.Where(l => l.ProducedAt <= query.EndDate.Value.ToUniversalTime().AddDays(1).AddSeconds(-1));

			// 3. Özet istatistikleri filtreye uyan TÜM kayıtlardan hesapla (sayfalamadan önce)
			var summaryTotalQuantity = await q.SumAsync(l => l.Quantity);
			var summaryTotalCost = await q.SumAsync(l => l.TotalCost);
			var summaryUniqueProducts = await q.Select(l => l.ProductId).Distinct().CountAsync();
			var totalCount = await q.CountAsync();

			// 4. Sıralama
			q = query.SortBy.ToLower() switch
			{
				"quantity" => query.IsDescending
					? q.OrderByDescending(l => l.Quantity)
					: q.OrderBy(l => l.Quantity),
				"totalcost" => query.IsDescending
					? q.OrderByDescending(l => l.TotalCost)
					: q.OrderBy(l => l.TotalCost),
				_ => query.IsDescending // "date" varsayılan
					? q.OrderByDescending(l => l.ProducedAt)
					: q.OrderBy(l => l.ProducedAt)
			};

			// 5. Sayfalama ve DTO dönüşümü
			var rawItems = await q
				.Skip((query.Page - 1) * query.PageSize)
				.Take(query.PageSize)
				.Select(l => new
				{
					l.Id,
					ProductName = l.Product!.Name,
					l.Quantity,
					l.TotalCost,
					l.ProducedAt
				})
				.ToListAsync();

			var items = rawItems.Select(l => new ProductionLogResponseDto
			{
				Id = l.Id,
				ProductName = l.ProductName,
				Quantity = l.Quantity,
				TotalCost = l.TotalCost,
				UnitCost = l.Quantity > 0 ? Math.Round(l.TotalCost / l.Quantity, 4) : 0,
				ProducedAt = l.ProducedAt
			}).ToList();

			return new ProductionLogPagedResponseDto
			{
				Items = items,
				TotalCount = totalCount,
				TotalPages = (int)Math.Ceiling((double)totalCount / query.PageSize),
				CurrentPage = query.Page,
				SummaryTotalQuantity = summaryTotalQuantity,
				SummaryTotalCost = summaryTotalCost,
				SummaryUniqueProducts = summaryUniqueProducts
			};
		}
	}
}
