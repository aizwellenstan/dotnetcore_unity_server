using System;
using System.Collections.Generic;
using System.Net;
using ETModel;
using Google.Protobuf.Collections;

namespace ETHotfix
{
    [MessageHandler(AppType.Map)]
    public class L2M_SetReservationMemberHandler : AMRpcHandler<L2M_SetReservationMember, M2L_SetReservationMember>
    {
        protected override void Run(Session session, L2M_SetReservationMember message, Action<M2L_SetReservationMember> reply)
        {
            RunAsync(session, message, reply);
        }

        private void RunAsync(Session session, L2M_SetReservationMember message, Action<M2L_SetReservationMember> reply)
        {
            M2L_SetReservationMember response = new M2L_SetReservationMember();
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
                if(roomTeamComponent == null)
                {
                    response.Error = ErrorCode.ERR_RoomTeamComponentNull;
                    reply(response);
                    return;
                }
                roomTeamComponent.SetReservationMember(message.Data);
                reply(response);
            }
            catch (Exception e)
            {
                ReplyError(response, e, reply);
            }
        }
    }
}
