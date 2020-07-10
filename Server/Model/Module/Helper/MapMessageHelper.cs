using System.Collections.Generic;

namespace ETModel
{
    public static class MapMessageHelper
    {
        private static bool CheckMapUnitSenderVailed(MapUnit mapUnit, out long gateSessionActorId)
        {
            gateSessionActorId = 0;
            if (mapUnit == null)
                return false;

            if (mapUnit.MapUnitType == MapUnitType.Npc)
                return false;

            var proxy = Game.Scene.GetComponent<CacheProxyComponent>();
            var playerSync = proxy.GetMemorySyncSolver<Player>();
            var player = playerSync.Get<Player>(mapUnit.Uid);
            if (player == null || !player.isOnline)
                return false;

            var mapUnitGateComponent = mapUnit.GetComponent<MapUnitGateComponent>();
            if (mapUnitGateComponent == null)
                return false;
            if (mapUnitGateComponent.IsDisconnect)
                return false;

            gateSessionActorId = mapUnitGateComponent.GateSessionActorId;

            if (gateSessionActorId == 0)
                return false;

            return true;
        }

        public static void BroadcastTarget(IActorMessage message, params MapUnit[] targets)
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

        public static void BroadcastTarget(IActorMessage message, List<MapUnit> targets)
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

        public static void BroadcastRoom(long roonId, IActorMessage message, params long[] filterUids)
        {
            Room room = Game.Scene.GetComponent<RoomComponent>().Get(roonId);
            if (room == null)
            {
                Log.Error($"{message?.GetType()?.ToString()} Broadcast 失敗! 找不到Room : {roonId}");
                return;
            }
            List<MapUnit> targets = room.GetAll();
            ActorMessageSenderComponent actorLocationSenderComponent = Game.Scene.GetComponent<ActorMessageSenderComponent>();
            for (int i = 0; i < targets.Count; i++)
            {
                if (filterUids != null)
                {
                    bool isFilter = false;
                    for (int k = 0; k < filterUids.Length; k++)
                    {
                        if (targets[i].Uid == filterUids[k])
                        {
                            isFilter = true;
                            break;
                        }
                    }
                    if (isFilter)
                        continue;
                }

                if (!CheckMapUnitSenderVailed(targets[i], out var gateSessionActorId))
                    continue;

                ActorMessageSender actorMessageSender = actorLocationSenderComponent.Get(gateSessionActorId);
                actorMessageSender.Send(message);
            }
        }
    }
}
