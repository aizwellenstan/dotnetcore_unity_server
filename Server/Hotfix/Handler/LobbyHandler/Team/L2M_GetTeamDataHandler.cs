using System;
using System.Net;
using ETModel;
using MongoDB.Bson;

namespace ETHotfix
{
    [MessageHandler(AppType.Map)]
    public class L2M_GetTeamDataHandler : AMRpcHandler<L2M_GetTeamData, M2L_GetTeamData>
    {
        protected override void Run(Session session, L2M_GetTeamData message, Action<M2L_GetTeamData> reply)
        {
            RunAsync(session, message, reply);
        }

        private void RunAsync(Session session, L2M_GetTeamData message, Action<M2L_GetTeamData> reply)
        {
            M2L_GetTeamData response = new M2L_GetTeamData();
            try
            {
                var roomComponent = Game.Scene.GetComponent<RoomComponent>();
                var room = roomComponent.Get(message.RoomId);
                if(room == null)
                {
                    response.Error = ErrorCode.ERR_RoomIdNotFound;
                    reply(response);
                    return;
                }
                var roomTeamComponent = room.GetComponent<RoomTeamComponent>();
                if (roomTeamComponent == null)
                {
                    response.Error = ErrorCode.ERR_RoomTeamComponentNull;
                    reply(response);
                    return;
                }
                response.RoomJson = room.ToJson();
                response.TeamData = roomTeamComponent.Data;
                for (int i = 0; i < roomTeamComponent.MemberDatas.Length; i++)
                {
                    if (roomTeamComponent.MemberDatas[i] != null)
                        response.TeamMember.Add(roomTeamComponent.MemberDatas[i]);
                }
                for (int i = 0; i < roomTeamComponent.ReservationMembers?.Count; i++)
                {
                    if (roomTeamComponent.ReservationMembers[i] != null)
                        response.ReservationTeamData.Add(roomTeamComponent.ReservationMembers[i]);
                }
                reply(response);
            }
            catch (Exception e)
            {
                ReplyError(response, e, reply);
            }
        }
    }
}
