namespace ETModel
{
	[Config((int)(AppType.ClientM))]
	public partial class LanguageSetting_FirstCategory : ACategory<LanguageSetting_First>
	{
	}

	public class LanguageSetting_First: IConfig
	{
		public long Id { get; set; }
		public string zh_tw;
		public string en;
	}
}
