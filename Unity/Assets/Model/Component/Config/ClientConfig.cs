namespace ETModel
{
	public class ClientConfig: AConfigComponent
	{
		public string Address { get; set; }

        public int RobotMode { get; set; }

        public string UserCollectionPath { get; set; }

        public int Count { get; set; }

        public int RobotCount { get; set; }

        public int CurrentRobot { get; set; }

        public int RoadSettingId { get; set; }
    }
}