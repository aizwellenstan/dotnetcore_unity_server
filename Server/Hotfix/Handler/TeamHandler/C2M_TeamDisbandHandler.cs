using ETModel;
using System;
using System.Collections.Generic;

namespace ETHotfix
{
	[ActorMessageHandler(AppType.Map)]
	public class C2M_TeamDisbandHandler : AMActorLocationRpcHandler<MapUnit, C2M_TeamDisband, M2C_TeamDisband>
    {
        protected override async ETTask Run(MapUnit mapUnit, C2M_TeamDisband message, Action<M2C_TeamDisband> reply)
        {
            await ETTask.CompletedTask;

            M2C_TeamDisband response = new M2C_TeamDisband();
            try
            {
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

                if (roomTeamComponent.Data?.LeaderUid != mapUnit.Uid)
                {
                    response.Error = ErrorCode.ERR_RoomTeamIsNotLeader;
                    reply(response);
                    return;
                }
                response.Error = 0;
                reply(response);

                //告知失去房間訊息
                M2C_TeamLose m2c_TeamLose = new M2C_TeamLose();
                m2c_TeamLose.LoseType = TeamLoseType.Disband;
                List<MapUnit> broadcastMapUnits = new List<MapUnit>();
                broadcastMapUnits.AddRange(room.GetAll());
                MapMessageHelper.BroadcastTarget(m2c_TeamLose, broadcastMapUnits);

                //讓隊員清空Map相關訊息
                var mapUnits = room.GetAll();
                var proxy = Game.Scene.GetComponent<CacheProxyComponent>();
                var playerSync = proxy.GetMemorySyncSolver<Player>();
                for (int i = 0; i < mapUnits?.Count; i++) 
                {
                    //Player移除mapUnitId
                    var player = playerSync.Get<Player>(mapUnits[i].Uid);
                    player?.LeaveRoom();
                    await playerSync.Update(player);

                    //中斷指定玩家與Map的連接
                    mapUnits[i].GetComponent<MapUnitGateComponent>().IsDisconnect = true;
                    Game.Scene.GetComponent<MapUnitComponent>().Remove(mapUnits[i].Id);
                }

                //刪除房間實體
                var roomComponent = Game.Scene.GetComponent<RoomComponent>();
                await roomComponent.DestroyRoom(room.Id);
            }
            catch (Exception e)
            {
                ReplyError(response, e, reply);
            }
        }
    }
}