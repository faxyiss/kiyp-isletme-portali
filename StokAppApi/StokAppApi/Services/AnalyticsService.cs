using Microsoft.EntityFrameworkCore;
using StokAppApi.Entities;

namespace StokAppApi.Services
{
    public interface IAnalyticsService
    {
        Task<OverviewResult> GetOverviewAsync(Guid userId, string range);
        Task<SalesAnalyticsResult> GetSalesAnalyticsAsync(Guid userId, string range);
        Task<StockAnalyticsResult> GetStockAnalyticsAsync(Guid userId, string range);
        Task<CustomerAnalyticsResult> GetCustomerAnalyticsAsync(Guid userId, string range);
        Task<ExpenseAnalyticsResult> GetExpenseAnalyticsAsync(Guid userId, string range);
        Task<ProductionAnalyticsResult> GetProductionAnalyticsAsync(Guid userId, string range);
        Task<HRAnalyticsResult> GetHRAnalyticsAsync(Guid userId, string range);
        Task<PeriodComparisonResult> GetPeriodComparisonAsync(Guid userId, string range);
        Task<BreakevenResult> GetBreakevenAsync(Guid userId);
        Task<ReceivablesAgingResult> GetReceivablesAgingAsync(Guid userId);
        Task<RFMResult> GetRFMAsync(Guid userId);
        Task<StockTurnoverResult> GetStockTurnoverAsync(Guid userId);
        // Yeni analizler
        Task<ProductPortfolioResult> GetProductPortfolioAsync(Guid userId, string range);
        Task<ExpenseRatioResult> GetExpenseRatioAsync(Guid userId);
        Task<CashFlowProjectionResult> GetCashFlowProjectionAsync(Guid userId);
        Task<ProductionProfitabilityResult> GetProductionProfitabilityAsync(Guid userId, string range);
        Task<MaterialPriceTrendResult> GetMaterialPriceTrendAsync(Guid userId);
    }

    // ─── Dönüş tipleri ───────────────────────────────────────────────────────

    public record MonthlyRevenueExpense(string Label, decimal Revenue, decimal Expense);

    public record OverviewResult(
        decimal TotalRevenue,
        decimal TotalProfit,
        decimal TotalExpenses,
        decimal NetProfit,
        decimal StockValue,
        decimal TotalReceivables,
        IList<MonthlyRevenueExpense> RevenueVsExpense
    );

    public record DailyRevenue(string Date, decimal Revenue, decimal Profit);
    public record PaymentBreakdown(string Label, decimal Value);
    public record CategorySale(string Category, decimal Revenue, decimal Profit);
    public record TopProduct(string Name, decimal Quantity, decimal Revenue, decimal Profit);

    public record SalesAnalyticsResult(
        decimal TotalRevenue,
        decimal TotalProfit,
        int TotalOrders,
        decimal AvgOrderValue,
        IList<DailyRevenue> DailyRevenue,
        IList<PaymentBreakdown> PaymentBreakdown,
        IList<CategorySale> CategorySales,
        IList<TopProduct> TopProducts
    );

    public record StockLevelItem(string ProductName, string CategoryName, decimal RemainingQty, decimal UnitCost, decimal TotalValue, bool IsCritical);
    public record StockCategoryValue(string Category, decimal TotalValue);
    public record FastMovingItem(string ProductName, decimal SoldQty, decimal RemainingQty);

    public record StockAnalyticsResult(
        decimal TotalStockValue,
        int TotalProducts,
        int CriticalStockCount,
        IList<StockLevelItem> StockLevels,
        IList<StockCategoryValue> ValueByCategory,
        IList<FastMovingItem> FastMoving,
        IList<FastMovingItem> SlowMoving
    );

    public record TopCustomerItem(string FullName, string? Phone, decimal TotalRevenue, int OrderCount, decimal AvgOrder);
    public record DebtCustomerItem(string FullName, string? Phone, decimal Balance, DateTime CreatedAt);
    public record MonthlyNewCustomer(string Label, int Count);

    public record CustomerAnalyticsResult(
        int TotalCustomers,
        int DebtCustomerCount,
        decimal TotalReceivables,
        decimal AvgOrderValue,
        IList<TopCustomerItem> TopCustomers,
        IList<DebtCustomerItem> DebtCustomers,
        IList<MonthlyNewCustomer> NewCustomerTrend
    );

    public record ExpenseCategoryBreakdown(string Category, decimal Amount);
    public record MonthlyExpenseTrend(string Label, decimal Expenses, decimal Payroll);
    public record TopExpenseItem(string Title, string Category, string Type, decimal Amount, DateTime Date);

    public record ExpenseAnalyticsResult(
        decimal TotalExpenses,
        decimal TotalPayroll,
        decimal TotalCombined,
        int ExpenseCount,
        IList<ExpenseCategoryBreakdown> CategoryBreakdown,
        IList<MonthlyExpenseTrend> MonthlyTrend,
        IList<TopExpenseItem> TopExpenses
    );

    public record ProductionProductSummary(
        string ProductName, decimal TotalQuantity, decimal TotalCost,
        decimal AvgUnitCost, int BatchCount, DateTime LastProducedAt
    );
    public record MonthlyProductionTrend(string Label, decimal Quantity, decimal Cost);
    public record DailyProductionPoint(string Date, decimal Quantity, decimal Cost, int BatchCount);
    public record RawMaterialConsumptionItem(string MaterialName, decimal ConsumedQty);

    public record ProductionAnalyticsResult(
        decimal TotalQuantity,
        decimal TotalCost,
        decimal AvgUnitCost,
        int UniqueProductCount,
        int TotalBatches,
        IList<ProductionProductSummary> ProductSummary,
        IList<MonthlyProductionTrend> MonthlyTrend,
        IList<DailyProductionPoint> DailyProduction,
        IList<RawMaterialConsumptionItem> RawMaterialConsumption
    );

    public record MonthlyPayrollTrend(string Label, decimal Gross, decimal Net, decimal EmployerCost);
    public record EmployeePayrollSummary(
        string FullName, string Position, bool IsActive,
        decimal GrossSalary, decimal AvgNetSalary, decimal AvgEmployerCost,
        int PaymentCount, int TotalLeaveDays
    );
    public record EmployeeLeaveRecord(string FullName, string Position, DateTime StartDate, DateTime EndDate, int DayCount, string? Reason);
    public record SalaryComponentBreakdown(decimal Net, decimal EmployeeTax, decimal EmployerExtra);

    public record HRAnalyticsResult(
        int ActiveEmployeeCount,
        int TotalEmployeeCount,
        decimal TotalGrossSalary,
        decimal TotalEmployerCost,
        decimal AvgNetSalary,
        int TotalLeaveDays,
        SalaryComponentBreakdown SalaryComponents,
        IList<MonthlyPayrollTrend> MonthlyTrend,
        IList<EmployeePayrollSummary> EmployeeSummaries,
        IList<EmployeeLeaveRecord> LeaveRecords
    );

    // ── Dönemsel Karşılaştırma ────────────────────────────────────────────────
    public record PeriodMetric(decimal Current, decimal Previous, decimal ChangePercent);
    public record PeriodComparisonResult(
        PeriodMetric Revenue, PeriodMetric Profit,
        PeriodMetric Expenses, PeriodMetric NetProfit, PeriodMetric OrderCount
    );

    // ── Başabaş Noktası ───────────────────────────────────────────────────────
    public record BreakevenResult(
        decimal MonthlyFixedCosts, decimal VariableCostRatio,
        decimal BreakevenRevenue, decimal CurrentMonthlyRevenue,
        decimal SafetyMargin, decimal SafetyMarginPercent, bool IsAboveBreakeven
    );

    // ── Veresiye Yaşlandırma ──────────────────────────────────────────────────
    public record AgingBucket(string Label, decimal Amount, int CustomerCount);
    public record AgingCustomerDetail(string FullName, string? Phone, decimal Balance, int DaysOld, string Bucket);
    public record ReceivablesAgingResult(
        decimal TotalReceivables,
        IList<AgingBucket> Buckets,
        IList<AgingCustomerDetail> CustomerDetails
    );

    // ── RFM Müşteri Segmentasyonu ─────────────────────────────────────────────
    public record RFMCustomerItem(
        string FullName, string? Phone,
        int RScore, int FScore, int MScore, int TotalScore,
        string Segment, string SegmentColor,
        decimal TotalRevenue, int OrderCount, DateTime LastOrderDate
    );
    public record RFMResult(
        int TotalCustomers, int Champions, int Loyal, int AtRisk, int Lost,
        IList<RFMCustomerItem> Customers
    );

    // ── Stok Devir Hızı ───────────────────────────────────────────────────────
    public record StockTurnoverItem(
        string ProductName, string CategoryName,
        decimal TurnoverRate, decimal DaysToSell,
        decimal SoldQty30d, decimal RemainingQty,
        decimal BoundCapital, bool IsDeadStock
    );
    public record StockTurnoverResult(
        decimal AvgTurnoverRate, int DeadStockCount,
        decimal DeadStockValue, decimal TotalBoundCapital,
        IList<StockTurnoverItem> Items
    );

    // ── Ürün Portföy Matrisi ──────────────────────────────────────────────────
    public record PortfolioProductItem(
        string ProductName, string CategoryName,
        decimal Revenue, decimal RevenueShare, decimal GrowthRate,
        string Quadrant, string QuadrantColor
    );
    public record ProductPortfolioResult(
        decimal TotalRevenue, decimal AvgGrowthRate, decimal AvgRevenueShare,
        IList<PortfolioProductItem> Products
    );

    // ── Gider Oranı Analizi ───────────────────────────────────────────────────
    public record MonthlyExpenseRatio(
        string Label, decimal Revenue, decimal Expenses, decimal Payroll,
        decimal TotalCost, decimal Ratio, decimal OperatingProfit
    );
    public record ExpenseRatioResult(
        decimal AvgExpenseRatio, decimal AvgOperatingMargin,
        decimal TotalRevenue, decimal TotalExpenses,
        IList<MonthlyExpenseRatio> Monthly
    );

    // ── Nakit Akış Projeksiyonu ───────────────────────────────────────────────
    public record CashFlowPoint(
        string Label, decimal Revenue, decimal Expenses,
        decimal NetCashFlow, bool IsProjection
    );
    public record CashFlowProjectionResult(
        decimal AvgMonthlyRevenue, decimal AvgMonthlyExpenses,
        decimal AvgNetCashFlow, decimal ProjectedAnnualRevenue,
        IList<CashFlowPoint> Points
    );

    // ── Üretim Karlılık Marjı ─────────────────────────────────────────────────
    public record ProductProfitabilityItem(
        string ProductName, string CategoryName,
        decimal ProductionCost, decimal SalesRevenue, decimal SalesProfit,
        decimal GrossMarginPercent, decimal ProductionQty, decimal SoldQty
    );
    public record ProductionProfitabilityResult(
        decimal TotalProductionCost, decimal TotalSalesRevenue,
        decimal TotalProfit, decimal OverallMargin,
        IList<ProductProfitabilityItem> Products
    );

    // ── Hammadde Fiyat Trendi ─────────────────────────────────────────────────
    public record MaterialPricePoint(
        string Label, decimal AvgPrice, decimal MinPrice, decimal MaxPrice, int InflowCount
    );
    public record MaterialPriceSeries(string MaterialName, IList<MaterialPricePoint> Points);
    public record MaterialPriceTrendResult(
        IList<string> Labels,
        IList<MaterialPriceSeries> Series
    );

    // ─── Servis implementasyonu ───────────────────────────────────────────────

    public class AnalyticsService : IAnalyticsService
    {
        private readonly AppDbContext _db;

        public AnalyticsService(AppDbContext db)
        {
            _db = db;
        }

        private static DateTime GetStartDate(string range) => range switch
        {
            "7d"   => DateTime.UtcNow.AddDays(-7),
            "30d"  => DateTime.UtcNow.AddDays(-30),
            "90d"  => DateTime.UtcNow.AddDays(-90),
            "year" => new DateTime(DateTime.UtcNow.Year, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            _      => DateTime.UtcNow.AddDays(-30),
        };

        public async Task<OverviewResult> GetOverviewAsync(Guid userId, string range)
        {
            var startDate = GetStartDate(range);

            var totalRevenue = await _db.Sales
                .Where(s => s.UserId == userId && s.SaleDate >= startDate)
                .SumAsync(s => (decimal?)s.TotalAmount) ?? 0m;

            var totalProfit = await _db.Sales
                .Where(s => s.UserId == userId && s.SaleDate >= startDate)
                .SumAsync(s => (decimal?)(s.Quantity * (s.SalePrice - s.UnitCostAtSale))) ?? 0m;

            var totalExpenses = await _db.Expenses
                .Where(e => e.UserId == userId && e.CreatedAt >= startDate)
                .SumAsync(e => (decimal?)e.Amount) ?? 0m;

            var overviewEmpIds = await _db.Employees
                .Where(e => e.UserId == userId)
                .Select(e => e.Id)
                .ToListAsync();
            var allOverviewPayments = await _db.SalaryPayments
                .Where(sp => overviewEmpIds.Contains(sp.EmployeeId))
                .ToListAsync();

            var totalPayroll = allOverviewPayments
                .Where(sp => new DateTime(sp.Year, sp.Month, 1) >= startDate)
                .Sum(sp => (decimal?)sp.TotalEmployerCost) ?? 0m;

            var totalExpensesAll = totalExpenses + totalPayroll;
            var netProfit = totalProfit - totalExpensesAll;

            var stockValue = await _db.Stocks
                .Where(s => s.UserId == userId && s.RemainingQuantity > 0)
                .SumAsync(s => (decimal?)(s.RemainingQuantity * s.UnitCost)) ?? 0m;

            var totalReceivables = await _db.Customers
                .Where(c => c.UserId == userId && c.CurrentBalance > 0)
                .SumAsync(c => (decimal?)c.CurrentBalance) ?? 0m;

            // Son 12 ay — gelir vs gider
            var twelveMonthsAgo = DateTime.UtcNow.AddMonths(-11);
            var monthStart = new DateTime(twelveMonthsAgo.Year, twelveMonthsAgo.Month, 1, 0, 0, 0, DateTimeKind.Utc);

            var monthlySales = await _db.Sales
                .Where(s => s.UserId == userId && s.SaleDate >= monthStart)
                .GroupBy(s => new { s.SaleDate.Year, s.SaleDate.Month })
                .Select(g => new { g.Key.Year, g.Key.Month, Revenue = g.Sum(s => s.TotalAmount) })
                .ToListAsync();

            var monthlyExpenses = await _db.Expenses
                .Where(e => e.UserId == userId && e.CreatedAt >= monthStart)
                .GroupBy(e => new { e.CreatedAt.Year, e.CreatedAt.Month })
                .Select(g => new { g.Key.Year, g.Key.Month, Expense = g.Sum(e => e.Amount) })
                .ToListAsync();

            var monthlyPayrolls = allOverviewPayments
                .GroupBy(sp => new { sp.Year, sp.Month })
                .Select(g => new { g.Key.Year, g.Key.Month, Payroll = g.Sum(sp => sp.TotalEmployerCost) })
                .ToList();

            string[] turkishMonths = ["Oca", "Şub", "Mar", "Nis", "May", "Haz", "Tem", "Ağu", "Eyl", "Eki", "Kas", "Ara"];

            var revenueVsExpense = Enumerable.Range(0, 12)
                .Select(i => DateTime.UtcNow.AddMonths(-11 + i))
                .Select(d => new MonthlyRevenueExpense(
                    Label: turkishMonths[d.Month - 1],
                    Revenue: monthlySales.FirstOrDefault(s => s.Year == d.Year && s.Month == d.Month)?.Revenue ?? 0m,
                    Expense: (monthlyExpenses.FirstOrDefault(e => e.Year == d.Year && e.Month == d.Month)?.Expense ?? 0m)
                           + (monthlyPayrolls.FirstOrDefault(p => p.Year == d.Year && p.Month == d.Month)?.Payroll ?? 0m)
                ))
                .ToList();

            return new OverviewResult(totalRevenue, totalProfit, totalExpensesAll, netProfit, stockValue, totalReceivables, revenueVsExpense);
        }

        public async Task<SalesAnalyticsResult> GetSalesAnalyticsAsync(Guid userId, string range)
        {
            var startDate = GetStartDate(range);

            var sales = await _db.Sales
                .Where(s => s.UserId == userId && s.SaleDate >= startDate)
                .Include(s => s.Product)
                    .ThenInclude(p => p!.Category)
                .ToListAsync();

            var dailyRevenue = sales
                .GroupBy(s => s.SaleDate.Date)
                .OrderBy(g => g.Key)
                .Select(g => new DailyRevenue(
                    Date: g.Key.ToString("dd MMM", new System.Globalization.CultureInfo("tr-TR")),
                    Revenue: g.Sum(s => s.TotalAmount),
                    Profit: g.Sum(s => s.Quantity * (s.SalePrice - s.UnitCostAtSale))
                ))
                .ToList();

            var paymentBreakdown = new List<PaymentBreakdown>
            {
                new("Nakit",       sales.Where(s => s.PaymentType == PaymentType.Nakit).Sum(s => s.TotalAmount)),
                new("Kredi Kartı", sales.Where(s => s.PaymentType == PaymentType.KrediKarti).Sum(s => s.TotalAmount)),
                new("Veresiye",    sales.Where(s => s.PaymentType == PaymentType.Veresiye).Sum(s => s.TotalAmount)),
            };

            var categorySales = sales
                .Where(s => s.Product?.Category != null)
                .GroupBy(s => s.Product!.Category!.Name)
                .Select(g => new CategorySale(
                    Category: g.Key,
                    Revenue: g.Sum(s => s.TotalAmount),
                    Profit: g.Sum(s => s.Quantity * (s.SalePrice - s.UnitCostAtSale))
                ))
                .OrderByDescending(x => x.Revenue)
                .ToList();

            var topProducts = sales
                .Where(s => s.Product != null)
                .GroupBy(s => new { s.ProductId, s.Product!.Name })
                .Select(g => new TopProduct(
                    Name: g.Key.Name,
                    Quantity: g.Sum(s => s.Quantity),
                    Revenue: g.Sum(s => s.TotalAmount),
                    Profit: g.Sum(s => s.Quantity * (s.SalePrice - s.UnitCostAtSale))
                ))
                .OrderByDescending(x => x.Revenue)
                .Take(10)
                .ToList();

            var totalRevenue = sales.Sum(s => s.TotalAmount);
            var totalProfit = sales.Sum(s => s.Quantity * (s.SalePrice - s.UnitCostAtSale));
            var totalOrders = sales.Count;
            var avgOrderValue = totalOrders > 0 ? totalRevenue / totalOrders : 0m;

            return new SalesAnalyticsResult(totalRevenue, totalProfit, totalOrders, avgOrderValue, dailyRevenue, paymentBreakdown, categorySales, topProducts);
        }

        public async Task<StockAnalyticsResult> GetStockAnalyticsAsync(Guid userId, string range)
        {
            // Tüm aktif stokları ürün ve kategoriyle birlikte çek
            var stocks = await _db.Stocks
                .Where(s => s.UserId == userId && s.RemainingQuantity > 0)
                .Include(s => s.Product)
                    .ThenInclude(p => p!.Category)
                .ToListAsync();

            // Ürün bazlı stok özeti
            var stockByProduct = stocks
                .Where(s => s.Product != null)
                .GroupBy(s => new { s.ProductId, Name = s.Product!.Name, Category = s.Product.Category?.Name ?? "Kategorisiz", CategoryType = s.Product.Category?.Type ?? CategoryType.Urun })
                .Select(g => new
                {
                    g.Key.Name,
                    g.Key.Category,
                    g.Key.CategoryType,
                    RemainingQty = g.Sum(s => s.RemainingQuantity),
                    TotalValue = g.Sum(s => s.RemainingQuantity * s.UnitCost),
                    UnitCost = g.OrderByDescending(s => s.CreatedAt).First().UnitCost,
                })
                .OrderByDescending(x => x.TotalValue)
                .ToList();

            // Kritik stok eşiği: toplam stok miktarının %5'inden az kalan ürünler
            var criticalThreshold = stockByProduct.Count > 0
                ? stockByProduct.Average(s => s.RemainingQty) * 0.2m
                : 0m;

            var stockLevels = stockByProduct.Select(s => new StockLevelItem(
                ProductName: s.Name,
                CategoryName: s.Category,
                RemainingQty: s.RemainingQty,
                UnitCost: s.UnitCost,
                TotalValue: s.TotalValue,
                IsCritical: s.RemainingQty <= criticalThreshold
            )).ToList();

            // Kategori bazlı stok değeri
            var valueByCategory = stockByProduct
                .GroupBy(s => s.Category)
                .Select(g => new StockCategoryValue(g.Key, g.Sum(s => s.TotalValue)))
                .OrderByDescending(x => x.TotalValue)
                .ToList();

            // Seçili range'de satılan ürünler → hızlı/yavaş eriyen
            var startDate = GetStartDate(range);
            var salesInRange = await _db.Sales
                .Where(s => s.UserId == userId && s.SaleDate >= startDate)
                .Include(s => s.Product)
                .ToListAsync();

            var soldByProduct = salesInRange
                .Where(s => s.Product != null)
                .GroupBy(s => new { s.ProductId, Name = s.Product!.Name })
                .Select(g => new { g.Key.Name, SoldQty = g.Sum(s => s.Quantity) })
                .ToList();

            // Stok listesiyle eşleştir (hammaddeler hızlı/yavaş eriyen analizine dahil edilmez)
            var combined = stockByProduct
                .Where(sp => sp.CategoryType != CategoryType.Hammadde)
                .Select(sp => new
                {
                    sp.Name,
                    sp.RemainingQty,
                    SoldQty = soldByProduct.FirstOrDefault(s => s.Name == sp.Name)?.SoldQty ?? 0m,
                }).ToList();

            var fastMoving = combined
                .Where(x => x.SoldQty > 0)
                .OrderByDescending(x => x.SoldQty)
                .Take(8)
                .Select(x => new FastMovingItem(x.Name, x.SoldQty, x.RemainingQty))
                .ToList();

            var slowMoving = combined
                .OrderBy(x => x.SoldQty)
                .Take(8)
                .Select(x => new FastMovingItem(x.Name, x.SoldQty, x.RemainingQty))
                .ToList();

            return new StockAnalyticsResult(
                TotalStockValue: stocks.Sum(s => s.RemainingQuantity * s.UnitCost),
                TotalProducts: stockByProduct.Count,
                CriticalStockCount: stockLevels.Count(s => s.IsCritical),
                StockLevels: stockLevels,
                ValueByCategory: valueByCategory,
                FastMoving: fastMoving,
                SlowMoving: slowMoving
            );
        }

        public async Task<CustomerAnalyticsResult> GetCustomerAnalyticsAsync(Guid userId, string range)
        {
            var startDate = GetStartDate(range);

            // Toplam müşteri sayısı
            var totalCustomers = await _db.Customers
                .CountAsync(c => c.UserId == userId);

            // Veresiye (borçlu) müşteriler
            var debtCustomers = await _db.Customers
                .Where(c => c.UserId == userId && c.CurrentBalance > 0)
                .OrderByDescending(c => c.CurrentBalance)
                .Select(c => new DebtCustomerItem(
                    c.FirstName + " " + c.LastName,
                    c.PhoneNumber,
                    c.CurrentBalance,
                    c.CreatedAt
                ))
                .ToListAsync();

            var totalReceivables = debtCustomers.Sum(d => d.Balance);

            // Range içindeki satışlar (müşterili olanlar)
            var sales = await _db.Sales
                .Where(s => s.UserId == userId && s.SaleDate >= startDate && s.CustomerId != null)
                .Include(s => s.Customer)
                .ToListAsync();

            var avgOrderValue = sales.Count > 0 ? sales.Average(s => s.TotalAmount) : 0m;

            // En çok alışveriş yapan 10 müşteri
            var topCustomers = sales
                .Where(s => s.Customer != null)
                .GroupBy(s => new { s.CustomerId, Name = s.Customer!.FirstName + " " + s.Customer.LastName, s.Customer.PhoneNumber })
                .Select(g => new TopCustomerItem(
                    FullName: g.Key.Name,
                    Phone: g.Key.PhoneNumber,
                    TotalRevenue: g.Sum(s => s.TotalAmount),
                    OrderCount: g.Count(),
                    AvgOrder: g.Average(s => s.TotalAmount)
                ))
                .OrderByDescending(x => x.TotalRevenue)
                .Take(10)
                .ToList();

            // Son 12 ay yeni müşteri trendi
            string[] turkishMonths = ["Oca", "Şub", "Mar", "Nis", "May", "Haz", "Tem", "Ağu", "Eyl", "Eki", "Kas", "Ara"];
            var twelveMonthsAgo = DateTime.UtcNow.AddMonths(-11);
            var monthStart = new DateTime(twelveMonthsAgo.Year, twelveMonthsAgo.Month, 1, 0, 0, 0, DateTimeKind.Utc);

            var monthlyCounts = await _db.Customers
                .Where(c => c.UserId == userId && c.CreatedAt >= monthStart)
                .GroupBy(c => new { c.CreatedAt.Year, c.CreatedAt.Month })
                .Select(g => new { g.Key.Year, g.Key.Month, Count = g.Count() })
                .ToListAsync();

            var newCustomerTrend = Enumerable.Range(0, 12)
                .Select(i => DateTime.UtcNow.AddMonths(-11 + i))
                .Select(d => new MonthlyNewCustomer(
                    Label: turkishMonths[d.Month - 1],
                    Count: monthlyCounts.FirstOrDefault(m => m.Year == d.Year && m.Month == d.Month)?.Count ?? 0
                ))
                .ToList();

            return new CustomerAnalyticsResult(
                TotalCustomers: totalCustomers,
                DebtCustomerCount: debtCustomers.Count,
                TotalReceivables: totalReceivables,
                AvgOrderValue: avgOrderValue,
                TopCustomers: topCustomers,
                DebtCustomers: debtCustomers,
                NewCustomerTrend: newCustomerTrend
            );
        }

        public async Task<ExpenseAnalyticsResult> GetExpenseAnalyticsAsync(Guid userId, string range)
        {
            var startDate = GetStartDate(range);

            // Giderler (CreatedAt'e göre filtrele — mevcut proje pattern'ı)
            var expenses = await _db.Expenses
                .Where(e => e.UserId == userId && e.CreatedAt >= startDate)
                .Include(e => e.Category)
                .ToListAsync();

            // Maaş ödemeleri
            var expEmpIds = await _db.Employees
                .Where(e => e.UserId == userId)
                .Select(e => e.Id)
                .ToListAsync();
            var allSalaryPayments = await _db.SalaryPayments
                .Where(sp => expEmpIds.Contains(sp.EmployeeId))
                .ToListAsync();
            var salaryPayments = allSalaryPayments
                .Where(sp => new DateTime(sp.Year, sp.Month, 1) >= startDate)
                .ToList();

            var totalExpenses = expenses.Sum(e => e.Amount);
            var totalPayroll = salaryPayments.Sum(sp => sp.TotalEmployerCost);
            var totalCombined = totalExpenses + totalPayroll;

            // Kategori bazlı dağılım
            var categoryBreakdown = expenses
                .GroupBy(e => e.Category?.Name ?? "Kategorisiz")
                .Select(g => new ExpenseCategoryBreakdown(g.Key, g.Sum(e => e.Amount)))
                .OrderByDescending(x => x.Amount)
                .ToList();

            // En yüksek 10 gider
            string[] expenseTypeLabels = ["Tek Seferlik", "Aylık Sabit", "Dönemsel"];
            var topExpenses = expenses
                .OrderByDescending(e => e.Amount)
                .Take(10)
                .Select(e => new TopExpenseItem(
                    Title: e.Title,
                    Category: e.Category?.Name ?? "Kategorisiz",
                    Type: expenseTypeLabels[(int)e.ExpenseType],
                    Amount: e.Amount,
                    Date: e.CreatedAt
                ))
                .ToList();

            // Son 12 ay aylık trend
            string[] turkishMonths = ["Oca", "Şub", "Mar", "Nis", "May", "Haz", "Tem", "Ağu", "Eyl", "Eki", "Kas", "Ara"];
            var twelveMonthsAgo = DateTime.UtcNow.AddMonths(-11);
            var monthStart = new DateTime(twelveMonthsAgo.Year, twelveMonthsAgo.Month, 1, 0, 0, 0, DateTimeKind.Utc);

            var monthlyExpensesRaw = await _db.Expenses
                .Where(e => e.UserId == userId && e.CreatedAt >= monthStart)
                .GroupBy(e => new { e.CreatedAt.Year, e.CreatedAt.Month })
                .Select(g => new { g.Key.Year, g.Key.Month, Total = g.Sum(e => e.Amount) })
                .ToListAsync();

            var monthlyPayrollRaw = allSalaryPayments
                .GroupBy(sp => new { sp.Year, sp.Month })
                .Select(g => new { g.Key.Year, g.Key.Month, Total = g.Sum(sp => sp.TotalEmployerCost) })
                .ToList();

            var monthlyTrend = Enumerable.Range(0, 12)
                .Select(i => DateTime.UtcNow.AddMonths(-11 + i))
                .Select(d => new MonthlyExpenseTrend(
                    Label: turkishMonths[d.Month - 1],
                    Expenses: monthlyExpensesRaw.FirstOrDefault(e => e.Year == d.Year && e.Month == d.Month)?.Total ?? 0m,
                    Payroll: monthlyPayrollRaw.FirstOrDefault(p => p.Year == d.Year && p.Month == d.Month)?.Total ?? 0m
                ))
                .ToList();

            return new ExpenseAnalyticsResult(
                TotalExpenses: totalExpenses,
                TotalPayroll: totalPayroll,
                TotalCombined: totalCombined,
                ExpenseCount: expenses.Count,
                CategoryBreakdown: categoryBreakdown,
                MonthlyTrend: monthlyTrend,
                TopExpenses: topExpenses
            );
        }

        public async Task<ProductionAnalyticsResult> GetProductionAnalyticsAsync(Guid userId, string range)
        {
            var startDate = GetStartDate(range);

            // Seçili dönem üretim logları
            var logs = await _db.ProductionLogs
                .Where(l => l.UserId == userId && l.ProducedAt >= startDate)
                .Include(l => l.Product)
                .ToListAsync();

            var totalQuantity = logs.Sum(l => l.Quantity);
            var totalCost = logs.Sum(l => l.TotalCost);
            var avgUnitCost = totalQuantity > 0 ? totalCost / totalQuantity : 0m;
            var uniqueProductCount = logs.Select(l => l.ProductId).Distinct().Count();
            var totalBatches = logs.Count;

            // Ürün bazlı özet
            var productSummary = logs
                .Where(l => l.Product != null)
                .GroupBy(l => new { l.ProductId, Name = l.Product!.Name })
                .Select(g =>
                {
                    var qty = g.Sum(l => l.Quantity);
                    var cost = g.Sum(l => l.TotalCost);
                    return new ProductionProductSummary(
                        ProductName: g.Key.Name,
                        TotalQuantity: qty,
                        TotalCost: cost,
                        AvgUnitCost: qty > 0 ? Math.Round(cost / qty, 4) : 0m,
                        BatchCount: g.Count(),
                        LastProducedAt: g.Max(l => l.ProducedAt)
                    );
                })
                .OrderByDescending(x => x.TotalCost)
                .ToList();

            // Günlük üretim (seçili dönem)
            var dailyProduction = logs
                .GroupBy(l => l.ProducedAt.Date)
                .OrderBy(g => g.Key)
                .Select(g => new DailyProductionPoint(
                    Date: g.Key.ToString("dd MMM", new System.Globalization.CultureInfo("tr-TR")),
                    Quantity: g.Sum(l => l.Quantity),
                    Cost: g.Sum(l => l.TotalCost),
                    BatchCount: g.Count()
                ))
                .ToList();

            // Son 12 ay aylık trend
            string[] turkishMonths = ["Oca", "Şub", "Mar", "Nis", "May", "Haz", "Tem", "Ağu", "Eyl", "Eki", "Kas", "Ara"];
            var twelveMonthsAgo = DateTime.UtcNow.AddMonths(-11);
            var monthStart = new DateTime(twelveMonthsAgo.Year, twelveMonthsAgo.Month, 1, 0, 0, 0, DateTimeKind.Utc);

            var allMonthlyLogs = await _db.ProductionLogs
                .Where(l => l.UserId == userId && l.ProducedAt >= monthStart)
                .ToListAsync();

            var monthlyTrend = Enumerable.Range(0, 12)
                .Select(i => DateTime.UtcNow.AddMonths(-11 + i))
                .Select(d =>
                {
                    var monthLogs = allMonthlyLogs
                        .Where(l => l.ProducedAt.Year == d.Year && l.ProducedAt.Month == d.Month)
                        .ToList();
                    return new MonthlyProductionTrend(
                        Label: turkishMonths[d.Month - 1],
                        Quantity: monthLogs.Sum(l => l.Quantity),
                        Cost: monthLogs.Sum(l => l.TotalCost)
                    );
                })
                .ToList();

            // Hammadde tüketimi — reçete × üretim miktarından hesapla
            var producedProductIds = logs.Select(l => l.ProductId).Distinct().ToList();
            var recipes = await _db.Recipes
                .Where(r => producedProductIds.Contains(r.ProductId))
                .Include(r => r.RecipeItems)
                    .ThenInclude(ri => ri.RawMaterial)
                .ToListAsync();

            var consumptionMap = new Dictionary<string, decimal>();
            foreach (var log in logs)
            {
                var recipe = recipes.FirstOrDefault(r => r.ProductId == log.ProductId);
                if (recipe == null) continue;
                foreach (var item in recipe.RecipeItems)
                {
                    var name = item.RawMaterial?.Name ?? "Bilinmeyen";
                    var consumed = item.RequiredQuantity * log.Quantity;
                    if (!consumptionMap.ContainsKey(name)) consumptionMap[name] = 0m;
                    consumptionMap[name] += consumed;
                }
            }

            var rawMaterialConsumption = consumptionMap
                .OrderByDescending(kv => kv.Value)
                .Select(kv => new RawMaterialConsumptionItem(kv.Key, Math.Round(kv.Value, 4)))
                .ToList();

            return new ProductionAnalyticsResult(
                TotalQuantity: totalQuantity,
                TotalCost: totalCost,
                AvgUnitCost: Math.Round(avgUnitCost, 4),
                UniqueProductCount: uniqueProductCount,
                TotalBatches: totalBatches,
                ProductSummary: productSummary,
                MonthlyTrend: monthlyTrend,
                DailyProduction: dailyProduction,
                RawMaterialConsumption: rawMaterialConsumption
            );
        }

        public async Task<HRAnalyticsResult> GetHRAnalyticsAsync(Guid userId, string range)
        {
            var startDate = GetStartDate(range);

            // Tüm çalışanlar (izin kayıtlarıyla birlikte)
            var employees = await _db.Employees
                .Where(e => e.UserId == userId)
                .Include(e => e.SalaryPayments)
                .Include(e => e.LeaveRecords)
                .ToListAsync();

            var activeEmployeeCount = employees.Count(e => e.IsActive);
            var totalEmployeeCount = employees.Count;

            // Range içindeki maaş ödemeleri
            var payments = employees
                .SelectMany(e => e.SalaryPayments.Where(p => new DateTime(p.Year, p.Month, 1) >= startDate))
                .ToList();

            var totalGrossSalary = payments.Sum(p => p.GrossSalary);
            var totalEmployerCost = payments.Sum(p => p.TotalEmployerCost);
            var avgNetSalary = payments.Count > 0 ? payments.Average(p => p.NetSalary) : 0m;

            // Maaş bileşen dağılımı (range içindeki ödemelerden toplam)
            var totalNet = payments.Sum(p => p.NetSalary);
            var totalEmployeeTax = payments.Sum(p => p.EmployeeSGK + p.EmployeeUnemployment + p.IncomeTax + p.StampTax);
            var totalEmployerExtra = payments.Sum(p => p.EmployerSGK + p.EmployerUnemployment);
            var salaryComponents = new SalaryComponentBreakdown(totalNet, totalEmployeeTax, totalEmployerExtra);

            // Range içindeki izin kayıtları
            var leaveRecordsInRange = employees
                .SelectMany(e => e.LeaveRecords
                    .Where(l => l.StartDate >= startDate)
                    .Select(l => new { Employee = e, Leave = l }))
                .ToList();

            var totalLeaveDays = leaveRecordsInRange.Sum(x => x.Leave.DayCount);

            // Personel bazlı özet
            var employeeSummaries = employees.Select(e =>
            {
                var emp_payments = e.SalaryPayments.Where(p => new DateTime(p.Year, p.Month, 1) >= startDate).ToList();
                var emp_leaves = e.LeaveRecords.Where(l => l.StartDate >= startDate).ToList();
                return new EmployeePayrollSummary(
                    FullName: e.FullName,
                    Position: e.Position,
                    IsActive: e.IsActive,
                    GrossSalary: e.GrossSalary,
                    AvgNetSalary: emp_payments.Count > 0 ? emp_payments.Average(p => p.NetSalary) : 0m,
                    AvgEmployerCost: emp_payments.Count > 0 ? emp_payments.Average(p => p.TotalEmployerCost) : 0m,
                    PaymentCount: emp_payments.Count,
                    TotalLeaveDays: emp_leaves.Sum(l => l.DayCount)
                );
            })
            .Where(e => e.PaymentCount > 0 || e.IsActive)
            .OrderByDescending(e => e.GrossSalary)
            .ToList();

            // İzin kayıtları detay (son 20, tarihe göre)
            var leaveRecords = leaveRecordsInRange
                .OrderByDescending(x => x.Leave.StartDate)
                .Take(20)
                .Select(x => new EmployeeLeaveRecord(
                    FullName: x.Employee.FullName,
                    Position: x.Employee.Position,
                    StartDate: x.Leave.StartDate,
                    EndDate: x.Leave.EndDate,
                    DayCount: x.Leave.DayCount,
                    Reason: x.Leave.Reason
                ))
                .ToList();

            // Son 12 ay aylık bordro trendi
            string[] turkishMonths = ["Oca", "Şub", "Mar", "Nis", "May", "Haz", "Tem", "Ağu", "Eyl", "Eki", "Kas", "Ara"];
            var twelveMonthsAgo = DateTime.UtcNow.AddMonths(-11);
            var monthStart = new DateTime(twelveMonthsAgo.Year, twelveMonthsAgo.Month, 1, 0, 0, 0, DateTimeKind.Utc);

            var allPayments = employees
                .SelectMany(e => e.SalaryPayments.Where(p => new DateTime(p.Year, p.Month, 1) >= monthStart))
                .ToList();

            var monthlyTrend = Enumerable.Range(0, 12)
                .Select(i => DateTime.UtcNow.AddMonths(-11 + i))
                .Select(d =>
                {
                    var month_payments = allPayments
                        .Where(p => p.Year == d.Year && p.Month == d.Month)
                        .ToList();
                    return new MonthlyPayrollTrend(
                        Label: turkishMonths[d.Month - 1],
                        Gross: month_payments.Sum(p => p.GrossSalary),
                        Net: month_payments.Sum(p => p.NetSalary),
                        EmployerCost: month_payments.Sum(p => p.TotalEmployerCost)
                    );
                })
                .ToList();

            return new HRAnalyticsResult(
                ActiveEmployeeCount: activeEmployeeCount,
                TotalEmployeeCount: totalEmployeeCount,
                TotalGrossSalary: totalGrossSalary,
                TotalEmployerCost: totalEmployerCost,
                AvgNetSalary: avgNetSalary,
                TotalLeaveDays: totalLeaveDays,
                SalaryComponents: salaryComponents,
                MonthlyTrend: monthlyTrend,
                EmployeeSummaries: employeeSummaries,
                LeaveRecords: leaveRecords
            );
        }

        // ── 1. Dönemsel Karşılaştırma ─────────────────────────────────────────
        public async Task<PeriodComparisonResult> GetPeriodComparisonAsync(Guid userId, string range)
        {
            var now = DateTime.UtcNow;
            DateTime startCurrent, startPrevious, endPrevious;

            switch (range)
            {
                case "7d":
                    startCurrent = now.AddDays(-7);  startPrevious = now.AddDays(-14); endPrevious = now.AddDays(-7);  break;
                case "90d":
                    startCurrent = now.AddDays(-90); startPrevious = now.AddDays(-180); endPrevious = now.AddDays(-90); break;
                case "year":
                    startCurrent = new DateTime(now.Year, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                    startPrevious = new DateTime(now.Year - 1, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                    endPrevious = startCurrent; break;
                default: // 30d
                    startCurrent = now.AddDays(-30); startPrevious = now.AddDays(-60); endPrevious = now.AddDays(-30); break;
            }

            var currentSales = await _db.Sales
                .Where(s => s.UserId == userId && s.SaleDate >= startCurrent).ToListAsync();
            var previousSales = await _db.Sales
                .Where(s => s.UserId == userId && s.SaleDate >= startPrevious && s.SaleDate < endPrevious).ToListAsync();

            var curExpenses = await _db.Expenses
                .Where(e => e.UserId == userId && e.CreatedAt >= startCurrent)
                .SumAsync(e => (decimal?)e.Amount) ?? 0m;
            var compEmpIds = await _db.Employees
                .Where(e => e.UserId == userId).Select(e => e.Id).ToListAsync();
            var allCompPayments = await _db.SalaryPayments
                .Where(sp => compEmpIds.Contains(sp.EmployeeId)).ToListAsync();
            var curPayroll = allCompPayments
                .Where(sp => new DateTime(sp.Year, sp.Month, 1) >= startCurrent)
                .Sum(sp => (decimal?)sp.TotalEmployerCost) ?? 0m;
            var prevExpenses = await _db.Expenses
                .Where(e => e.UserId == userId && e.CreatedAt >= startPrevious && e.CreatedAt < endPrevious)
                .SumAsync(e => (decimal?)e.Amount) ?? 0m;
            var prevPayroll = allCompPayments
                .Where(sp => new DateTime(sp.Year, sp.Month, 1) >= startPrevious && new DateTime(sp.Year, sp.Month, 1) < endPrevious)
                .Sum(sp => (decimal?)sp.TotalEmployerCost) ?? 0m;

            var curRev  = currentSales.Sum(s => s.TotalAmount);
            var curProf = currentSales.Sum(s => s.Quantity * (s.SalePrice - s.UnitCostAtSale));
            var curExp  = curExpenses + curPayroll;
            var curNet  = curProf - curExp;
            var curOrd  = (decimal)currentSales.Count;

            var prevRev  = previousSales.Sum(s => s.TotalAmount);
            var prevProf = previousSales.Sum(s => s.Quantity * (s.SalePrice - s.UnitCostAtSale));
            var prevExp  = prevExpenses + prevPayroll;
            var prevNet  = prevProf - prevExp;
            var prevOrd  = (decimal)previousSales.Count;

            PeriodMetric Calc(decimal cur, decimal prev)
            {
                var pct = prev != 0 ? Math.Round((cur - prev) / Math.Abs(prev) * 100, 1) : 0m;
                return new PeriodMetric(cur, prev, pct);
            }

            return new PeriodComparisonResult(
                Revenue: Calc(curRev, prevRev), Profit: Calc(curProf, prevProf),
                Expenses: Calc(curExp, prevExp), NetProfit: Calc(curNet, prevNet),
                OrderCount: Calc(curOrd, prevOrd)
            );
        }

        // ── 2. Başabaş Noktası ────────────────────────────────────────────────
        public async Task<BreakevenResult> GetBreakevenAsync(Guid userId)
        {
            // Sabit giderler: son 30 günde kaydedilen aylık/dönemsel giderler
            var expStart = DateTime.UtcNow.AddDays(-30);
            var monthlyExpenses = await _db.Expenses
                .Where(e => e.UserId == userId
                         && e.ExpenseType != ExpenseType.OneTime
                         && e.StartDate >= expStart)
                .SumAsync(e => (decimal?)e.Amount) ?? 0m;

            // Ortalama aylık personel maliyeti (son 3 ay)
            var threeMonthsAgo = DateTime.UtcNow.AddMonths(-3);
            var beEmpIds = await _db.Employees
                .Where(e => e.UserId == userId).Select(e => e.Id).ToListAsync();
            var payroll3m = (await _db.SalaryPayments
                .Where(sp => beEmpIds.Contains(sp.EmployeeId)).ToListAsync())
                .Where(sp => new DateTime(sp.Year, sp.Month, 1) >= threeMonthsAgo)
                .Sum(sp => (decimal?)sp.TotalEmployerCost) ?? 0m;
            var avgMonthlyPayroll = payroll3m / 3;

            var totalMonthlyFixed = monthlyExpenses + avgMonthlyPayroll;

            // Değişken maliyet oranı (son 30 günün satışlarından)
            var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
            var recentSales = await _db.Sales
                .Where(s => s.UserId == userId && s.SaleDate >= thirtyDaysAgo).ToListAsync();

            var rev30d = recentSales.Sum(s => s.TotalAmount);
            var varCost30d = recentSales.Sum(s => s.Quantity * s.UnitCostAtSale);
            var varCostRatio = rev30d > 0 ? varCost30d / rev30d : 0.6m;
            var contributionMarginRatio = 1 - varCostRatio;

            var breakevenRevenue = contributionMarginRatio > 0
                ? totalMonthlyFixed / contributionMarginRatio : 0m;

            var safetyMargin = rev30d - breakevenRevenue;
            var safetyMarginPct = rev30d > 0 ? safetyMargin / rev30d * 100 : -100m;

            return new BreakevenResult(
                MonthlyFixedCosts: Math.Round(totalMonthlyFixed, 0),
                VariableCostRatio: Math.Round(varCostRatio * 100, 1),
                BreakevenRevenue: Math.Round(breakevenRevenue, 0),
                CurrentMonthlyRevenue: Math.Round(rev30d, 0),
                SafetyMargin: Math.Round(safetyMargin, 0),
                SafetyMarginPercent: Math.Round(safetyMarginPct, 1),
                IsAboveBreakeven: safetyMargin >= 0
            );
        }

        // ── 3. Veresiye Yaşlandırma ───────────────────────────────────────────
        public async Task<ReceivablesAgingResult> GetReceivablesAgingAsync(Guid userId)
        {
            var now = DateTime.UtcNow;
            var debtCustomers = await _db.Customers
                .Where(c => c.UserId == userId && c.CurrentBalance > 0).ToListAsync();

            if (!debtCustomers.Any())
                return new ReceivablesAgingResult(0, new List<AgingBucket>(), new List<AgingCustomerDetail>());

            var customerIds = debtCustomers.Select(c => c.Id).ToList();
            var oldestSaleDates = await _db.Sales
                .Where(s => s.UserId == userId && s.CustomerId.HasValue
                       && customerIds.Contains(s.CustomerId.Value)
                       && s.PaymentType == PaymentType.Veresiye)
                .GroupBy(s => s.CustomerId)
                .Select(g => new { CustomerId = g.Key, OldestDate = g.Min(s => s.SaleDate) })
                .ToListAsync();

            var customerDetails = debtCustomers.Select(c =>
            {
                var oldest = oldestSaleDates.FirstOrDefault(d => d.CustomerId == c.Id)?.OldestDate ?? c.CreatedAt;
                var days = (int)(now - oldest).TotalDays;
                var bucket = days <= 30 ? "0–30 Gün" : days <= 60 ? "31–60 Gün" : days <= 90 ? "61–90 Gün" : "90+ Gün";
                return new AgingCustomerDetail(c.FirstName + " " + c.LastName, c.PhoneNumber, c.CurrentBalance, days, bucket);
            })
            .OrderByDescending(c => c.DaysOld).ToList();

            var bucketOrder = new[] { "0–30 Gün", "31–60 Gün", "61–90 Gün", "90+ Gün" };
            var buckets = bucketOrder.Select(label => new AgingBucket(
                Label: label,
                Amount: customerDetails.Where(c => c.Bucket == label).Sum(c => c.Balance),
                CustomerCount: customerDetails.Count(c => c.Bucket == label)
            )).ToList();

            return new ReceivablesAgingResult(debtCustomers.Sum(c => c.CurrentBalance), buckets, customerDetails);
        }

        // ── 4. RFM Müşteri Segmentasyonu ──────────────────────────────────────
        public async Task<RFMResult> GetRFMAsync(Guid userId)
        {
            var now = DateTime.UtcNow;
            var sales = await _db.Sales
                .Where(s => s.UserId == userId && s.CustomerId != null)
                .Include(s => s.Customer).ToListAsync();

            var groups = sales
                .Where(s => s.Customer != null)
                .GroupBy(s => new { s.CustomerId, Name = s.Customer!.FirstName + " " + s.Customer.LastName, s.Customer.PhoneNumber })
                .Select(g => new
                {
                    g.Key.CustomerId, g.Key.Name, g.Key.PhoneNumber,
                    RecencyDays = (int)(now - g.Max(s => s.SaleDate)).TotalDays,
                    Frequency = g.Count(),
                    Monetary = g.Sum(s => s.TotalAmount),
                    LastOrderDate = g.Max(s => s.SaleDate)
                })
                .ToList();

            if (!groups.Any())
                return new RFMResult(0, 0, 0, 0, 0, new List<RFMCustomerItem>());

            var sortedR = groups.OrderBy(c => c.RecencyDays).ToList();
            var sortedF = groups.OrderByDescending(c => c.Frequency).ToList();
            var sortedM = groups.OrderByDescending(c => c.Monetary).ToList();
            int total = groups.Count;
            int Score(int rank) => rank < total / 3 ? 3 : rank < 2 * total / 3 ? 2 : 1;

            var customers = groups.Select(c =>
            {
                var rS = Score(sortedR.FindIndex(x => x.CustomerId == c.CustomerId));
                var fS = Score(sortedF.FindIndex(x => x.CustomerId == c.CustomerId));
                var mS = Score(sortedM.FindIndex(x => x.CustomerId == c.CustomerId));
                var tot = rS + fS + mS;

                string seg, col;
                if (rS == 3 && fS == 3 && mS == 3)        { seg = "Şampiyon";           col = "emerald"; }
                else if (rS == 1 && fS == 1 && mS == 1)   { seg = "Kayıp";              col = "red";     }
                else if (rS == 1 && fS >= 2)               { seg = "Risk Altında";       col = "orange";  }
                else if (fS >= 2 && mS >= 2)               { seg = "Sadık";              col = "blue";    }
                else if (rS == 3 && fS <= 1)               { seg = "Yeni";               col = "violet";  }
                else                                        { seg = "Potansiyel Sadık";  col = "sky";     }

                return new RFMCustomerItem(c.Name, c.PhoneNumber, rS, fS, mS, tot, seg, col,
                    c.Monetary, c.Frequency, c.LastOrderDate);
            })
            .OrderByDescending(c => c.TotalScore).ToList();

            return new RFMResult(
                TotalCustomers: customers.Count,
                Champions: customers.Count(c => c.Segment == "Şampiyon"),
                Loyal: customers.Count(c => c.Segment is "Sadık" or "Potansiyel Sadık"),
                AtRisk: customers.Count(c => c.Segment == "Risk Altında"),
                Lost: customers.Count(c => c.Segment == "Kayıp"),
                Customers: customers
            );
        }

        private static DateTime GetPreviousStart(string range, DateTime currentStart) => range switch
        {
            "7d"   => currentStart.AddDays(-7),
            "90d"  => currentStart.AddDays(-90),
            "year" => new DateTime(currentStart.Year - 1, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            _      => currentStart.AddDays(-30),
        };

        // ── 5. Stok Devir Hızı ────────────────────────────────────────────────
        public async Task<StockTurnoverResult> GetStockTurnoverAsync(Guid userId)
        {
            var stocks = await _db.Stocks
                .Where(s => s.UserId == userId && s.RemainingQuantity > 0)
                .Include(s => s.Product).ThenInclude(p => p!.Category)
                .ToListAsync();

            var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
            var sold30d = await _db.Sales
                .Where(s => s.UserId == userId && s.SaleDate >= thirtyDaysAgo)
                .GroupBy(s => s.ProductId)
                .Select(g => new { ProductId = g.Key, Qty = g.Sum(s => s.Quantity) })
                .ToListAsync();

            var soldMap = sold30d.ToDictionary(x => x.ProductId, x => x.Qty);

            var items = stocks
                .Where(s => s.Product != null)
                .GroupBy(s => new { s.ProductId, Name = s.Product!.Name, Cat = s.Product.Category?.Name ?? "Kategorisiz" })
                .Select(g =>
                {
                    var remaining = g.Sum(s => s.RemainingQuantity);
                    var unitCost  = g.OrderByDescending(s => s.CreatedAt).First().UnitCost;
                    var capital   = remaining * unitCost;
                    var soldQty   = soldMap.GetValueOrDefault(g.Key.ProductId, 0m);
                    var rate      = remaining > 0 ? Math.Round(soldQty / remaining, 2) : 0m;
                    var days      = soldQty > 0 ? Math.Round(remaining / (soldQty / 30m), 0) : 0m;
                    return new StockTurnoverItem(
                        ProductName: g.Key.Name, CategoryName: g.Key.Cat,
                        TurnoverRate: rate, DaysToSell: days,
                        SoldQty30d: soldQty, RemainingQty: remaining,
                        BoundCapital: Math.Round(capital, 0), IsDeadStock: soldQty == 0
                    );
                })
                .OrderBy(x => x.TurnoverRate).ToList();

            var deadItems = items.Where(x => x.IsDeadStock).ToList();
            var activeItems = items.Where(x => !x.IsDeadStock).ToList();

            return new StockTurnoverResult(
                AvgTurnoverRate: activeItems.Any() ? Math.Round(activeItems.Average(x => x.TurnoverRate), 2) : 0m,
                DeadStockCount: deadItems.Count,
                DeadStockValue: deadItems.Sum(x => x.BoundCapital),
                TotalBoundCapital: items.Sum(x => x.BoundCapital),
                Items: items
            );
        }

        // ── 6. Ürün Portföy Matrisi (BCG) ─────────────────────────────────────
        public async Task<ProductPortfolioResult> GetProductPortfolioAsync(Guid userId, string range)
        {
            var startDate   = GetStartDate(range);
            var prevStart   = GetPreviousStart(range, startDate);

            var currentSales = await _db.Sales
                .Where(s => s.UserId == userId && s.SaleDate >= startDate)
                .Include(s => s.Product).ThenInclude(p => p!.Category)
                .ToListAsync();

            var previousRevMap = await _db.Sales
                .Where(s => s.UserId == userId && s.SaleDate >= prevStart && s.SaleDate < startDate)
                .GroupBy(s => s.ProductId)
                .Select(g => new { ProductId = g.Key, Revenue = g.Sum(s => s.TotalAmount) })
                .ToDictionaryAsync(x => x.ProductId, x => x.Revenue);

            var totalRevenue = currentSales.Sum(s => s.TotalAmount);
            if (totalRevenue == 0)
                return new ProductPortfolioResult(0, 0, 0, new List<PortfolioProductItem>());

            var productGroups = currentSales
                .Where(s => s.Product != null)
                .GroupBy(s => new { s.ProductId, s.Product!.Name, Cat = s.Product.Category?.Name ?? "Kategorisiz" })
                .Select(g =>
                {
                    var rev    = g.Sum(s => s.TotalAmount);
                    var share  = totalRevenue > 0 ? rev / totalRevenue * 100 : 0m;
                    var prev   = previousRevMap.GetValueOrDefault(g.Key.ProductId, 0m);
                    var growth = prev > 0 ? (rev - prev) / prev * 100 : (rev > 0 ? 100m : 0m);
                    return new { g.Key.Name, g.Key.Cat, Revenue = rev, Share = share, Growth = growth };
                })
                .OrderByDescending(x => x.Revenue)
                .Take(20)
                .ToList();

            if (!productGroups.Any())
                return new ProductPortfolioResult(Math.Round(totalRevenue, 0), 0, 0, new List<PortfolioProductItem>());

            var avgShare  = productGroups.Average(p => p.Share);
            var avgGrowth = productGroups.Average(p => p.Growth);

            var products = productGroups.Select(p =>
            {
                string quadrant, color;
                if      (p.Growth >= avgGrowth && p.Share >= avgShare) { quadrant = "Yıldız";       color = "emerald"; }
                else if (p.Growth <  avgGrowth && p.Share >= avgShare) { quadrant = "Nakit Kaynağı"; color = "blue";    }
                else if (p.Growth >= avgGrowth && p.Share <  avgShare) { quadrant = "Soru İşareti"; color = "orange";  }
                else                                                    { quadrant = "Köpek";        color = "red";     }

                return new PortfolioProductItem(
                    ProductName: p.Name, CategoryName: p.Cat,
                    Revenue: Math.Round(p.Revenue, 0),
                    RevenueShare: Math.Round(p.Share, 1),
                    GrowthRate: Math.Round(p.Growth, 1),
                    Quadrant: quadrant, QuadrantColor: color
                );
            }).ToList();

            return new ProductPortfolioResult(
                TotalRevenue:    Math.Round(totalRevenue, 0),
                AvgGrowthRate:   Math.Round(avgGrowth, 1),
                AvgRevenueShare: Math.Round(avgShare, 1),
                Products: products
            );
        }

        // ── 7. Gider Oranı Analizi ────────────────────────────────────────────
        public async Task<ExpenseRatioResult> GetExpenseRatioAsync(Guid userId)
        {
            string[] months = ["Oca", "Şub", "Mar", "Nis", "May", "Haz", "Tem", "Ağu", "Eyl", "Eki", "Kas", "Ara"];
            var monthStart = new DateTime(DateTime.UtcNow.AddMonths(-11).Year, DateTime.UtcNow.AddMonths(-11).Month, 1, 0, 0, 0, DateTimeKind.Utc);

            var allSales    = await _db.Sales.Where(s => s.UserId == userId && s.SaleDate >= monthStart).ToListAsync();
            var allExpenses = await _db.Expenses.Where(e => e.UserId == userId && e.CreatedAt >= monthStart).ToListAsync();
            var ratioEmpIds = await _db.Employees
                .Where(e => e.UserId == userId).Select(e => e.Id).ToListAsync();
            var allPayments = await _db.SalaryPayments
                .Where(sp => ratioEmpIds.Contains(sp.EmployeeId)).ToListAsync();

            var monthly = Enumerable.Range(0, 12).Select(i =>
            {
                var d   = DateTime.UtcNow.AddMonths(-11 + i);
                var rev = allSales.Where(s => s.SaleDate.Year == d.Year && s.SaleDate.Month == d.Month).Sum(s => s.TotalAmount);
                var exp = allExpenses.Where(e => e.CreatedAt.Year == d.Year && e.CreatedAt.Month == d.Month).Sum(e => e.Amount);
                var pay = allPayments.Where(p => p.Year == d.Year && p.Month == d.Month).Sum(p => p.TotalEmployerCost);
                var tot = exp + pay;
                return new MonthlyExpenseRatio(
                    months[d.Month - 1], Math.Round(rev, 0), Math.Round(exp, 0), Math.Round(pay, 0),
                    Math.Round(tot, 0),
                    rev > 0 ? Math.Round(tot / rev * 100, 1) : 0m,
                    Math.Round(rev - tot, 0)
                );
            }).ToList();

            var totalRev = monthly.Sum(m => m.Revenue);
            var totalExp = monthly.Sum(m => m.TotalCost);
            var activeMonths = monthly.Where(m => m.Revenue > 0).ToList();
            var avgRatio     = activeMonths.Any() ? Math.Round(activeMonths.Average(m => m.Ratio), 1) : 0m;
            var avgMargin    = totalRev > 0 ? Math.Round((totalRev - totalExp) / totalRev * 100, 1) : 0m;

            return new ExpenseRatioResult(
                AvgExpenseRatio: avgRatio, AvgOperatingMargin: avgMargin,
                TotalRevenue: Math.Round(totalRev, 0), TotalExpenses: Math.Round(totalExp, 0),
                Monthly: monthly
            );
        }

        // ── 8. Nakit Akış Projeksiyonu ────────────────────────────────────────
        public async Task<CashFlowProjectionResult> GetCashFlowProjectionAsync(Guid userId)
        {
            string[] months = ["Oca", "Şub", "Mar", "Nis", "May", "Haz", "Tem", "Ağu", "Eyl", "Eki", "Kas", "Ara"];
            var histStart = new DateTime(DateTime.UtcNow.AddMonths(-5).Year, DateTime.UtcNow.AddMonths(-5).Month, 1, 0, 0, 0, DateTimeKind.Utc);

            var allSales    = await _db.Sales.Where(s => s.UserId == userId && s.SaleDate >= histStart).ToListAsync();
            var allExpenses = await _db.Expenses.Where(e => e.UserId == userId && e.CreatedAt >= histStart).ToListAsync();
            var cashEmpIds = await _db.Employees
                .Where(e => e.UserId == userId).Select(e => e.Id).ToListAsync();
            var allPayments = await _db.SalaryPayments
                .Where(sp => cashEmpIds.Contains(sp.EmployeeId)).ToListAsync();

            var historical = Enumerable.Range(0, 6).Select(i =>
            {
                var d   = DateTime.UtcNow.AddMonths(-5 + i);
                var rev = allSales.Where(s => s.SaleDate.Year == d.Year && s.SaleDate.Month == d.Month).Sum(s => s.TotalAmount);
                var exp = allExpenses.Where(e => e.CreatedAt.Year == d.Year && e.CreatedAt.Month == d.Month).Sum(e => e.Amount)
                        + allPayments.Where(p => p.Year == d.Year && p.Month == d.Month).Sum(p => p.TotalEmployerCost);
                return new CashFlowPoint(months[d.Month - 1], Math.Round(rev, 0), Math.Round(exp, 0), Math.Round(rev - exp, 0), false);
            }).ToList();

            var last3   = historical.TakeLast(3).ToList();
            var avgRev  = last3.Any() ? last3.Average(m => m.Revenue)      : 0m;
            var avgExp  = last3.Any() ? last3.Average(m => m.Expenses)     : 0m;
            var avgNet  = last3.Any() ? last3.Average(m => m.NetCashFlow)  : 0m;

            var projected = Enumerable.Range(1, 3).Select(i =>
            {
                var d = DateTime.UtcNow.AddMonths(i);
                return new CashFlowPoint(months[d.Month - 1] + "*", Math.Round(avgRev, 0), Math.Round(avgExp, 0), Math.Round(avgNet, 0), true);
            }).ToList();

            return new CashFlowProjectionResult(
                AvgMonthlyRevenue:    Math.Round(avgRev, 0),
                AvgMonthlyExpenses:   Math.Round(avgExp, 0),
                AvgNetCashFlow:       Math.Round(avgNet, 0),
                ProjectedAnnualRevenue: Math.Round(avgRev * 12, 0),
                Points: historical.Concat(projected).ToList()
            );
        }

        // ── 9. Üretim Karlılık Marjı ──────────────────────────────────────────
        public async Task<ProductionProfitabilityResult> GetProductionProfitabilityAsync(Guid userId, string range)
        {
            var startDate = GetStartDate(range);

            var prodLogs = await _db.ProductionLogs
                .Where(l => l.UserId == userId && l.ProducedAt >= startDate)
                .Include(l => l.Product).ThenInclude(p => p!.Category)
                .ToListAsync();

            var sales = await _db.Sales
                .Where(s => s.UserId == userId && s.SaleDate >= startDate)
                .Include(s => s.Product).ThenInclude(p => p!.Category)
                .ToListAsync();

            var prodMap = prodLogs
                .Where(l => l.Product != null)
                .GroupBy(l => new { l.ProductId, l.Product!.Name, Cat = l.Product.Category?.Name ?? "Kategorisiz" })
                .ToDictionary(g => g.Key.ProductId, g => new
                {
                    g.Key.Name, g.Key.Cat,
                    TotalCost = g.Sum(l => l.TotalCost),
                    TotalQty  = g.Sum(l => l.Quantity)
                });

            var saleMap = sales
                .GroupBy(s => s.ProductId)
                .ToDictionary(g => g.Key, g => new
                {
                    Revenue = g.Sum(s => s.TotalAmount),
                    Profit  = g.Sum(s => s.Quantity * (s.SalePrice - s.UnitCostAtSale)),
                    Qty     = g.Sum(s => s.Quantity)
                });

            var allIds = prodMap.Keys.ToList();

            // Ürün adı için yedek kaynak
            var nameMap = prodLogs.Where(l => l.Product != null)
                .GroupBy(l => l.ProductId)
                .ToDictionary(g => g.Key, g => new { g.First().Product!.Name, Cat = g.First().Product!.Category?.Name ?? "Kategorisiz" });
            var saleNameMap = sales.Where(s => s.Product != null)
                .GroupBy(s => s.ProductId)
                .ToDictionary(g => g.Key, g => new { g.First().Product!.Name, Cat = g.First().Product!.Category?.Name ?? "Kategorisiz" });

            var products = allIds.Select(pid =>
            {
                var prod    = prodMap.GetValueOrDefault(pid);
                var sale    = saleMap.GetValueOrDefault(pid);
                var name    = prod?.Name ?? nameMap.GetValueOrDefault(pid)?.Name ?? saleNameMap.GetValueOrDefault(pid)?.Name ?? "Bilinmeyen";
                var cat     = prod?.Cat  ?? nameMap.GetValueOrDefault(pid)?.Cat  ?? saleNameMap.GetValueOrDefault(pid)?.Cat  ?? "Kategorisiz";
                var prodCost = prod?.TotalCost ?? 0m;
                var salesRev = sale?.Revenue   ?? 0m;
                var salesPrf = sale?.Profit    ?? 0m;
                var margin   = salesRev > 0 ? salesPrf / salesRev * 100 : 0m;

                return new ProductProfitabilityItem(
                    name, cat,
                    Math.Round(prodCost, 0), Math.Round(salesRev, 0), Math.Round(salesPrf, 0),
                    Math.Round(margin, 1),
                    Math.Round(prod?.TotalQty ?? 0m, 2), Math.Round(sale?.Qty ?? 0m, 2)
                );
            })
            .Where(p => p.ProductionCost > 0 || p.SalesRevenue > 0)
            .OrderByDescending(p => p.SalesRevenue)
            .ToList();

            var totalProd  = products.Sum(p => p.ProductionCost);
            var totalRev   = products.Sum(p => p.SalesRevenue);
            var totalProf  = products.Sum(p => p.SalesProfit);
            var margin     = totalRev > 0 ? Math.Round(totalProf / totalRev * 100, 1) : 0m;

            return new ProductionProfitabilityResult(totalProd, totalRev, totalProf, margin, products);
        }

        // ── 10. Hammadde Fiyat Trendi ─────────────────────────────────────────
        public async Task<MaterialPriceTrendResult> GetMaterialPriceTrendAsync(Guid userId)
        {
            string[] months = ["Oca", "Şub", "Mar", "Nis", "May", "Haz", "Tem", "Ağu", "Eyl", "Eki", "Kas", "Ara"];
            var histStart = new DateTime(DateTime.UtcNow.AddMonths(-5).Year, DateTime.UtcNow.AddMonths(-5).Month, 1, 0, 0, 0, DateTimeKind.Utc);

            var stocks = await _db.Stocks
                .Where(s => s.UserId == userId && s.InflowDate >= histStart)
                .Include(s => s.Product)
                    .ThenInclude(p => p!.Category)
                .ToListAsync();

            var labels = Enumerable.Range(0, 6).Select(i =>
            {
                var d = DateTime.UtcNow.AddMonths(-5 + i);
                return months[d.Month - 1];
            }).ToList();

            var topMaterials = stocks
                .Where(s => s.Product?.Category?.Type == CategoryType.Hammadde)
                .GroupBy(s => new { s.ProductId, s.Product!.Name })
                .OrderByDescending(g => g.Count())
                .Take(6)
                .Select(g => new { g.Key.ProductId, g.Key.Name })
                .ToList();

            if (!topMaterials.Any())
                return new MaterialPriceTrendResult(labels, new List<MaterialPriceSeries>());

            var series = topMaterials.Select(m =>
            {
                var points = Enumerable.Range(0, 6).Select(i =>
                {
                    var d     = DateTime.UtcNow.AddMonths(-5 + i);
                    var batch = stocks.Where(s => s.ProductId == m.ProductId
                                    && s.InflowDate.Year == d.Year && s.InflowDate.Month == d.Month).ToList();
                    if (!batch.Any())
                        return new MaterialPricePoint(months[d.Month - 1], 0, 0, 0, 0);

                    return new MaterialPricePoint(
                        months[d.Month - 1],
                        Math.Round(batch.Average(s => s.UnitCost), 2),
                        Math.Round(batch.Min(s => s.UnitCost), 2),
                        Math.Round(batch.Max(s => s.UnitCost), 2),
                        batch.Count
                    );
                }).ToList();

                return new MaterialPriceSeries(m.Name, points);
            }).ToList();

            return new MaterialPriceTrendResult(labels, series);
        }
    }
}
