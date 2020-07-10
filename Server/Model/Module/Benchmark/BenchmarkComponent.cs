using System.Collections.Generic;

namespace ETModel
{
	public class BenchmarkComponent: Component
	{
        public interface IUpdate
        {
            void Update();
            string GetMessage();
        }

        public int k;

		public long time1 = TimeHelper.ClientNowMilliSeconds();

        public readonly Dictionary<string, BenchmarkClientComponent> clients = new Dictionary<string, BenchmarkClientComponent>();

        public readonly List<BenchmarkClientComponent> clientList = new List<BenchmarkClientComponent>();

        public float frameCount = 0;

        public double updateTimer = 0;

        public const double logFreqAtSec = 1.0f;

        public double logTimer = logFreqAtSec;

        public TimerComponent timerComponent;

        public ProfilerUtility.NetworkProfiler networkProfiler;

        public Dictionary<int, List<ETTaskCompletionSource>> groupToTaskControllerMap = new Dictionary<int, List<ETTaskCompletionSource>>();

        public Dictionary<int, long> groupToRoomMap = new Dictionary<int, long>();

        public List<int> watchTargetList = new List<int>();

        public bool isOnProfiler = false;

        public float updatePeriod = 0.033333f;

        public bool isNeedToUpdate = true;
    }
}