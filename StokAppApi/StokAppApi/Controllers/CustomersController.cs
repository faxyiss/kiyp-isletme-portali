using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StokAppApi.Entities.Dto.Customers;
using StokAppApi.Services;

namespace StokAppApi.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	[Authorize] // Sadece giriş yapmış kullanıcılar erişebilir
	public class CustomersController : ControllerBase
	{
		private readonly ICustomerService _customerService;

		public CustomersController(ICustomerService customerService)
		{
			_customerService = customerService;
		}

		[HttpGet]
		public async Task<IActionResult> GetCustomers([FromQuery] CustomerQueryDto query)
		{
			// Artık sayfalamalı ve filtreli veri dönecek
			var result = await _customerService.GetCustomersAsync(query);
			return Ok(result);
		}

		[HttpGet("{id}")]
		public async Task<IActionResult> GetCustomer(Guid id)
		{
			var customer = await _customerService.GetCustomerByIdAsync(id);
			if (customer == null) return NotFound("Müşteri bulunamadı.");
			return Ok(customer);
		}

		[HttpPost]
		public async Task<IActionResult> CreateCustomer([FromBody] CustomerCreateDto dto)
		{
			if (!ModelState.IsValid) return BadRequest(ModelState);

			var createdCustomer = await _customerService.CreateCustomerAsync(dto);
			return CreatedAtAction(nameof(GetCustomer), new { id = createdCustomer.Id }, createdCustomer);
		}

		[HttpPut("{id}")]
		public async Task<IActionResult> UpdateCustomer(Guid id, [FromBody] CustomerUpdateDto dto)
		{
			if (!ModelState.IsValid) return BadRequest(ModelState);

			var updatedCustomer = await _customerService.UpdateCustomerAsync(id, dto);
			if (updatedCustomer == null) return NotFound("Müşteri bulunamadı veya yetkiniz yok.");

			return Ok(updatedCustomer);
		}

		[HttpDelete("{id}")]
		public async Task<IActionResult> DeleteCustomer(Guid id)
		{
			var result = await _customerService.DeleteCustomerAsync(id);
			if (!result) return NotFound("Müşteri bulunamadı veya silme yetkiniz yok.");

			return NoContent();
		}

		// 🎯 TAHSİLAT AL (BORÇ DÜŞ) ENDPOİNT'İ
		[HttpPost("{id}/payment")]
		public async Task<IActionResult> ReceivePayment(Guid id, [FromBody] CustomerPaymentDto dto)
		{
			if (!ModelState.IsValid) return BadRequest(ModelState);

			var updatedCustomer = await _customerService.ReceivePaymentAsync(id, dto);
			if (updatedCustomer == null) return NotFound("Müşteri bulunamadı.");

			return Ok(new { Message = "Tahsilat başarıyla eklendi, borç düşüldü.", Customer = updatedCustomer });
		}
	}
}