using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StokAppApi.Services;
using System.Security.Claims;

namespace StokAppApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AnalyticsController : ControllerBase
    {
        private readonly IAnalyticsService _analyticsService;

        public AnalyticsController(IAnalyticsService analyticsService)
        {
            _analyticsService = analyticsService;
        }

        private Guid GetUserId()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (Guid.TryParse(userIdStr, out Guid userId)) return userId;
            throw new UnauthorizedAccessException("Geçersiz kullanıcı token'ı.");
        }

        // GET /api/analytics/overview?range=7d|30d|90d|year
        [HttpGet("overview")]
        public async Task<IActionResult> GetOverview([FromQuery] string range = "30d")
        {
            var result = await _analyticsService.GetOverviewAsync(GetUserId(), range);
            return Ok(result);
        }

        // GET /api/analytics/sales?range=7d|30d|90d|year
        [HttpGet("sales")]
        public async Task<IActionResult> GetSalesAnalytics([FromQuery] string range = "30d")
        {
            var result = await _analyticsService.GetSalesAnalyticsAsync(GetUserId(), range);
            return Ok(result);
        }

        // GET /api/analytics/stocks?range=7d|30d|90d|year
        [HttpGet("stocks")]
        public async Task<IActionResult> GetStockAnalytics([FromQuery] string range = "30d")
        {
            var result = await _analyticsService.GetStockAnalyticsAsync(GetUserId(), range);
            return Ok(result);
        }

        // GET /api/analytics/customers?range=7d|30d|90d|year
        [HttpGet("customers")]
        public async Task<IActionResult> GetCustomerAnalytics([FromQuery] string range = "30d")
        {
            var result = await _analyticsService.GetCustomerAnalyticsAsync(GetUserId(), range);
            return Ok(result);
        }

        // GET /api/analytics/expenses?range=7d|30d|90d|year
        [HttpGet("expenses")]
        public async Task<IActionResult> GetExpenseAnalytics([FromQuery] string range = "30d")
        {
            var result = await _analyticsService.GetExpenseAnalyticsAsync(GetUserId(), range);
            return Ok(result);
        }

        // GET /api/analytics/production?range=7d|30d|90d|year
        [HttpGet("production")]
        public async Task<IActionResult> GetProductionAnalytics([FromQuery] string range = "30d")
        {
            var result = await _analyticsService.GetProductionAnalyticsAsync(GetUserId(), range);
            return Ok(result);
        }

        // GET /api/analytics/hr?range=7d|30d|90d|year
        [HttpGet("hr")]
        public async Task<IActionResult> GetHRAnalytics([FromQuery] string range = "30d")
        {
            var result = await _analyticsService.GetHRAnalyticsAsync(GetUserId(), range);
            return Ok(result);
        }

        // GET /api/analytics/period-comparison?range=7d|30d|90d|year
        [HttpGet("period-comparison")]
        public async Task<IActionResult> GetPeriodComparison([FromQuery] string range = "30d")
        {
            var result = await _analyticsService.GetPeriodComparisonAsync(GetUserId(), range);
            return Ok(result);
        }

        // GET /api/analytics/breakeven
        [HttpGet("breakeven")]
        public async Task<IActionResult> GetBreakeven()
        {
            var result = await _analyticsService.GetBreakevenAsync(GetUserId());
            return Ok(result);
        }

        // GET /api/analytics/receivables-aging
        [HttpGet("receivables-aging")]
        public async Task<IActionResult> GetReceivablesAging()
        {
            var result = await _analyticsService.GetReceivablesAgingAsync(GetUserId());
            return Ok(result);
        }

        // GET /api/analytics/rfm
        [HttpGet("rfm")]
        public async Task<IActionResult> GetRFM()
        {
            var result = await _analyticsService.GetRFMAsync(GetUserId());
            return Ok(result);
        }

        // GET /api/analytics/stock-turnover
        [HttpGet("stock-turnover")]
        public async Task<IActionResult> GetStockTurnover()
        {
            var result = await _analyticsService.GetStockTurnoverAsync(GetUserId());
            return Ok(result);
        }

        // GET /api/analytics/product-portfolio?range=7d|30d|90d|year
        [HttpGet("product-portfolio")]
        public async Task<IActionResult> GetProductPortfolio([FromQuery] string range = "30d")
        {
            var result = await _analyticsService.GetProductPortfolioAsync(GetUserId(), range);
            return Ok(result);
        }

        // GET /api/analytics/expense-ratio
        [HttpGet("expense-ratio")]
        public async Task<IActionResult> GetExpenseRatio()
        {
            var result = await _analyticsService.GetExpenseRatioAsync(GetUserId());
            return Ok(result);
        }

        // GET /api/analytics/cashflow-projection
        [HttpGet("cashflow-projection")]
        public async Task<IActionResult> GetCashFlowProjection()
        {
            var result = await _analyticsService.GetCashFlowProjectionAsync(GetUserId());
            return Ok(result);
        }

        // GET /api/analytics/production-profitability?range=7d|30d|90d|year
        [HttpGet("production-profitability")]
        public async Task<IActionResult> GetProductionProfitability([FromQuery] string range = "30d")
        {
            var result = await _analyticsService.GetProductionProfitabilityAsync(GetUserId(), range);
            return Ok(result);
        }

        // GET /api/analytics/material-price-trend
        [HttpGet("material-price-trend")]
        public async Task<IActionResult> GetMaterialPriceTrend()
        {
            var result = await _analyticsService.GetMaterialPriceTrendAsync(GetUserId());
            return Ok(result);
        }
    }
}
