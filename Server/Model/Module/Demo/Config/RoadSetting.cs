namespace ETModel
{
	[Config((int)(AppType.ClientH | AppType.Gate | AppType.Map | AppType.Lobby))]
	public partial class RoadSettingCategory : ACategory<RoadSetting>
	{
	}

	public class RoadSetting: IConfig
	{
		public long Id { get; set; }
		public long Title;
		public double Distance;
		public string FilterName;
		public string SceneName;
		public string MinimapName;
		public string PreviewName;
		public int[] MusicIds;
		public int MapServerIndex;
		public int Difficulty;
	}
}
