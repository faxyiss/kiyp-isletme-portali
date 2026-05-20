namespace StokAppApi.Entities.Dto
{
	public class BusinessProfileCreateDto
	{
		public string CompanyName { get; set; } = string.Empty;
		public string? Phone { get; set; }
		public string? ContactEmail { get; set; }
		public string? Address { get; set; }
		public string? TaxOffice { get; set; }
		public string? TaxNumber { get; set; }
	}
}
