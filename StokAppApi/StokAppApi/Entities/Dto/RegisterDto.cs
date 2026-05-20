namespace StokAppApi.Entities.Dto
{
	public class RegisterDto
	{
		public string FullName { get; set; } = string.Empty;
		public string Email { get; set; } = string.Empty;
		public string Password { get; set; } = string.Empty;
		public BusinessType BusinessType { get; set; } // 0: Satış, 1: Üretim
	}
}
