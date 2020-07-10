namespace ETModel
{
	[Config((int)( AppType.Gate | AppType.Map | AppType.Lobby))]
	public partial class MessageTipSettingCategory : ACategory<MessageTipSetting>
	{
	}

	public class MessageTipSetting: IConfig
	{
		public long Id { get; set; }
		public string zh_tw_context;
		public string en_context;
	}
}
