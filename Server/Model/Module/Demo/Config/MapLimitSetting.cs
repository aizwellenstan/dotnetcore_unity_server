namespace ETModel
{
	[Config((int)(AppType.ClientH | AppType.Map | AppType.Lobby))]
	public partial class MapLimitSettingCategory : ACategory<MapLimitSetting>
	{
	}

	public class MapLimitSetting: IConfig
	{
		public long Id { get; set; }
		public int MaxUserCount;
		public int MaxPartyCount;
	}
}
