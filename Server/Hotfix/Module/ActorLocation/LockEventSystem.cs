using ETModel;
using System.Collections.Generic;

namespace ETHotfix
{
	[ObjectSystem]
	public class LockEventAwakeSystem : AwakeSystem<LockEvent, long>
	{
		public override void Awake(LockEvent self, long key)
		{
			self.Awake(key);
		}
	}

	[ObjectSystem]
	public class LockEventDestroySystem : DestroySystem<LockEvent>
	{
		public override void Destroy(LockEvent self)
		{
			self.Destroy();
		}
	}

	public static class LockEventSystem
	{
		public static void Awake(this LockEvent self, long key)
		{
			self.key = key;
		}

        public static async ETTask<LockEvent> Wait(this LockEvent self, long timeout = 60000)
        {
            var cacheProxyComponent = Game.Scene.GetComponent<CacheProxyComponent>();
            self.Id = IdGenerater.GenerateId();
            // 模擬自旋鎖
            while (!await cacheProxyComponent.LockEvent(self.key.ToString(), self.Id.ToString(), timeout))
            {
                await Game.Scene.GetComponent<TimerComponent>().WaitForSecondAsync(0.5f);
            }

            //await Game.Scene.GetComponent<LocationProxyComponent>().LockEvent(self.Id, self.key, timeout);
            return self;
        }

        public static void Destroy(this LockEvent self)
        {
            var cacheProxyComponent = Game.Scene.GetComponent<CacheProxyComponent>();
            cacheProxyComponent.UnlockEvent(self.key.ToString(), self.Id.ToString()).Coroutine();

            //Game.Scene.GetComponent<LocationProxyComponent>().UnlockEvent(self.Id, self.key).Coroutine();
        }
    }
}