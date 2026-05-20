using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StokAppApi.Entities.Dto;
using StokAppApi.Services;
using System.Security.Claims;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
	private readonly IAuthService _authService;

	// Sadece servisimizi enjekte ediyoruz, DbContext veya Configuration ile işi bitti
	public AuthController(IAuthService authService)
	{
		_authService = authService;
	}

	[HttpPost("register")]
	public async Task<IActionResult> Register([FromBody] RegisterDto dto)
	{
		try
		{
			var result = await _authService.RegisterAsync(dto);
			return Ok(result);
		}
		catch (Exception ex)
		{
			return BadRequest(new { message = ex.Message });
		}
	}

	[HttpPost("login")]
	public async Task<IActionResult> Login([FromBody] LoginDto dto)
	{
		try
		{
			var result = await _authService.LoginAsync(dto);
			return Ok(result);
		}
		catch (Exception ex)
		{
			return BadRequest(new { message = ex.Message });
		}
	}
	[Authorize]
	[HttpDelete("delete-account")]
	public async Task<IActionResult> DeleteAccount()
	{
		try
		{
			var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
			if (string.IsNullOrEmpty(userIdString)) return Unauthorized();

			var userId = Guid.Parse(userIdString);
			var result = await _authService.DeleteAccountAsync(userId);

			if (!result) return NotFound(new { message = "Kullanıcı kaydı bulunamadı." });

			return Ok(new { message = "Hesabınız ve hesabınıza ait tüm veriler veritabanından kalıcı olarak temizlendi." });
		}
	 	catch(Exception ex)

		{
			return BadRequest(new { message = ex.Message });
		}
	}
}