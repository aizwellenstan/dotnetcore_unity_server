using System;
using System.Collections.Generic;
using System.Linq;

namespace ETModel
{
    [ObjectSystem]
    public class PingComponentAwakeSystem : AwakeSystem<PingComponent, long, long, Action<long>>
    {
        public override void Awake(PingComponent self, long waitTime, long overtime, Action<long> action)
        {
            self.Awake(waitTime, overtime, action);
        }
    }
    
    public class PingComponent : Component
    {
        private readonly Dictionary<long,long> _sessionTimes = new Dictionary<long, long>();

        private Action<long> onDisconnected = null;

        public async void Awake(long waitTime, long overtime, Action<long> action)
        {
            var timerComponent = Game.Scene.GetComponent<TimerComponent>();

            onDisconnected = action;

            while (true)
            {
                try
                {
                    Log.Info("在线人数 ：" + _sessionTimes.Count.ToString());

                    await timerComponent.WaitForMilliSecondAsync(waitTime);

                    // 检查所有Session，如果有时间超过指定的间隔就执行action

                    for (int i = 0; i < _sessionTimes.Count; i++)
                    {
                        if ((TimeHelper.ClientNowSeconds() - _sessionTimes.ElementAt(i).Value) > overtime)
                        {
                            RemoveSession(_sessionTimes.ElementAt(i).Key);
                        }
                    }
                }
                catch (Exception e)
                {
                    Log.Error(e.Message);
                }
            }
        }

        public void AddSession(long id)
        {
            _sessionTimes.Add(id, TimeHelper.ClientNowSeconds());
        }

        public bool RemoveSession(long id)
        {
            onDisconnected?.Invoke(id);
            return _sessionTimes.Remove(id);
        }

        public void UpdateSession(long id)
        {
            if (_sessionTimes.ContainsKey(id)) _sessionTimes[id] = TimeHelper.ClientNowSeconds();
        }

        public void UpsertSession(long id)
        {
            _sessionTimes[id] = TimeHelper.ClientNowSeconds();
        }
    }
}