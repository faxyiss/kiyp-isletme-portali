namespace StokAppApi.Entities
{
	public class BusinessProfile
	{
		public Guid Id { get; set; }
		public Guid UserId { get; set; } // Hangi kullanıcıya ait olduğu
		public string CompanyName { get; set; } = string.Empty;
		public string? Phone { get; set; }
		public string? ContactEmail { get; set; }
		public string? Address { get; set; }
		public string? TaxOffice { get; set; }
		public string? TaxNumber { get; set; }
		public DateTime CreatedAt { get; set; }
		public DateTime UpdatedAt { get; set; }

		// Navigation Property (Birebir ilişki için)
		public User User { get; set; } = null!;
	}
}
