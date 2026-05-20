using Microsoft.EntityFrameworkCore;
using StokAppApi.Entities;
using StokAppApi.Entities.Dto.Customers;
using System.Security.Claims;

namespace StokAppApi.Services
{
	public interface ICustomerService
	{
		Task<CustomerPagedResponseDto> GetCustomersAsync(CustomerQueryDto query);
		Task<CustomerResponseDto?> GetCustomerByIdAsync(Guid id);
		Task<CustomerResponseDto> CreateCustomerAsync(CustomerCreateDto dto);
		Task<CustomerResponseDto?> UpdateCustomerAsync(Guid id, CustomerUpdateDto dto);
		Task<bool> DeleteCustomerAsync(Guid id);
		Task<CustomerResponseDto?> ReceivePaymentAsync(Guid id, CustomerPaymentDto dto);


	}

	public class CustomerService : ICustomerService
	{
		private readonly AppDbContext _context;
		private readonly IHttpContextAccessor _httpContextAccessor;

		public CustomerService(AppDbContext context, IHttpContextAccessor httpContextAccessor)
		{
			_context = context;
			_httpContextAccessor = httpContextAccessor;
		}

		private Guid GetCurrentUserId()
		{
			var userIdStr = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
			if (string.IsNullOrEmpty(userIdStr))
				throw new UnauthorizedAccessException("Kullanıcı kimliği bulunamadı.");
			return Guid.Parse(userIdStr);
		}

		public async Task<CustomerPagedResponseDto> GetCustomersAsync(CustomerQueryDto query)
		{
			var userId = GetCurrentUserId();

			// Veritabanı sorgusunu başlat
			var q = _context.Customers.Where(c => c.UserId == userId).AsQueryable();

			// 1. ARAMA FİLTRESİ
			if (!string.IsNullOrWhiteSpace(query.Search))
			{
				q = q.Where(c => c.FirstName.Contains(query.Search) ||
								 c.LastName.Contains(query.Search) ||
								 (c.PhoneNumber != null && c.PhoneNumber.Contains(query.Search)));
			}

			// 2. BORÇ DURUMU FİLTRESİ
			if (query.DebtFilter == "has_debt")
				q = q.Where(c => c.CurrentBalance > 0);
			else if (query.DebtFilter == "no_debt")
				q = q.Where(c => c.CurrentBalance <= 0);

			// 3. İSİM SIRALAMASI (A'dan Z'ye)
			q = q.OrderBy(c => c.FirstName).ThenBy(c => c.LastName);

			// 4. SAYFALAMA HESAPLAMALARI
			var totalItems = await q.CountAsync();
			var totalPages = (int)Math.Ceiling(totalItems / (double)query.PageSize);

			// 5. VERİYİ ÇEK (Skip ve Take ile sadece istenen sayfayı getir)
			var items = await q.Skip((query.Page - 1) * query.PageSize)
							   .Take(query.PageSize)
							   .Select(c => new CustomerResponseDto
							   {
								   Id = c.Id,
								   FirstName = c.FirstName,
								   LastName = c.LastName,
								   PhoneNumber = c.PhoneNumber,
								   CurrentBalance = c.CurrentBalance,
								   CreatedAt = c.CreatedAt
							   }).ToListAsync();

			return new CustomerPagedResponseDto
			{
				Items = items,
				TotalItems = totalItems,
				TotalPages = totalPages,
				CurrentPage = query.Page
			};
		}

		public async Task<CustomerResponseDto?> GetCustomerByIdAsync(Guid id)
		{
			var userId = GetCurrentUserId();
			var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

			if (customer == null) return null;

			return new CustomerResponseDto
			{
				Id = customer.Id,
				FirstName = customer.FirstName,
				LastName = customer.LastName,
				PhoneNumber = customer.PhoneNumber,
				CurrentBalance = customer.CurrentBalance,
				CreatedAt = customer.CreatedAt
			};
		}

		public async Task<CustomerResponseDto> CreateCustomerAsync(CustomerCreateDto dto)
		{
			var userId = GetCurrentUserId();

			var customer = new Customer
			{
				UserId = userId,
				FirstName = dto.FirstName,
				LastName = dto.LastName,
				PhoneNumber = dto.PhoneNumber,
				CurrentBalance = dto.InitialDebt // Başlangıç borcu
			};

			_context.Customers.Add(customer);
			await _context.SaveChangesAsync();

			return await GetCustomerByIdAsync(customer.Id) ?? throw new Exception("Müşteri oluşturulurken hata oluştu.");
		}

		public async Task<CustomerResponseDto?> UpdateCustomerAsync(Guid id, CustomerUpdateDto dto)
		{
			var userId = GetCurrentUserId();
			var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

			if (customer == null) return null;

			customer.FirstName = dto.FirstName;
			customer.LastName = dto.LastName;
			customer.PhoneNumber = dto.PhoneNumber;
			customer.UpdatedAt = DateTime.UtcNow;

			await _context.SaveChangesAsync();

			return await GetCustomerByIdAsync(customer.Id);
		}

		public async Task<bool> DeleteCustomerAsync(Guid id)
		{
			var userId = GetCurrentUserId();
			var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

			if (customer == null) return false;

			_context.Customers.Remove(customer);
			await _context.SaveChangesAsync();
			return true;
		}

		// TAHSİLAT AL (BORÇ DÜŞ) İŞLEMİ
		public async Task<CustomerResponseDto?> ReceivePaymentAsync(Guid id, CustomerPaymentDto dto)
		{
			var userId = GetCurrentUserId();
			var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

			if (customer == null) return null;

			// Bakiyeden (borçtan) düş
			customer.CurrentBalance -= dto.Amount;
			customer.UpdatedAt = DateTime.UtcNow;

			// İleride buraya Tahsilat (PaymentHistory) loglama tablosu da eklenebilir.

			await _context.SaveChangesAsync();

			return await GetCustomerByIdAsync(customer.Id);
		}
	}
}