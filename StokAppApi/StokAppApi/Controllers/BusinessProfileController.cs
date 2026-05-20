using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StokAppApi.Entities;
using StokAppApi.Entities.Dto;
using StokAppApi.Services;
using System.Security.Claims;

namespace StokAppApi.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	[Authorize] // Sadece giriş yapmış (Token'ı olan) kullanıcılar erişebilir
	public class BusinessProfileController : ControllerBase
	{
		private readonly IBusinessProfileService _service;

		public BusinessProfileController(IBusinessProfileService service)
		{
			_service = service;
		}

		[HttpGet("check")]
		public async Task<IActionResult> CheckProfile()
		{
			// Token'dan giriş yapan kullanıcının ID'sini çekiyoruz
			var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
			if (string.IsNullOrEmpty(userIdString)) return Unauthorized();

			var userId = Guid.Parse(userIdString);
			var exists = await _service.CheckIfExistsAsync(userId);

			if (!exists)
			{
				// Vue tarafında yakaladığımız 404 durumu
				return NotFound(new { message = "İşletme profili bulunamadı." });
			}

			return Ok(new { message = "Profil mevcut." });
		}

		[HttpPost("create")]
		public async Task<IActionResult> Create([FromBody] BusinessProfileCreateDto dto)
		{
			try
			{
				var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
				if (string.IsNullOrEmpty(userIdString)) return Unauthorized();

				var userId = Guid.Parse(userIdString);

				await _service.CreateProfileAsync(userId, dto);

				return Ok(new { message = "İşletme profili başarıyla oluşturuldu." });
			}
			catch (Exception ex)
			{
				return BadRequest(new { message = ex.Message });
			}
		}

		[HttpGet("my-profile")]
		public async Task<IActionResult> GetMyProfile()
		{
			var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
			if (string.IsNullOrEmpty(userIdString)) return Unauthorized();

			var userId = Guid.Parse(userIdString);
			var profile = await _service.GetProfileAsync(userId);

			if (profile == null)
			{
				return NotFound(new { message = "İşletme profili bulunamadı." });
			}

			// İhtiyacımız olan formatta veriyi dönüyoruz
			return Ok(new
			{
				companyName = profile.CompanyName,
				fullName = profile.User.FullName,
				role = profile.User.Role,
				initial = profile.User.FullName.Substring(0, 1).ToUpper() // İsmin ilk harfi (Avatar için)
			});
		}

		[HttpPut("update")]
		public async Task<IActionResult> UpdateProfile([FromBody] ProfileUpdateDto dto)
		{
			var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
			if (string.IsNullOrEmpty(userIdString)) return Unauthorized();

			var userId = Guid.Parse(userIdString);
			var result = await _service.UpdateProfileAsync(userId, dto);

			if (!result) return NotFound(new { message = "İşletme profili bulunamadı." });

			return Ok(new { message = "Profil bilgileriniz başarıyla güncellendi moruq!" });
		}
	}
}
