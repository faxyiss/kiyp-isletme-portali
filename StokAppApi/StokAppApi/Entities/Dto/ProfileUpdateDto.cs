using System.ComponentModel.DataAnnotations;

namespace StokAppApi.Entities.Dto
{
	public class ProfileUpdateDto
	{
		[Required(ErrorMessage = "Ad Soyad alanı zorunludur.")]
		[MaxLength(100)]
		public string FullName { get; set; } = string.Empty;

		[Required(ErrorMessage = "Şirket / İşletme adı zorunludur.")]
		[MaxLength(150)]
		public string CompanyName { get; set; } = string.Empty;
	}
}
