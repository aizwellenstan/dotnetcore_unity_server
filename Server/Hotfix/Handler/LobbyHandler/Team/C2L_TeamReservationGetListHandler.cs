using ETModel;
using System;

namespace ETHotfix
{
    [ActorMessageHandler(AppType.Lobby)]
    public class C2L_TeamReservationGetListHandler : AMActorLocationRpcHandler<Player, C2L_TeamReservationGetList, L2C_TeamReservationGetList>
    {
        protected override async ETTask Run(Player player, C2L_TeamReservationGetList message, Action<L2C_TeamReservationGetList> reply)
        {
            await ETTask.CompletedTask;

            L2C_TeamReservationGetList response = new L2C_TeamReservationGetList();
            try
            {
                if (player == null)
                {
                    response.Error = ErrorCode.ERR_PlayerDoesntExist;
                    reply(response);
                    return;
                }

                //取得預約資料
                var reservationList = Game.Scene.GetComponent<ReservationComponent>().GetByUid(player.uid);
                if (reservationList != null)
                    response.ReservationDatas = reservationList.Datas;
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
