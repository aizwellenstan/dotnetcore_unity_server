namespace ETModel
{
	[Config((int)(AppType.ClientH))]
	public partial class MusicSettingCategory : ACategory<MusicSetting>
	{
	}

	public class MusicSetting: IConfig
	{
		public long Id { get; set; }
		public string Name;
		public double Volume;
	}
}
