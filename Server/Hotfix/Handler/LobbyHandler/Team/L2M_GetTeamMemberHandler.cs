using System;
using System.Net;
using ETModel;

namespace ETHotfix
{
    [MessageHandler(AppType.Map)]
    public class L2M_GetTeamMemberHandler : AMRpcHandler<L2M_GetTeamMember, M2L_GetTeamMember>
    {
        protected override void Run(Session session, L2M_GetTeamMember message, Action<M2L_GetTeamMember> reply)
        {
            RunAsync(session, message, reply);
        }

        private void RunAsync(Session session, L2M_GetTeamMember message, Action<M2L_GetTeamMember> reply)
        {
            M2L_GetTeamMember response = new M2L_GetTeamMember();
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
                response.MemberData = roomTeamComponent.GetMember(message.Uid);
                reply(response);
            }
            catch (Exception e)
            {
                ReplyError(response, e, reply);
            }
        }
    }
}
