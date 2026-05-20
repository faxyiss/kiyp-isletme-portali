using StokAppApi.Entities;
using StokAppApi.Entities.Dto.Expense;
using Microsoft.EntityFrameworkCore;

namespace StokAppApi.Services
{
	public interface IExpenseService
	{
		// Kategori
		Task<ExpenseCategoryResponseDto> CreateCategoryAsync(Guid userId, ExpenseCategoryCreateDto dto);
		Task<List<ExpenseCategoryResponseDto>> GetCategoriesAsync(Guid userId);
		Task<bool> DeleteCategoryAsync(Guid userId, Guid categoryId);
		// Gider
		Task<ExpenseResponseDto> CreateExpenseAsync(Guid userId, ExpenseCreateDto dto);
		Task<ExpensePagedResponseDto> GetExpensesAsync(Guid userId, ExpenseQueryDto query);
		Task<bool> DeleteExpenseAsync(Guid userId, Guid expenseId);

		// Grafik
		Task<List<ExpenseMonthlySummaryDto>> GetMonthlySummaryAsync(Guid userId, int year);
	}

	public class ExpenseService : IExpenseService
	{
		private readonly AppDbContext _context;

		public ExpenseService(AppDbContext context)
		{
			_context = context;
		}

		// ── KATEGORİ ──────────────────────────────────────────────────

		public async Task<ExpenseCategoryResponseDto> CreateCategoryAsync(Guid userId, ExpenseCategoryCreateDto dto)
		{
			var category = new ExpenseCategory
			{
				UserId = userId,
				Name = dto.Name
			};
			_context.ExpenseCategories.Add(category);
			await _context.SaveChangesAsync();

			return new ExpenseCategoryResponseDto { Id = category.Id, Name = category.Name };
		}

		public async Task<List<ExpenseCategoryResponseDto>> GetCategoriesAsync(Guid userId)
		{
			return await _context.ExpenseCategories
				.Where(c => c.UserId == userId)
				.OrderBy(c => c.Name)
				.Select(c => new ExpenseCategoryResponseDto { Id = c.Id, Name = c.Name })
				.ToListAsync();
		}

		public async Task<bool> DeleteCategoryAsync(Guid userId, Guid categoryId)
		{
			var category = await _context.ExpenseCategories
				.FirstOrDefaultAsync(c => c.Id == categoryId && c.UserId == userId);

			if (category == null) return false;

			// İçinde kayıt varsa silmeyi engelle
			var hasExpenses = await _context.Expenses.AnyAsync(e => e.CategoryId == categoryId);
			if (hasExpenses)
				throw new Exception("Bu kategoriye ait gider kayıtları mevcut. Önce giderleri silin.");

			_context.ExpenseCategories.Remove(category);
			await _context.SaveChangesAsync();
			return true;
		}

		// ── GİDER ─────────────────────────────────────────────────────

		public async Task<ExpenseResponseDto> CreateExpenseAsync(Guid userId, ExpenseCreateDto dto)
		{
			// Validasyon: aylık giderde RecurringDay zorunlu
			if (dto.ExpenseType == ExpenseType.Monthly && !dto.RecurringDay.HasValue)
				throw new Exception("Aylık gider için ayın günü (RecurringDay) belirtilmelidir.");

			// Validasyon: dönemsel giderde EndDate zorunlu
			if (dto.ExpenseType == ExpenseType.Periodic && !dto.EndDate.HasValue)
				throw new Exception("Dönemsel gider için bitiş tarihi belirtilmelidir.");

			// Validasyon: bitiş tarihi başlangıçtan önce olamaz
			if (dto.EndDate.HasValue && dto.EndDate.Value < dto.StartDate)
				throw new Exception("Bitiş tarihi başlangıç tarihinden önce olamaz.");

			var category = await _context.ExpenseCategories
				.FirstOrDefaultAsync(c => c.Id == dto.CategoryId && c.UserId == userId);
			if (category == null)
				throw new Exception("Kategori bulunamadı.");

			var expense = new Expense
			{
				UserId = userId,
				CategoryId = dto.CategoryId,
				Title = dto.Title,
				Amount = dto.Amount,
				ExpenseType = dto.ExpenseType,
				StartDate = dto.StartDate.ToUniversalTime(),
				EndDate = dto.EndDate.HasValue ? dto.EndDate.Value.ToUniversalTime() : null,
				RecurringDay = dto.RecurringDay,
				Notes = dto.Notes
			};

			_context.Expenses.Add(expense);
			await _context.SaveChangesAsync();

			return MapToDto(expense, category.Name);
		}

		public async Task<ExpensePagedResponseDto> GetExpensesAsync(Guid userId, ExpenseQueryDto query)
		{
			var q = _context.Expenses
				.Include(e => e.Category)
				.Where(e => e.UserId == userId)
				.AsQueryable();

			// Tip filtresi
			if (query.ExpenseType.HasValue)
				q = q.Where(e => e.ExpenseType == query.ExpenseType.Value);

			// Kategori filtresi
			if (query.CategoryId.HasValue)
				q = q.Where(e => e.CategoryId == query.CategoryId.Value);

			// Ay/Yıl filtresi — StartDate'i seçilen aya ait kayıtları getir
			if (query.Month.HasValue && query.Year.HasValue)
			{
				var targetYear = query.Year.Value;
				var targetMonth = query.Month.Value;
				q = q.Where(e => e.StartDate.Year == targetYear && e.StartDate.Month == targetMonth);
			}

			var totalAmount = await q.SumAsync(e => e.Amount);
			var totalCount = await q.CountAsync();

			var rawItems = await q
				.OrderByDescending(e => e.CreatedAt)
				.Skip((query.Page - 1) * query.PageSize)
				.Take(query.PageSize)
				.Select(e => new { Expense = e, CategoryName = e.Category!.Name })
				.ToListAsync();

			var items = rawItems.Select(x => MapToDto(x.Expense, x.CategoryName)).ToList();

			return new ExpensePagedResponseDto
			{
				Items = items,
				TotalCount = totalCount,
				TotalPages = (int)Math.Ceiling((double)totalCount / query.PageSize),
				CurrentPage = query.Page,
				TotalAmount = totalAmount
			};
		}

		public async Task<bool> DeleteExpenseAsync(Guid userId, Guid expenseId)
		{
			var expense = await _context.Expenses
				.FirstOrDefaultAsync(e => e.Id == expenseId && e.UserId == userId);

			if (expense == null) return false;

			_context.Expenses.Remove(expense);
			await _context.SaveChangesAsync();
			return true;
		}

		// ── GRAFİK / ÖZET ─────────────────────────────────────────────

		public async Task<List<ExpenseMonthlySummaryDto>> GetMonthlySummaryAsync(Guid userId, int year)
		{
			// O yıla ait tüm giderleri çek
			var expenses = await _context.Expenses
				.Include(e => e.Category)
				.Where(e => e.UserId == userId &&
					(e.StartDate.Year == year ||
					 (e.EndDate.HasValue && e.EndDate.Value.Year >= year && e.StartDate.Year <= year) ||
					 e.ExpenseType == ExpenseType.Monthly))
				.ToListAsync();

			var monthlyData = new List<ExpenseMonthlySummaryDto>();
			var turkishMonths = new[] { "", "Ocak", "Şubat", "Mart", "Nisan", "Mayıs", "Haziran", "Temmuz", "Ağustos", "Eylül", "Ekim", "Kasım", "Aralık" };

			for (int month = 1; month <= 12; month++)
			{
				// O aya düşen giderleri filtrele (in-memory)
				var monthExpenses = expenses.Where(e =>
					(e.ExpenseType == ExpenseType.OneTime &&
					 e.StartDate.Year == year && e.StartDate.Month == month)
					||
					(e.ExpenseType == ExpenseType.Monthly &&
					 (e.StartDate.Year < year || (e.StartDate.Year == year && e.StartDate.Month <= month)))
					||
					(e.ExpenseType == ExpenseType.Periodic &&
					 e.StartDate <= new DateTime(year, month, DateTime.DaysInMonth(year, month)) &&
					 e.EndDate >= new DateTime(year, month, 1))
				).ToList();

				if (!monthExpenses.Any()) continue; // Boş ayları atla

				var totalAmount = monthExpenses.Sum(e => e.Amount);

				// Kategori bazlı dağılım
				var breakdown = monthExpenses
					.GroupBy(e => e.Category?.Name ?? "Diğer")
					.Select(g => new ExpenseCategoryBreakdownDto
					{
						CategoryName = g.Key,
						Amount = g.Sum(e => e.Amount),
						Percentage = totalAmount > 0
							? Math.Round((double)g.Sum(e => e.Amount) / (double)totalAmount * 100, 1)
							: 0
					})
					.OrderByDescending(b => b.Amount)
					.ToList();

				monthlyData.Add(new ExpenseMonthlySummaryDto
				{
					Year = year,
					Month = month,
					MonthLabel = $"{turkishMonths[month]} {year}",
					TotalAmount = totalAmount,
					Breakdown = breakdown
				});
			}

			return monthlyData;
		}

		// ── YARDIMCI ──────────────────────────────────────────────────

		private static ExpenseResponseDto MapToDto(Expense e, string categoryName)
		{
			var typeLabels = new Dictionary<ExpenseType, string>
			{
				{ ExpenseType.OneTime, "Tek Seferlik" },
				{ ExpenseType.Monthly, "Aylık Sabit" },
				{ ExpenseType.Periodic, "Dönemsel" }
			};

			return new ExpenseResponseDto
			{
				Id = e.Id,
				Title = e.Title,
				CategoryName = categoryName,
				Amount = e.Amount,
				ExpenseType = e.ExpenseType,
				ExpenseTypeLabel = typeLabels[e.ExpenseType],
				StartDate = e.StartDate,
				EndDate = e.EndDate,
				RecurringDay = e.RecurringDay,
				Notes = e.Notes,
				CreatedAt = e.CreatedAt
			};
		}
	}
}
