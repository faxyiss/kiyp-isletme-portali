using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using StokAppApi;
using StokAppApi.Services;
using System.Text;

namespace StokAppApi
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var builder = WebApplication.CreateBuilder(args);

			// Add services to the container.
			builder.Services.AddControllers();
			builder.Services.AddOpenApi();

			var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

			builder.Services.AddDbContext<AppDbContext>(options =>
				options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

			// ===========================
			// SERVİS KATMANI BAĞLANTILARI 
			// ===========================
			builder.Services.AddHttpContextAccessor();

			builder.Services.AddScoped<IAuthService, AuthService>();
			builder.Services.AddScoped<IBusinessProfileService, BusinessProfileService>();
			builder.Services.AddScoped<IProductService, ProductService>();
			builder.Services.AddScoped<ICategoryService, CategoryService>();
			builder.Services.AddScoped<IStockService, StockService>();
			builder.Services.AddScoped<ICustomerService, CustomerService>();
			builder.Services.AddScoped<ISaleService, SaleService>();
			builder.Services.AddScoped<IExpenseService, ExpenseService>();
			builder.Services.AddScoped<IEmployeeService, EmployeeService>();
			builder.Services.AddScoped<ProductionService>();
			builder.Services.AddScoped<IAnalyticsService, AnalyticsService>();
			// ==========================================
			// JWT KİMLİK DOĞRULAMA SERVİSİ (YENİ EKLENDİ)
			// ==========================================
			var keySecret = builder.Configuration["Jwt:Key"] ?? "BuSistemAnaliziProjesiIcinEnAz32KarakterliGizliAnahtar!!";
			var key = Encoding.ASCII.GetBytes(keySecret);

			builder.Services.AddAuthentication(options =>
			{
				options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
				options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
			})
			.AddJwtBearer(options =>
			{
				options.RequireHttpsMetadata = false;
				options.SaveToken = true;
				options.TokenValidationParameters = new TokenValidationParameters
				{
					ValidateIssuerSigningKey = true,
					IssuerSigningKey = new SymmetricSecurityKey(key),
					ValidateIssuer = false,
					ValidateAudience = false
				};
			});

			builder.Services.AddCors(options =>
			{
				options.AddPolicy("AllowAll", builder =>
				{
					builder.AllowAnyOrigin()
						   .AllowAnyMethod()
						   .AllowAnyHeader();
				});
			});

			var app = builder.Build();

			// Configure the HTTP request pipeline.
			if (app.Environment.IsDevelopment())
			{
				app.MapOpenApi();
			}

			// CRITICAL CRITICAL: KRALİTE SIRALAMASI BURASIDIR
			app.UseCors("AllowAll");         // 1. Önce CORS kapıları açılacak (Tarayıcı engellemesin diye)
			app.UseAuthentication();       // 2. Sonra gelen Token doğrulanacak (Giriş yapıldı mı?)
			app.UseAuthorization();        // 3. Sonra yetki kontrol edilecek (Admin mi?)

			app.MapControllers();

			app.Run();
		}
	}
	
}
