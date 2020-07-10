using System;
using System.Collections.Generic;
using ETModel;

namespace ETHotfix
{
    [ActorMessageHandler(AppType.Map)]
    public class C2M_TeamReadyHandler : AMActorLocationRpcHandler<MapUnit, C2M_TeamReady, M2C_TeamReady>
    {
        protected override async ETTask Run(MapUnit mapUnit, C2M_TeamReady message, Action<M2C_TeamReady> reply)
        {
            await ETTask.CompletedTask;

            M2C_TeamReady response = new M2C_TeamReady();
            try
            {
                // 判斷房間是否合法
                Room room = mapUnit.Room;
                if (room == null)
                {
                    response.Error = ErrorCode.ERR_RoomIdNotFound;
                    reply(response);
                    return;
                }

                if (room.Type != RoomType.Team)
                {
                    response.Error = ErrorCode.ERR_RoonTypeError;
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

                // 設置IsReady
                roomTeamComponent.SetReady(mapUnit.Uid, message.IsReady);

                // 廣播給所有玩家 更新該玩家進度
                M2C_TeamReadyModify m2c_TeamReadyModify = new M2C_TeamReadyModify();
                m2c_TeamReadyModify.Uid = mapUnit.Uid;
                m2c_TeamReadyModify.IsReady = message.IsReady;
                MapMessageHelper.BroadcastRoom(mapUnit.Room.Id, m2c_TeamReadyModify);

                reply(response);
            }
            catch (Exception e)
            {
                ReplyError(response, e, reply);
            }
        }
    }
}
