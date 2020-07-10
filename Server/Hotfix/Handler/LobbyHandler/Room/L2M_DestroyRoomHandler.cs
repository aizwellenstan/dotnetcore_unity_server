using System;
using System.Collections.Generic;
using System.Net;
using ETModel;
using Google.Protobuf.Collections;

namespace ETHotfix
{
    [MessageHandler(AppType.Map)]
    public class L2M_DestroyRoomHandler : AMRpcHandler<L2M_DestroyRoom, M2L_DestroyRoom>
    {
        protected override void Run(Session session, L2M_DestroyRoom message, Action<M2L_DestroyRoom> reply)
        {
            RunAsync(session, message, reply);
        }

        private async void RunAsync(Session session, L2M_DestroyRoom message, Action<M2L_DestroyRoom> reply)
        {
            M2L_DestroyRoom response = new M2L_DestroyRoom();
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
                await roomComponent.DestroyRoom(room.Id);
                reply(response);
            }
            catch (Exception e)
            {
                ReplyError(response, e, reply);
            }
        }
    }
}
