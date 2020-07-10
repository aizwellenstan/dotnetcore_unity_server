using System;
using System.Collections.Generic;
using ETModel;

namespace ETHotfix
{
    [ActorMessageHandler(AppType.Lobby)]
    public class C2L_RoamingGetListHandler : AMActorLocationRpcHandler<Player, C2L_RoamingGetList, L2C_RoamingGetList>
    {
        protected override async ETTask Run(Player player, C2L_RoamingGetList message, Action<L2C_RoamingGetList> reply)
        {
            await ETTask.CompletedTask;

            LobbyComponent lobbyComponent = Game.Scene.GetComponent<LobbyComponent>();
            List<Room> roamingRooms = lobbyComponent.GetAllByType(RoomType.Roaming);

            L2C_RoamingGetList response = new L2C_RoamingGetList();

            if (roamingRooms != null)
            {
                for (int i = 0; i < roamingRooms.Count; i++)
                {
                    Room room = roamingRooms[i];
                    if (room == null)
                        continue;

                    response.Infos.Add(room.info);
                }
            }

            reply(response);
        }
    }
}
