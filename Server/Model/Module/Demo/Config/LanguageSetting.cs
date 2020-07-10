namespace ETModel
{
	[Config((int)(AppType.ClientH))]
	public partial class LanguageSettingCategory : ACategory<LanguageSetting>
	{
	}

	public class LanguageSetting: IConfig
	{
		public long Id { get; set; }
		public string zh_tw;
		public string en;
	}
}
