using ETModel;
using System;

namespace ETHotfix
{
    [MessageHandler(AppType.Gate)]
    public class C2G_TeamReservationGetListHandler : AMRpcHandler<C2G_TeamReservationGetList, G2C_TeamReservationGetList>
    {
        protected override void Run(Session session, C2G_TeamReservationGetList message, Action<G2C_TeamReservationGetList> reply)
        {
            G2C_TeamReservationGetList response = new G2C_TeamReservationGetList();
            try
            {
                //取得自身資料
                Player player = session.GetComponent<SessionPlayerComponent>().Player;
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
