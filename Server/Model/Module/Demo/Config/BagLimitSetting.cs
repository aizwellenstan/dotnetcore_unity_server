namespace ETModel
{
	[Config((int)(AppType.ClientH | AppType.Realm | AppType.Gate | AppType.Map | AppType.Lobby | AppType.DB))]
	public partial class BagLimitSettingCategory : ACategory<BagLimitSetting>
	{
	}

	public class BagLimitSetting: IConfig
	{
		public long Id { get; set; }
		public int DefaultSlotCount;
		public int MaxSlotCount;
	}
}
