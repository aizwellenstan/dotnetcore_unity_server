using System;
using UnityEngine;

namespace ETModel.Share
{
    [ObjectSystem]
    public partial class PingComponentAwakeSystem : AwakeSystem<PingComponent, long, Action<Exception>>
    {
        public override void Awake(PingComponent self, long waitTime, Action<Exception> action)
        {
            self.Awake(waitTime, action);
        }
    }
    
    /// <summary>
    /// 心跳组件+PING组件
    /// </summary>
    public class PingComponent: Component
    {
        #region 成员变量

        /// <summary>
        /// 发送时间
        /// </summary>
        private long _sendTimer;
        
        /// <summary>
        /// 接收时间
        /// </summary>
        private long _receiveTimer;

        /// <summary>
        /// 延时
        /// </summary>
        public long Ping = 0;
        
        /// <summary>
        /// 心跳协议包
        /// </summary>
        private readonly C2S_Ping _request = new C2S_Ping();

        public event Action<long> onPingChanged;

        #endregion

        #region Awake

        public void Awake(long waitTime, Action<Exception> action)
        {
            StartToPing(waitTime, action).Coroutine();
        }

        private async ETTask StartToPing(long waitTime, Action<Exception> action)
        {
            var timerComponent = Game.Scene.GetComponent<TimerComponent>();

            var session = this.GetParent<Session>();

            while (!this.IsDisposed)
            {
                try
                {
                    _sendTimer = TimeHelper.ClientNowMilliSeconds();
                    await session.Call(_request);
                    _receiveTimer = TimeHelper.ClientNowMilliSeconds();
                    // 計算延遲時間
                    Ping = ((_receiveTimer - _sendTimer) / 2) < 0 ? 0 : (_receiveTimer - _sendTimer) / 2;
                    onPingChanged?.Invoke(Ping);
                    //Log.Info($"_receiveTimer{_receiveTimer}, _sendTimer{_sendTimer}, Ping:{Ping}");
                }
                catch (Exception e)
                {
                    // 執行斷線後操作
                    action?.Invoke(e);
                    Log.Info("[Ping]網路斷線");
                }
                await timerComponent.WaitForMilliSecondAsync(waitTime);
            }
        }

        #endregion

        public override void Dispose()
        {
            if (this.Entity.Id == 0) return;
			
            base.Dispose();
            onPingChanged = null;
        }
    }
}