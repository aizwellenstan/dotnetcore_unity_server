using System;
using System.Collections.Generic;
using ETModel;

namespace ETHotfix
{
    [MessageHandler(AppType.Gate)]
    public class C2G_RoamingGetListHandler : AMRpcHandler<C2G_RoamingGetList, G2C_RoamingGetList>
    {
        protected override void Run(Session session, C2G_RoamingGetList message, Action<G2C_RoamingGetList> reply)
        {
            RoomComponent roomComponent = Game.Scene.GetComponent<RoomComponent>();
            List<Room> roamingRooms = roomComponent.GetAllByType(RoomType.Roaming);

            G2C_RoamingGetList response = new G2C_RoamingGetList();

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
