namespace StokAppApi.Entities
{
	public class ProductionLog
	{
		public Guid Id { get; set; } = Guid.NewGuid();
		public Guid UserId { get; set; }
		public Guid ProductId { get; set; }
		public decimal Quantity { get; set; }
		public decimal TotalCost { get; set; }
		public DateTime ProducedAt { get; set; } = DateTime.UtcNow;

		// Navigation properties
		public User? User { get; set; }
		public Product? Product { get; set; }
	}
}
