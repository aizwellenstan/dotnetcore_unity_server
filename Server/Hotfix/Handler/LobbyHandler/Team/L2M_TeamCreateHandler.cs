using System;
using System.Net;
using ETModel;
using MongoDB.Bson;

namespace ETHotfix
{
    [MessageHandler(AppType.Map)]
    public class L2M_TeamCreateHandler : AMRpcHandler<L2M_TeamCreate, M2L_TeamCreate>
    {
        protected override void Run(Session session, L2M_TeamCreate message, Action<M2L_TeamCreate> reply)
        {
            RunAsync(session, message, reply).Coroutine();
        }

        private async ETTask RunAsync(Session session, L2M_TeamCreate message, Action<M2L_TeamCreate> reply)
        {
            M2L_TeamCreate response = new M2L_TeamCreate();
            try
            {
                var roomComponent = Game.Scene.GetComponent<RoomComponent>();
                var room = await roomComponent.CreateTeamRoom(message.Info, message.Data);
                if(room == null)
                {
                    response.Error = ErrorCode.ERR_RoomIdNotFound;
                    reply(response);
                    return;
                }
                response.RoomId = room.Id;
                response.Json = room.ToJson();
                reply(response);
            }
            catch (Exception e)
            {
                ReplyError(response, e, reply);
            }
        }
    }
}
