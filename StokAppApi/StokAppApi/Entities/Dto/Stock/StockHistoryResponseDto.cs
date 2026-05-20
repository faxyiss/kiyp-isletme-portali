namespace StokAppApi.Entities.Dto.Stock
{
	public class StockHistoryResponseDto
	{
		public Guid Id { get; set; }
		public decimal InflowQuantity { get; set; }
		public decimal RemainingQuantity { get; set; }
		public decimal UnitCost { get; set; }
		public DateTime InflowDate { get; set; }
	}
}
