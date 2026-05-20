using System.ComponentModel.DataAnnotations;

namespace StokAppApi.Entities
{
	public class Category
	{
		public Guid Id { get; set; } = Guid.NewGuid();
		public Guid UserId { get; set; }
		[Required, MaxLength(100)]
		public string Name { get; set; } = string.Empty;
		public CategoryType Type { get; set; } = CategoryType.Urun;
		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
		public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

		// Navigation Property
		public User? User { get; set; }
	}
}
