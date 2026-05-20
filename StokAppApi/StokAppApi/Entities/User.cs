using System.ComponentModel.DataAnnotations;

namespace StokAppApi.Entities
{
	public class User
	{
		public Guid Id { get; set; } = Guid.NewGuid();
		[Required, MaxLength(100)]
		public string FullName { get; set; } = string.Empty;
		[Required, MaxLength(150)]
		public string Email { get; set; } = string.Empty;
		[Required, MaxLength(255)]
		public string PasswordHash { get; set; } = string.Empty;
		public BusinessType BusinessType { get; set; }
		[MaxLength(20)]
		public string Role { get; set; } = "Admin";
		public bool IsActive { get; set; } = true;
		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
		public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
	}
}
