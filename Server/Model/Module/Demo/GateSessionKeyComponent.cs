using System.Collections.Generic;

namespace ETModel
{
	public class GateSessionKeyComponent : Component
	{
		private readonly Dictionary<long, long> sessionKeyUidDict = new Dictionary<long, long>();
		
		public void Add(long key, long uid)
		{
			this.sessionKeyUidDict.Add(key, uid);
			this.TimeoutRemoveKey(key).Coroutine();
		}

		public long Get(long key)
		{
			this.sessionKeyUidDict.TryGetValue(key, out var uid);
			return uid;
		}

		public void Remove(long key)
		{
			this.sessionKeyUidDict.Remove(key);
		}

		private async ETVoid TimeoutRemoveKey(long key)
		{
			await Game.Scene.GetComponent<TimerComponent>().WaitForMilliSecondAsync(20000);
			this.sessionKeyUidDict.Remove(key);
		}
	}
}
