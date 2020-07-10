using ETModel;
using System;
using System.Linq;
using System.Collections.Generic;

namespace ETHotfix
{
	[MessageHandler(AppType.Master)]
	public class S2M_RegisterServiceHandler : AMRpcHandler<S2M_RegisterService, M2S_RegisterService>
	{
		protected override void Run(Session session, S2M_RegisterService message, Action<M2S_RegisterService> reply)
		{
            M2S_RegisterService response = new M2S_RegisterService();
            try
            {
                NetInnerComponent netInnerComponent = Game.Scene.GetComponent<NetInnerComponent>();
                StartConfigComponent startConfigComponent = Game.Scene.GetComponent<StartConfigComponent>();
                MasterComponent masterComponent = Game.Scene.GetComponent<MasterComponent>();

                StartConfig startConfig = (StartConfig)message.Component;
                if (!masterComponent.AddConfig(startConfig))
                {
                    response.Error = ErrorCode.ERR_RegisterServerRepeatly;
                    reply(response);
                    return;
                }
                else
                {
                    Log.Info($"Server[{startConfig.AppType}:{startConfig.AppId}] is online.");
                }

                startConfigComponent.AddConfig(startConfig);

                var startConfigs = masterComponent.GetAll();
                response.Components = startConfigs.Select(e => (ComponentWithId)e).ToList();
                reply(response);

                foreach (StartConfig v in startConfigs)
                {
                    // 不傳給自己
                    if (v.AppId == startConfig.AppId)
                        continue;
                    InnerConfig innerConfig = v.GetComponent<InnerConfig>();
                    Session serverSession = netInnerComponent.Get(innerConfig.IPEndPoint);
                    serverSession.Send(new M2A_RegisterService
                    {
                        Component = startConfig
                    });
                }
            }
            catch (Exception e)
            {
                ReplyError(response, e, reply);
            }
        }
	}
}