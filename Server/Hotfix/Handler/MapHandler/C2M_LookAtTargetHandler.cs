using System;
using System.Collections.Generic;
using ETModel;

namespace ETHotfix
{
    [ActorMessageHandler(AppType.Map)]
    public class C2M_LookAtTargetHandler : AMActorLocationRpcHandler<MapUnit, C2M_LookAtTarget, M2C_LookAtTarget>
    {
        protected override async ETTask Run(MapUnit mapUnit, C2M_LookAtTarget message, Action<M2C_LookAtTarget> reply)
        {
            await ETTask.CompletedTask;

            M2C_LookAtTarget response = new M2C_LookAtTarget();
            try
            {
                //判斷房間是否合法
                Room room = mapUnit.Room;
                if (room == null)
                {
                    response.Error = ErrorCode.ERR_RoomIdNotFound;
                    reply(response);
                    return;
                }

                MapUnit target = mapUnit.Room.GetMapUnitById(message.MapUnitId);
                if (target == null)
                {
                    response.Error = ErrorCode.ERR_InviteIdNotFind;
                    reply(response);
                    return;
                }

                response.Error = ErrorCode.ERR_Success;
                reply(response);

                mapUnit.LookMapUnitBlockChannel(target);
            }
            catch (Exception e)
            {
                ReplyError(response, e, reply);
            }
        }
    }
}
