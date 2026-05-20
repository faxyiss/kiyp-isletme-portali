using Microsoft.EntityFrameworkCore;
using StokAppApi.Entities;
using StokAppApi.Entities.Dto;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
namespace StokAppApi.Services

{
	public interface IAuthService
	{
		Task<AuthResponseDto?> RegisterAsync(RegisterDto dto);
		Task<AuthResponseDto?> LoginAsync(LoginDto dto);

		Task<bool> DeleteAccountAsync(Guid userId);
	}
	public class AuthService : IAuthService
	{
		private readonly AppDbContext _context;
		private readonly IConfiguration _configuration;

		public AuthService(AppDbContext context, IConfiguration configuration)
		{
			_context = context;
			_configuration = configuration;
		}

		public async Task<AuthResponseDto?> RegisterAsync(RegisterDto dto)
		{
			if (await _context.Users.AnyAsync(u => u.Email.ToLower() == dto.Email.ToLower()))
				throw new Exception("Bu e-posta adresi zaten kullanımda.");

			var user = new User
			{
				Id = Guid.NewGuid(),
				FullName = dto.FullName,
				Email = dto.Email,
				PasswordHash = HashPassword(dto.Password),
				BusinessType = dto.BusinessType,
				Role = "Admin",
				IsActive = true,
				CreatedAt = DateTime.UtcNow,
				UpdatedAt = DateTime.UtcNow
			};

			_context.Users.Add(user);
			await _context.SaveChangesAsync();

			return new AuthResponseDto
			{
				Token = GenerateJwtToken(user),
				UserId = user.Id,
				FullName = user.FullName,
				BusinessType = user.BusinessType
			};
		}

		public async Task<AuthResponseDto?> LoginAsync(LoginDto dto)
		{
			var user = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == dto.Email.ToLower());

			if (user == null || !VerifyPassword(dto.Password, user.PasswordHash))
				throw new Exception("E-posta veya şifre hatalı.");

			if (!user.IsActive)
				throw new Exception("Hesabınız askıya alınmış.");

			return new AuthResponseDto
			{
				Token = GenerateJwtToken(user),
				UserId = user.Id,
				FullName = user.FullName,
				BusinessType = user.BusinessType
			};
		}

		// ==========================================
		// GÜVENLİK YARDIMCI METODLARI (Servis İçinde Gizli)
		// ==========================================
		private string HashPassword(string password)
		{
			using var sha256 = SHA256.Create();
			var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
			return Convert.ToBase64String(hashedBytes);
		}

		private bool VerifyPassword(string password, string passwordHash)
		{
			return HashPassword(password) == passwordHash;
		}

		private string GenerateJwtToken(User user)
		{
			var tokenHandler = new JwtSecurityTokenHandler();
			var keySecret = _configuration["Jwt:Key"] ?? "BuSistemAnaliziProjesiIcinEnAz32KarakterliGizliAnahtar!!";
			var key = Encoding.ASCII.GetBytes(keySecret);

			var tokenDescriptor = new SecurityTokenDescriptor
			{
				Subject = new ClaimsIdentity(new[]
				{
				new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
				new Claim(ClaimTypes.Name, user.FullName),
				new Claim(ClaimTypes.Email, user.Email),
				new Claim("BusinessType", ((byte)user.BusinessType).ToString()),
				new Claim(ClaimTypes.Role, user.Role)
			}),
				Expires = DateTime.UtcNow.AddDays(7),
				SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
			};

			var token = tokenHandler.CreateToken(tokenDescriptor);
			return tokenHandler.WriteToken(token);
		}

		public async Task<bool> DeleteAccountAsync(Guid userId)
		{
			var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
			if (user == null) return false;

			// 🎯 BURASI AMBARDAN MAL KAÇIRMA ALANI, SIRA ÇOK KRİTİK MORUQ!
			// Önce en alt bağımlı tablolardan başlayıp yukarı doğru her şeyi kazıyoruz:

			// 1. Satış Faturası ve Satış Kayıtları uçsun
			var sales = _context.Sales.Where(s => s.UserId == userId);
			_context.Sales.RemoveRange(sales);

			// 2. Stok Hareketleri ve Ambar Girişleri uçsun
			var stocks = _context.Stocks.Where(s => s.UserId == userId);
			_context.Stocks.RemoveRange(stocks);

			// 3. Ürün Kartları uçsun
			var products = _context.Products.Where(p => p.UserId == userId);
			_context.Products.RemoveRange(products);

			// 4. Kategoriler uçsun
			var categories = _context.Categories.Where(c => c.UserId == userId);
			_context.Categories.RemoveRange(categories);

			// 5. Müşteri ve Cari Kartları uçsun
			var customers = _context.Customers.Where(c => c.UserId == userId);
			_context.Customers.RemoveRange(customers);

			// 6. İşletme Profili uçsun
			var profile = _context.BusinessProfiles.Where(b => b.UserId == userId);
			_context.BusinessProfiles.RemoveRange(profile);

			// 7. En son sahibini (Kullanıcı kaydını) yok ediyoruz
			_context.Users.Remove(user);

			// Tüm silme emirlerini tek bir transaction halinde veritabanına tek tıkla şutluyoruz!
			await _context.SaveChangesAsync();
			return true;
		}
	}
}
