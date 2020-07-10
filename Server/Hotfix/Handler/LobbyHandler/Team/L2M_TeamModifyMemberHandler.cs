using System;
using System.Net;
using ETModel;

namespace ETHotfix
{
    [MessageHandler(AppType.Map)]
    public class L2M_TeamModifyMemberHandler : AMRpcHandler<L2M_TeamModifyMember, M2L_TeamModifyMember>
    {
        protected override void Run(Session session, L2M_TeamModifyMember message, Action<M2L_TeamModifyMember> reply)
        {
            RunAsync(session, message, reply);
        }

        private void RunAsync(Session session, L2M_TeamModifyMember message, Action<M2L_TeamModifyMember> reply)
        {
            M2L_TeamModifyMember response = new M2L_TeamModifyMember();
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
                TeamMemberData selfMemberData = roomTeamComponent.GetMember(message.Uid);
                M2C_TeamModifyMember m2c_TeamModifyMember = new M2C_TeamModifyMember();
                m2c_TeamModifyMember.Uid = message.Uid;
                m2c_TeamModifyMember.MemberData = selfMemberData;
                MapMessageHelper.BroadcastRoom(room.Id, m2c_TeamModifyMember, message.Uid);
                reply(response);
            }
            catch (Exception e)
            {
                ReplyError(response, e, reply);
            }
        }
    }
}
