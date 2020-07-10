using ETModel;
using System;
using System.Collections.Generic;

namespace ETHotfix
{
	[MessageHandler(AppType.AllServer)]
	public class M2A_RegisterServiceHandler : AMHandler<M2A_RegisterService>
	{
		protected override async void Run(Session session, M2A_RegisterService message)
		{
            try
            {
                StartConfig startConfig = (StartConfig)message.Component;
                NetInnerComponent netInnerComponent = Game.Scene.GetComponent<NetInnerComponent>();
                StartConfigComponent startConfigComponent = Game.Scene.GetComponent<StartConfigComponent>();
                startConfigComponent.AddConfig(startConfig);

                if (startConfig.AppId == (int)IdGenerater.AppId)
                    return;
                InnerConfig innerConfig = startConfig.GetComponent<InnerConfig>();
                Session serverSession = netInnerComponent.Get(innerConfig.IPEndPoint);
                A2S_ConnectService a2S_ConnectService = (A2S_ConnectService)await serverSession.Call(new S2A_ConnectService());
                if(a2S_ConnectService.Error != ErrorCode.ERR_Success)
                {
                    Log.Error($"to connect service[{startConfig.AppType}:{startConfig.AppId}] is failed.");
                }
                else
                {
                    Log.Info($"to connect service[{startConfig.AppType}:{startConfig.AppId}] is successful.");
                }
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }
	}
}