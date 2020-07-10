using ETModel;
using System.Collections.Generic;

namespace ETHotfix
{
	public static class LocationComponentSystem
	{
		public static bool LockEvent(this LocationComponent self, long id, long key, long timeout = 60000)
		{
			if (!self.lockEventDict.ContainsKey(key))
			{
				self.lockEventDict.Add(key, new Queue<long>());
			}
			if (self.lockEventHashSet.Contains(key))
			{
				// 鎖住
				self.lockEventDict[key].Enqueue(id);
				return false;
			}
			self.lockEventHashSet.Add(key);
			self.lockEventDict[key].Enqueue(id);
			self.ExpireLockEvent(id, key, timeout).Coroutine();
			return true;
		}

		public static bool UnlockEvent(this LocationComponent self, long id, long key)
		{
			if (!self.lockEventHashSet.Contains(key))
			{
				return false;
			}
			if (self.lockEventDict.TryGetValue(key, out var queue))
			{
				if (queue.Count != 0)
				{
					if (id == queue.Peek())
					{
						queue.Dequeue();
						if(queue.Count == 0)
						{
							self.lockEventHashSet.Remove(key);
							self.lockEventDict.Remove(key);
						}
						else
						{
							// 通知下一個可以進入
							var next = queue.Peek();
							var session = SessionHelper.GetSession(IdGenerater.GetAppId(next));
							session.Send(new L2S_ReceiveUnlockEvent { Key = key, Id = next });
						}
						return true;
					}
					else
					{
						return false;
					}
				}
				else
				{
				 	return false;
				}
			}
			else
			{
				return false;
			}
		}

		private static async ETTask ExpireLockEvent(this LocationComponent self, long id, long key, long timeout)
		{
			 await Game.Scene.GetComponent<TimerComponent>().WaitForMilliSecondAsync(timeout);
			 // 超時強制解鎖
			 self.UnlockEvent(id, key);
		}
	}
}