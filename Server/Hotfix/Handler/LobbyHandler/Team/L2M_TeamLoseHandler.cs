using System;
using System.Collections.Generic;
using System.Net;
using ETModel;

namespace ETHotfix
{
    [MessageHandler(AppType.Map)]
    public class L2M_TeamLoseHandler : AMRpcHandler<L2M_TeamLose, M2L_TeamLose>
    {
        protected override void Run(Session session, L2M_TeamLose message, Action<M2L_TeamLose> reply)
        {
            RunAsync(session, message, reply);
        }

        private async void RunAsync(Session session, L2M_TeamLose message, Action<M2L_TeamLose> reply)
        {
            M2L_TeamLose response = new M2L_TeamLose();
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
                // 告知失去房間訊息
                M2C_TeamLose m2c_TeamLose = new M2C_TeamLose();
                m2c_TeamLose.LoseType = TeamLoseType.Disband;
                List<MapUnit> broadcastMapUnits = new List<MapUnit>();
                broadcastMapUnits.AddRange(room.GetAll());
                MapMessageHelper.BroadcastTarget(m2c_TeamLose, broadcastMapUnits);

                // 讓隊員清空Map相關訊息
                var proxy = Game.Scene.GetComponent<CacheProxyComponent>();
                var playerSync = proxy.GetMemorySyncSolver<Player>();
                var mapUnits = room.GetAll();
                for (int i = 0; i < mapUnits?.Count; i++)
                {
                    // Player移除mapUnitId
                    var targetPlayer = playerSync.Get<Player>(mapUnits[i].Uid);
                    targetPlayer?.LeaveRoom();
                    await playerSync.Update(targetPlayer);

                    // 中斷指定玩家與Map的連接
                    mapUnits[i].GetComponent<MapUnitGateComponent>().IsDisconnect = true;
                    // TODO:G2M_TeamDisband抄C2M_TeamDisbandHandler
                    Game.Scene.GetComponent<MapUnitComponent>().Remove(mapUnits[i].Id);
                }

                // 刪除房間實體
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
