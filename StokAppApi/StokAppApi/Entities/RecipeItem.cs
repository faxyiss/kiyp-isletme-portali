namespace StokAppApi.Entities
{
	public class RecipeItem
	{
		public Guid Id { get; set; } = Guid.NewGuid();
		public Guid RecipeId { get; set; }
		public Guid RawMaterialId { get; set; }
		public decimal RequiredQuantity { get; set; }

		// Navigation Properties
		public Recipe? Recipe { get; set; }
		public Product? RawMaterial { get; set; }
	}
}
