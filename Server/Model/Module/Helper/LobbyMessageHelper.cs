using System.Collections.Generic;

namespace ETModel
{
    public static class LobbyMessageHelper
    {
        private static bool CheckMapUnitSenderVailed(Player player, out long gateSessionActorId)
        {
            gateSessionActorId = 0;

            if (player == null || !player.isOnline)
                return false;

            gateSessionActorId = player.gateSessionActorId;

            if (gateSessionActorId == 0)
                return false;

            return true;
        }

        public static void BroadcastTarget(IActorMessage message, params Player[] targets)
        {
            ActorMessageSenderComponent actorLocationSenderComponent = Game.Scene.GetComponent<ActorMessageSenderComponent>();
            for (int i = 0; i < targets?.Length; i++)
            {
                if (!CheckMapUnitSenderVailed(targets[i], out var gateSessionActorId))
                    continue;

                ActorMessageSender actorMessageSender = actorLocationSenderComponent.Get(gateSessionActorId);
                actorMessageSender.Send(message);
            }
        }

        public static void BroadcastTarget(IActorMessage message, List<Player> targets)
        {
            ActorMessageSenderComponent actorLocationSenderComponent = Game.Scene.GetComponent<ActorMessageSenderComponent>();
            for (int i = 0; i < targets?.Count; i++)
            {
                if (!CheckMapUnitSenderVailed(targets[i], out var gateSessionActorId))
                    continue;

                ActorMessageSender actorMessageSender = actorLocationSenderComponent.Get(gateSessionActorId);
                actorMessageSender.Send(message);
            }
        }
    }
}
