namespace ETModel
{
	[Config((int)(AppType.Gate | AppType.Map | AppType.Lobby))]
	public partial class NPCConfigCategory : ACategory<NPCConfig>
	{
	}

	public class NPCConfig: IConfig
	{
		public long Id { get; set; }
		public int Enable;
		public int RoadSettingId;
		public string Name;
		public int Location;
		public int CharacterId;
		public int BicycleId;
		public int BodyId;
		public int DecorationId;
		public double MinSpeed;
		public double MaxSpeed;
		public double RideTime;
		public double RestTime;
	}
}
