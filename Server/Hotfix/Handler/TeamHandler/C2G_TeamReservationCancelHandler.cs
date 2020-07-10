using System;
using System.Collections.Generic;
using ETModel;

namespace ETHotfix
{
    [MessageHandler(AppType.Gate)]
    public class C2G_TeamReservationCancelHandler : AMRpcHandler<C2G_TeamReservationCancel, G2C_TeamReservationCancel>
    {
        protected override void Run(Session session, C2G_TeamReservationCancel message, Action<G2C_TeamReservationCancel> reply)
        {
            RunAsync(session, message, reply).Coroutine();
        }

        private async ETTask RunAsync(Session session, C2G_TeamReservationCancel message, Action<G2C_TeamReservationCancel> reply)
        {
            G2C_TeamReservationCancel response = new G2C_TeamReservationCancel();
            try
            {
                //取得自身資料
                Player player = session.GetComponent<SessionPlayerComponent>().Player;
                User user = await UserDataHelper.FindOneUser((player?.uid).GetValueOrDefault());
                if (user == null)
                {
                    response.Error = ErrorCode.ERR_AccountDoesntExist;
                    reply(response);
                    return;
                }

                //判斷是否為發起人
                var reservationComponent = Game.Scene.GetComponent<ReservationComponent>();
                var reservation = reservationComponent.GetByReservationId(message.ReservationId);
                if (reservation.allData.SenderUid != player.uid)
                {
                    response.Error = ErrorCode.ERR_ReservationIsNotLeader;
                    reply(response);
                    return;
                }

                if (reservation.room != null)
                {
                    if (reservation.room.State != RoomState.Ready && reservation.room.State != RoomState.Start)
                    {
                        response.Error = ErrorCode.ERR_ReservationRoomStateCanNotToRemove;
                        reply(response);
                        return;
                    }
                    else
                    {
                        //告知失去房間訊息
                        M2C_TeamLose m2c_TeamLose = new M2C_TeamLose();
                        m2c_TeamLose.LoseType = TeamLoseType.Disband;
                        List<MapUnit> broadcastMapUnits = new List<MapUnit>();
                        broadcastMapUnits.AddRange(reservation.room.GetAll());
                        MapMessageHelper.BroadcastTarget(m2c_TeamLose, broadcastMapUnits);

                        //讓隊員清空Map相關訊息
                        var proxy = Game.Scene.GetComponent<CacheProxyComponent>();
                        var playerSync = proxy.GetMemorySyncSolver<Player>();
                        var mapUnits = reservation.room.GetAll();
                        for (int i = 0; i < mapUnits?.Count; i++)
                        {
                            //Player移除mapUnitId
                            var targetPlayer = playerSync.Get<Player>(mapUnits[i].Uid);
                            targetPlayer?.LeaveRoom();

                            //中斷指定玩家與Map的連接
                            mapUnits[i].GetComponent<MapUnitGateComponent>().IsDisconnect = true;
                            //TODO:G2M_TeamDisband抄C2M_TeamDisbandHandler
                            Game.Scene.GetComponent<MapUnitComponent>().Remove(mapUnits[i].Id);
                        }

                        //刪除房間實體
                        var roomComponent = Game.Scene.GetComponent<RoomComponent>();
                        await roomComponent.DestroyRoom(reservation.room.Id);
                    }
                }

                //刪除預約實體(DB會一起刪除)
                await reservationComponent.DestroyReservation(message.ReservationId);

                //回傳資料
                response.Error = ErrorCode.ERR_Success;
                reply(response);
            }
            catch (Exception e)
            {
                ReplyError(response, e, reply);
            }
        }
    }
}
