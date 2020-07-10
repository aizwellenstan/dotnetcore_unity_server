using System;
using System.Collections.Generic;
using ETModel;
using Google.Protobuf.Collections;
using MongoDB.Bson;

namespace ETHotfix
{
    [ActorMessageHandler(AppType.Lobby)]
    public class C2L_TeamGetListHandler : AMActorLocationRpcHandler<Player, C2L_TeamGetList, L2C_TeamGetList>
    {
        protected override async ETTask Run(Player player, C2L_TeamGetList message, Action<L2C_TeamGetList> reply)
        {
            L2C_TeamGetList response = new L2C_TeamGetList();
            try
            {
                var lobbyComponent = Game.Scene.GetComponent<LobbyComponent>();
                List<Room> room = lobbyComponent.GetAllByType(RoomType.Team);
                for (int i = 0; i < room.Count; i++)
                {
                    var result = await lobbyComponent.GetTeamInfo(room[i].Id);
                    if(result == null)
                    {
                        Log.Error($"room id:{room[i].Id} is not found!");
                        continue;
                    }
                    if (result.Item1.IsReservation && !IsReservationMember(result.Item3, player.uid))
                        continue;
                    if (room[i].State != RoomState.Start)
                        continue;
                    if (message.IsReservation != result.Item1.IsReservation)
                        continue;
                    response.Infos.Add(room[i].info);
                    response.Datas.Add(result.Item1);
                }

                //回傳資料
                reply(response);
            }
            catch (Exception e)
            {
                ReplyError(response, e, reply);
            }
        }

        public bool IsReservationMember(RepeatedField<ReservationMemberData> ReservationMembers, long uid)
        {
            for (int i = 0; i < ReservationMembers.Count; i++)
            {
                if (ReservationMembers[i].Uid == uid)
                    return true;
            }
            return false;
        }
    }
}
