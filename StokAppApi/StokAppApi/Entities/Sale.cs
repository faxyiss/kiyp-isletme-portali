namespace StokAppApi.Entities
{
	public class Sale
	{
		public Guid Id { get; set; } = Guid.NewGuid();
		public Guid UserId { get; set; }
		public Guid ProductId { get; set; }
		public Guid StockId { get; set; }
		public Guid? CustomerId { get; set; }
		public decimal Quantity { get; set; }
		public decimal SalePrice { get; set; }
		public decimal UnitCostAtSale { get; set; }
		public decimal TotalAmount { get; set; }
		public PaymentType PaymentType { get; set; }
		public DateTime SaleDate { get; set; } = DateTime.UtcNow;

		// Navigation Properties
		public User? User { get; set; }
		public Product? Product { get; set; }
		public Stock? Stock { get; set; }
		public Customer? Customer { get; set; }
	}
}
