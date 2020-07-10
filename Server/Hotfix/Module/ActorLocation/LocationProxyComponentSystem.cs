using ETModel;
using System.Collections.Generic;

namespace ETHotfix
{
	[ObjectSystem]
	public class LocationProxyComponentSystem : AwakeSystem<LocationProxyComponent>
	{
		public override void Awake(LocationProxyComponent self)
		{
			self.Awake();
		}
	}

	[ObjectSystem]
	public class LocationProxyComponentDestroySystem : DestroySystem<LocationProxyComponent>
	{
		public override void Destroy(LocationProxyComponent self)
		{
			self.Destroy();
		}
	}

	public static class LocationProxyComponentEx
	{
		public static void Awake(this LocationProxyComponent self)
		{
			StartConfigComponent startConfigComponent = StartConfigComponent.Instance;

			StartConfig startConfig = startConfigComponent.LocationConfig;
			self.LocationAddress = startConfig.GetComponent<InnerConfig>().IPEndPoint;
		}

		public static void Destroy(this LocationProxyComponent self)
		{
			self.lockDict.Clear();
		}

		public static async ETTask Add(this LocationProxyComponent self, long key, long instanceId)
		{
			Session session = Game.Scene.GetComponent<NetInnerComponent>().Get(self.LocationAddress);
			await session.Call(new ObjectAddRequest() { Key = key, InstanceId = instanceId });
		}

		public static async ETTask Lock(this LocationProxyComponent self, long key, long instanceId, int time = 1000)
		{
			Session session = Game.Scene.GetComponent<NetInnerComponent>().Get(self.LocationAddress);
			await session.Call(new ObjectLockRequest() { Key = key, InstanceId = instanceId, Time = time });
		}

		public static async ETTask UnLock(this LocationProxyComponent self, long key, long oldInstanceId, long instanceId)
		{
			Session session = Game.Scene.GetComponent<NetInnerComponent>().Get(self.LocationAddress);
			await session.Call(new ObjectUnLockRequest() { Key = key, OldInstanceId = oldInstanceId, InstanceId = instanceId});
		}

		public static async ETTask Remove(this LocationProxyComponent self, long key)
		{
			Session session = Game.Scene.GetComponent<NetInnerComponent>().Get(self.LocationAddress);
			await session.Call(new ObjectRemoveRequest() { Key = key });
		}

		public static async ETTask<long> Get(this LocationProxyComponent self, long key)
		{
			Session session = Game.Scene.GetComponent<NetInnerComponent>().Get(self.LocationAddress);
			ObjectGetResponse response = (ObjectGetResponse)await session.Call(new ObjectGetRequest() { Key = key });
			return response.InstanceId;
		}

		public static async ETTask LockEvent(this LocationProxyComponent self, long id, long key, long timeout = 60000)
		{
			Session session = Game.Scene.GetComponent<NetInnerComponent>().Get(self.LocationAddress);
			L2S_LockEvent response = (L2S_LockEvent)await session.Call(new S2L_LockEvent() { Id = id, Key = key, Timeout = timeout });
			if (response.Error != ErrorCode.ERR_Success)
			{
				if (!self.lockDict.TryGetValue(key, out var dict))
				{
					dict = new Dictionary<long, ETTaskCompletionSource>();
					self.lockDict.Add(key, dict);
				}
				var tcs = new ETTaskCompletionSource();
				dict.Add(id, tcs);
				await tcs.Task;
			}
		}

		public static async ETTask UnlockEvent(this LocationProxyComponent self, long id, long key)
		{
			Session session = Game.Scene.GetComponent<NetInnerComponent>().Get(self.LocationAddress);
			L2S_UnlockEvent response = (L2S_UnlockEvent)await session.Call(new S2L_UnlockEvent() { Id = id, Key = key });
		}

		public static void ReceiveUnlockEvent(this LocationProxyComponent self, long id, long key)
		{
			if (self.lockDict.TryGetValue(key, out var dict))
			{
				if(dict.TryGetValue(id, out var tcs))
				{
					tcs.TrySetResult();
					dict.Remove(id);
					if (dict.Count == 0)
					{
						self.lockDict.Remove(key);
					}
				}
			}
		}
	}
}