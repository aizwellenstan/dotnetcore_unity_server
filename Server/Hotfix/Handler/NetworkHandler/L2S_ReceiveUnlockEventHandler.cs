using ETModel;
using System;
using System.Collections.Generic;

namespace ETHotfix
{
	[MessageHandler(AppType.AllServer)]
	public class L2S_ReceiveUnlockEventHandler : AMHandler<L2S_ReceiveUnlockEvent>
	{
		protected override void Run(Session session, L2S_ReceiveUnlockEvent message)
		{
            try
            {
                var locationProxyComponent = Game.Scene.GetComponent<LocationProxyComponent>();
                locationProxyComponent.ReceiveUnlockEvent(message.Id, message.Key);
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }
	}
}