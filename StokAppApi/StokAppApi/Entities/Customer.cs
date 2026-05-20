using System.ComponentModel.DataAnnotations;

namespace StokAppApi.Entities
{
	public class Customer
	{
		public Guid Id { get; set; } = Guid.NewGuid();
		public Guid UserId { get; set; }
		[Required, MaxLength(50)]
		public string FirstName { get; set; } = string.Empty;
		[Required, MaxLength(50)]
		public string LastName { get; set; } = string.Empty;
		[MaxLength(20)]
		public string? PhoneNumber { get; set; }
		public decimal CurrentBalance { get; set; } = 0;
		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
		public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

		// Navigation Property
		public User? User { get; set; }
	}
}
