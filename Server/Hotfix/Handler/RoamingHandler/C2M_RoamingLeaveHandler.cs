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
                //�P�_�ж��O�_�X�k
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

                //�R��MapUnit�T��
                M2C_MapUnitDestroy m2C_MapUnitDestroy = new M2C_MapUnitDestroy();
                m2C_MapUnitDestroy.MapUnitId = mapUnit.Id;

                //�s���R��MapUnit�T��(���]�t�ۤv)
                List<MapUnit> broadcastMapUnits = new List<MapUnit>();
                broadcastMapUnits.AddRange(room.GetAll());
                for (int i = 0; i < broadcastMapUnits.Count; i++)
                {
                    //�L�o�ۤv
                    if (broadcastMapUnits[i].Uid == mapUnit.Uid)
                    {
                        broadcastMapUnits.RemoveAt(i);
                        break;
                    }
                }
                MapMessageHelper.BroadcastTarget(m2C_MapUnitDestroy, broadcastMapUnits);

                //Player����mapUnitId
                var proxy = Game.Scene.GetComponent<CacheProxyComponent>();
                var playerSync = proxy.GetMemorySyncSolver<Player>();
                var player = playerSync.Get<Player>(mapUnit.Uid);
                player?.LeaveRoom();
                await playerSync.Update(player);

                //��Response�~����mapUnit
                reply(response);

                //���_���w���a�PMap���s��
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
