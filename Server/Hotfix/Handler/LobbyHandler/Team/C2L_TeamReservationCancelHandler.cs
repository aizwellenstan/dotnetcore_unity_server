using System;
using System.Collections.Generic;
using ETModel;

namespace ETHotfix
{
    [ActorMessageHandler(AppType.Lobby)]
    public class C2L_TeamReservationCancelHandler : AMActorLocationRpcHandler<Player, C2L_TeamReservationCancel, L2C_TeamReservationCancel>
    {
        protected override async ETTask Run(Player player, C2L_TeamReservationCancel message, Action<L2C_TeamReservationCancel> reply)
        {
            await RunAsync(player, message, reply);
        }

        private async ETTask RunAsync(Player player, C2L_TeamReservationCancel message, Action<L2C_TeamReservationCancel> reply)
        {
            L2C_TeamReservationCancel response = new L2C_TeamReservationCancel();
            try
            {
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
                        var lobbyComponent = Game.Scene.GetComponent<LobbyComponent>();
                        // 告知失去房間訊息
                        await lobbyComponent.BroadcastTeamLose(reservation.room.Id);
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
