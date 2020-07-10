using System.Net;
using System.Collections.Generic;

namespace ETModel
{
	public class BenchmarkClientComponent : Entity
	{
        public enum RobotMode
        {
            Roaming,
            Party,
            Unknown,
        }

        public class ClientSetting
        {
            public NetOuterComponent networkComponent;

            public IPEndPoint ipEndPoint;

            public TestPlayerSetting testPlayerSetting;

            public RobotMode robotMode;

            public int roadSettingId { get; set; }
        }

        public NetOuterComponent networkComponent;

        public IPEndPoint ipEndPoint;

        public TestPlayerSetting testPlayerSetting;

        public RobotMode robotMode;

        public long roadSettingId;

        public Session session;

        public const long pingWaitMilisec = 5000L;

        public StateMachine<BenchmarkClientComponent> stateMachine;

        public StateMachine<BenchmarkClientComponent>.State login, roaming;

        public int index = 0;

        public List<BenchmarkComponent.IUpdate> components = new List<BenchmarkComponent.IUpdate>();

        public long currentPing;

        public string userName;
    }
}