namespace StokAppApi.Entities
{
	public class Recipe
	{
		public Guid Id { get; set; } = Guid.NewGuid();
		public Guid ProductId { get; set; }
		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
		public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

		// Navigation Properties
		public Product? Product { get; set; }
		public ICollection<RecipeItem> RecipeItems { get; set; } = new List<RecipeItem>();
	}
}
