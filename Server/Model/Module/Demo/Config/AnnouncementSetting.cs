namespace ETModel
{
	[Config((int)( AppType.Gate | AppType.Map | AppType.Lobby))]
	public partial class AnnouncementSettingCategory : ACategory<AnnouncementSetting>
	{
	}

	public class AnnouncementSetting: IConfig
	{
		public long Id { get; set; }
		public long timestamp;
		public string zh_tw_title;
		public string zh_tw_context;
		public string zh_cn_title;
		public string zh_cn_context;
		public string en_title;
		public string en_context;
	}
}
