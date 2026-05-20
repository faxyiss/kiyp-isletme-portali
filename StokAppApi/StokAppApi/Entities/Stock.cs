namespace StokAppApi.Entities
{
	public class Stock
	{
		public Guid Id { get; set; } = Guid.NewGuid();
		public Guid UserId { get; set; }
		public Guid ProductId { get; set; }
		public decimal InflowQuantity { get; set; }
		public decimal RemainingQuantity { get; set; }
		public decimal UnitCost { get; set; }
		public DateTime InflowDate { get; set; } = DateTime.UtcNow;
		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
		public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

		// Navigation Properties
		public User? User { get; set; }
		public Product? Product { get; set; }
	}
}
