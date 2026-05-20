using Microsoft.EntityFrameworkCore;
using StokAppApi.Entities;

namespace StokAppApi
{
	public class AppDbContext : DbContext
	{
		public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

		public DbSet<User> Users { get; set; }
		public DbSet<Category> Categories { get; set; }
		public DbSet<Product> Products { get; set; }
		public DbSet<Stock> Stocks { get; set; }
		public DbSet<Customer> Customers { get; set; }
		public DbSet<Sale> Sales { get; set; }
		public DbSet<Recipe> Recipes { get; set; }
		public DbSet<RecipeItem> RecipeItems { get; set; }
		public DbSet<BusinessProfile> BusinessProfiles { get; set; }
		public DbSet<ProductionLog> ProductionLogs { get; set; }
		public DbSet<ExpenseCategory> ExpenseCategories { get; set; }
		public DbSet<Expense> Expenses { get; set; }
		public DbSet<Employee> Employees { get; set; }
		public DbSet<SalaryPayment> SalaryPayments { get; set; }
		public DbSet<LeaveRecord> LeaveRecords { get; set; }
		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			modelBuilder.Entity<BusinessProfile>()
				.HasOne(b => b.User)
				.WithOne()
				.HasForeignKey<BusinessProfile>(b => b.UserId)
				.OnDelete(DeleteBehavior.Cascade);

			// Her kullanıcının kendi içinde 1, 2, 3 diye gitmesini sağlayan kural
			modelBuilder.Entity<Product>()
				.HasIndex(p => new { p.UserId, p.ProductNo });
			// DİKKAT: .IsUnique(); KISMINI GEÇİCİ OLARAK YORUM SATIRINA ALDIK
			// .IsUnique(); 
		}
	}
}
