using System;
using System.Collections.Generic;
using ETModel;

namespace ETHotfix
{
    [MessageHandler(AppType.Gate)]
    public class C2G_TeamGetListHandler : AMRpcHandler<C2G_TeamGetList, G2C_TeamGetList>
    {
        protected override void Run(Session session, C2G_TeamGetList message, Action<G2C_TeamGetList> reply)
        {
            G2C_TeamGetList response = new G2C_TeamGetList();
            try
            {
                Player player = session.GetComponent<SessionPlayerComponent>().Player;
                if (player == null)
                {
                    response.Error = ErrorCode.ERR_PlayerDoesntExist;
                    reply(response);
                    return;
                }

                var roomComponent = Game.Scene.GetComponent<RoomComponent>();
                List<Room> room = roomComponent.GetAllByType(RoomType.Team);
                for (int i = 0; i < room.Count; i++)
                {
                    var roomTeamComponent = room[i].GetComponent<RoomTeamComponent>();
                    if (roomTeamComponent == null)
                        continue;
                    if (roomTeamComponent.Data.IsReservation && !roomTeamComponent.IsReservationMember(player.uid))
                        continue;
                    if (room[i].State != RoomState.Start)
                        continue;
                    if (message.IsReservation != roomTeamComponent.Data.IsReservation)
                        continue;
                    response.Infos.Add(room[i].info);
                    response.Datas.Add(roomTeamComponent.Data);
                }

                //回傳資料
                reply(response);
            }
            catch (Exception e)
            {
                ReplyError(response, e, reply);
            }
        }
    }
}
