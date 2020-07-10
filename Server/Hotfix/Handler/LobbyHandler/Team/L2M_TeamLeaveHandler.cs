using System;
using System.Collections.Generic;
using ETModel;

namespace ETHotfix
{
    [MessageHandler(AppType.Map)]
    public class L2M_TeamLeaveHandler : AMRpcHandler<L2M_TeamLeave, M2L_TeamLeave>
    {
        protected override async void Run(Session session, L2M_TeamLeave message, Action<M2L_TeamLeave> reply)
        {
            M2L_TeamLeave response = new M2L_TeamLeave();
            try
            {
                var mapUnitComponent = Game.Scene.GetComponent<MapUnitComponent>();
                MapUnit mapUnit = mapUnitComponent.GetByUid(message.Uid);

                // 判斷MapUnit是否存在在該Map上
                if (mapUnit == null)
                {
                    response.Error = ErrorCode.ERR_MapUnitMissing;
                    reply(response);
                    return;
                }

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

                // 對全體廣播離開組隊訊息(不包含自己)
                M2C_TeamModifyMember m2c_TeamModifyMember = new M2C_TeamModifyMember();
                m2c_TeamModifyMember.Uid = mapUnit.Uid;
                MapMessageHelper.BroadcastTarget(m2c_TeamModifyMember, broadcastMapUnits);

                // 廣播給自己失去房間
                M2C_TeamLose m2c_TeamLose = new M2C_TeamLose();
                m2c_TeamLose.LoseType = TeamLoseType.Other;
                MapMessageHelper.BroadcastTarget(m2c_TeamLose, mapUnit);

                bool isLeader = mapUnit.Uid == roomTeamComponent.Data.LeaderUid;

                //對全體廣播更換隊長(不包含自己)
                if (isLeader)
                {
                    M2C_TeamModifyData m2c_TeamModifyData = new M2C_TeamModifyData();
                    m2c_TeamModifyData.Data = roomTeamComponent.Data;
                    MapMessageHelper.BroadcastTarget(m2c_TeamModifyData, broadcastMapUnits);
                }

                // 判斷是否是預約玩家離開?
                if (roomTeamComponent.Data.IsReservation)
                {
                    roomTeamComponent.LeaveTeam(mapUnit);
                }

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
