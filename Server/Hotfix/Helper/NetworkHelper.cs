using ETModel;

namespace ETHotfix
{
	public static class NetworkHelper
    {
        public static class DisconnectInfo
        {
            public const int Network_Delay = 0;
            public const int Network_Break = 1;
            public const int Server_Maintain = 2;
            public const int Multiple_Login = 3;
            public const int Version_Invalid = 4;
        }

        public static void OnheartbeatFailed(long sessionId)
        {
            Log.Info($"SessionID:{sessionId} has died");
            Game.Scene.GetComponent<NetOuterComponent>().Remove(sessionId);
            Game.Scene.GetComponent<NetInnerComponent>().Remove(sessionId);
        }

        public static void DisconnectSession(long uid, int disconnectInfo)
        {
            Log.Info($"SendForceDisconnect Start, uid:{uid}, disconnectInfo:{disconnectInfo}");

            G2C_ForceDisconnect message = new G2C_ForceDisconnect()
            {
                DisconnectInfo = disconnectInfo
            };

            GateMessageHelper.BroadcastTarget(message, uid);

            Log.Info($"SendForceDisconnect End, uid:{uid}, disconnectInfo:{disconnectInfo}");
        }

        public static async void DisconnectPlayer(Player player)
        {
            if (player == null)
                return;

            Log.Info($"DisconnectPlayer Start, uid:{player.uid}");

            // 向MapServer發送斷線訊息
            if (player.InRoom())
            {
                ActorLocationSender actorLocationSender = Game.Scene.GetComponent<ActorLocationSenderComponent>().Get(player.mapUnitId);
                actorLocationSender.Send(new L2M_SessionDisconnect());

                // Player移除mapUnitId
                player.LeaveRoom();
            }

            player.isOnDisconnectingStage = true;
            // 更新Player
            await Game.Scene.GetComponent<PlayerComponent>().Update(player);

            Log.Info($"DisconnectPlayer End, uid:{player.uid}");
        }

        public static StartConfig GetRandomMap()
        {
            var mapConfigs = StartConfigComponent.Instance.MapConfigs;
            var val = RandomHelper.RandomNumber(0, mapConfigs.Count);
            return mapConfigs[val];
        }
    }
}
