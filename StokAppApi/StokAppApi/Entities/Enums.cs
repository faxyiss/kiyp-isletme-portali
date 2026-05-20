namespace StokAppApi.Entities
{
	public enum BusinessType : byte
	{
		Satis = 0,
		Uretim = 1
	}

	public enum CategoryType : byte
	{
		Urun = 0,
		Hammadde = 1
	}

	public enum PaymentType : byte
	{
		Nakit = 0,
		KrediKarti = 1,
		Veresiye = 2
	}
}
