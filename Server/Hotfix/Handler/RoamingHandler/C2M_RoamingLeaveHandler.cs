using System;
using System.Collections.Generic;
using ETModel;

namespace ETHotfix
{
    [ActorMessageHandler(AppType.Map)]
    public class C2M_RoamingLeaveHandler : AMActorLocationRpcHandler<MapUnit, C2M_RoamingLeave, M2C_RoamingLeave>
    {
        protected override async ETTask Run(MapUnit mapUnit, C2M_RoamingLeave message, Action<M2C_RoamingLeave> reply)
        {
            await ETTask.CompletedTask;

            M2C_RoamingLeave response = new M2C_RoamingLeave();
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

                if (room.Type != RoomType.Roaming)
                {
                    response.Error = ErrorCode.ERR_RoonTypeError;
                    reply(response);
                    return;
                }

                //刪除MapUnit訊息
                M2C_MapUnitDestroy m2C_MapUnitDestroy = new M2C_MapUnitDestroy();
                m2C_MapUnitDestroy.MapUnitId = mapUnit.Id;

                //廣播刪除MapUnit訊息(不包含自己)
                List<MapUnit> broadcastMapUnits = new List<MapUnit>();
                broadcastMapUnits.AddRange(room.GetAll());
                for (int i = 0; i < broadcastMapUnits.Count; i++)
                {
                    //過濾自己
                    if (broadcastMapUnits[i].Uid == mapUnit.Uid)
                    {
                        broadcastMapUnits.RemoveAt(i);
                        break;
                    }
                }
                MapMessageHelper.BroadcastTarget(m2C_MapUnitDestroy, broadcastMapUnits);

                //Player移除mapUnitId
                var proxy = Game.Scene.GetComponent<CacheProxyComponent>();
                var playerSync = proxy.GetMemorySyncSolver<Player>();
                var player = playerSync.Get<Player>(mapUnit.Uid);
                player?.LeaveRoom();
                await playerSync.Update(player);

                //先Response才釋放mapUnit
                reply(response);

                //中斷指定玩家與Map的連接
                mapUnit.GetComponent<MapUnitGateComponent>().IsDisconnect = true;
                Game.Scene.GetComponent<MapUnitComponent>().Remove(mapUnit.Id);
            }
            catch (Exception e)
            {
                ReplyError(response, e, reply);
            }
        }
    }
}
