using System;
using System.Collections.Generic;
using ETModel;

namespace ETHotfix
{
    [MessageHandler(AppType.Map)]
    public class L2M_RoamingLeaveHandler : AMRpcHandler<L2M_RoamingLeave, M2L_RoamingLeave>
    {
        protected override async void Run(Session session, L2M_RoamingLeave message, Action<M2L_RoamingLeave> reply)
        {
            M2L_RoamingLeave response = new M2L_RoamingLeave();
            try
            {
                var mapUnitComponent = Game.Scene.GetComponent<MapUnitComponent>();
                MapUnit mapUnit = mapUnitComponent.GetByUid(message.Uid);

                // �P�_MapUnit�O�_�s�b�b��Map�W
                if(mapUnit == null)
                {
                    response.Error = ErrorCode.ERR_MapUnitMissing;
                    reply(response);
                    return;
                }

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
