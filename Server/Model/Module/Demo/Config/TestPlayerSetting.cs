namespace ETModel
{
	[Config((int)(AppType.Benchmark))]
	public partial class TestPlayerSettingCategory : ACategory<TestPlayerSetting>
	{
	}

	public class TestPlayerSetting: IConfig
	{
		public long Id { get; set; }
		public string DeviceUniqueIdentifier;
	}
}
