namespace ETModel
{
	[Config((int)( AppType.ClientH))]
	public partial class LocationSettingCategory : ACategory<LocationSetting>
	{
	}

	public class LocationSetting: IConfig
	{
		public long Id { get; set; }
		public string Location;
		public int Continent;
	}
}
