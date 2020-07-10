using ETModel;
using System.Collections.Generic;

namespace ETHotfix
{
	public static class GateMessageHelper
	{
        public static void BroadcastTarget(IActorMessage message, List<long> targetUids)
        {
            ActorMessageSenderComponent actorLocationSenderComponent = Game.Scene.GetComponent<ActorMessageSenderComponent>();
            for (int i = 0; i < targetUids?.Count; i++)
            {
                if (targetUids[i] <= 0)
                {
                    Log.Error($"BroadcastTargetg失敗! targetUid<=0 {message.ToString()}");
                    continue;
                }

                Player player = CacheHelper.GetFromCache<Player>(targetUids[i]);
                if (player == null || !player.isOnline || player.gateSessionActorId == 0)
                {
                    Log.Trace($"Don't broadcast player who have disconnected!");
                    continue;
                }
                
                ActorMessageSender actorMessageSender = actorLocationSenderComponent.Get(player.gateSessionActorId);
                actorMessageSender.Send(message);
            }
        }

        public static void BroadcastTarget(IActorMessage message, params long[] targetUids)
        {
            ActorMessageSenderComponent actorLocationSenderComponent = Game.Scene.GetComponent<ActorMessageSenderComponent>();
            for (int i = 0; i < targetUids?.Length; i++)
            {
                if (targetUids[i] <= 0)
                {
                    Log.Error($"BroadcastTargetg失敗! targetUid<=0 {message.ToString()}");
                    continue;
                }

                Player player = CacheHelper.GetFromCache<Player>(targetUids[i]);
                if (player == null || !player.isOnline || player.gateSessionActorId == 0)
                {
                    Log.Trace($"Don't broadcast player who have disconnected!");
                    continue;
                }

                ActorMessageSender actorMessageSender = actorLocationSenderComponent.Get(player.gateSessionActorId);
                actorMessageSender.Send(message);
            }
        }
    }
}
