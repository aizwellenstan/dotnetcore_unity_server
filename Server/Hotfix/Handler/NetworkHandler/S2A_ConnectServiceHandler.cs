using ETModel;
using System;
using System.Linq;
using System.Collections.Generic;

namespace ETHotfix
{
    /// <summary>
    /// 確認服務是否正常響應?
    /// </summary>
	[MessageHandler(AppType.AllServer)]
	public class S2A_ConnectServiceHandler : AMRpcHandler<S2A_ConnectService, A2S_ConnectService>
	{
		protected override void Run(Session session, S2A_ConnectService message, Action<A2S_ConnectService> reply)
		{
            A2S_ConnectService response = new A2S_ConnectService();
            try
            {
                reply(response);
            }
            catch (Exception e)
            {
                ReplyError(response, e, reply);
            }
        }
	}
}