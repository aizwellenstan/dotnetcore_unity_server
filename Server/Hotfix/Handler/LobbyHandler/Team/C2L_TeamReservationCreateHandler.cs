using System;
using System.Net;
using System.Collections.Generic;
using ETModel;

namespace ETHotfix
{
    [ActorMessageHandler(AppType.Lobby)]
    public class C2L_TeamReservationCreateHandler : AMActorLocationRpcHandler<Player, C2L_TeamReservationCreate, L2C_TeamReservationCreate>
    {
        protected override async ETTask Run(Player player, C2L_TeamReservationCreate message, Action<L2C_TeamReservationCreate> reply)
        {
            await RunAsync(player, message, reply);
        }

        private async ETTask RunAsync(Player player, C2L_TeamReservationCreate message, Action<L2C_TeamReservationCreate> reply)
        {
            L2C_TeamReservationCreate response = new L2C_TeamReservationCreate();
            try
            {
                User user = await UserDataHelper.FindOneUser((player?.uid).GetValueOrDefault());
                if (user == null)
                {
                    response.Error = ErrorCode.ERR_AccountDoesntExist;
                    reply(response);
                    return;
                }

                var reservationComponent = Game.Scene.GetComponent<ReservationComponent>();
                var reservationList = reservationComponent.GetByUid(player.uid);
                if (reservationList?.OwnCount >= ReservationDataHelper.ReservationMaxCount)
                {
                    response.Error = ErrorCode.ERR_ReservationIsFull;
                    reply(response);
                    return;
                }

                var reservationData = new ReservationAllData()
                {
                    ReservationId = IdGenerater.GenerateId(),
                    SenderName = user.name,
                    SenderUid = player.uid,
                    RoadSettingId = message.RoadSettingId,
                    AwakeUTCTimeTick = message.StartUTCTimeTick - ReservationDataHelper.AwakeBeforeTimeTick,
                    StartUTCTimeTick = message.StartUTCTimeTick,
                };

                //邀請對象加入自己
                reservationData.MemberUid.Add(player.uid);

                //邀請對象加入其他成員
                if (message.MemberUid?.Count > 0)
                    reservationData.MemberUid.AddRange(message.MemberUid);

                //寫入DB
                await ReservationDataHelper.Add(reservationData.ReservationId, reservationData);

                //建立預約資料實體
                var newReservation = await reservationComponent.CreateReservation(reservationData);
                var proxy = Game.Scene.GetComponent<CacheProxyComponent>();
                var playerSync = proxy.GetMemorySyncSolver<Player>();

                //告知成員
                for (int i = 0; i < reservationData.MemberUid?.Count; i++)
                {
                    //告知對方
                    long targetMemberUid = reservationData.MemberUid[i];
                    Player reservationTarget = playerSync.Get<Player>(targetMemberUid);
                    if (reservationTarget != null)
                    {
                        //在線上 廣播給指定玩家邀請訊息
                        G2C_TeamReservationAdd g2c_TeamReservationAdd = new G2C_TeamReservationAdd();
                        g2c_TeamReservationAdd.ReservationData = newReservation.data;
                        GateMessageHelper.BroadcastTarget(g2c_TeamReservationAdd, new[] { targetMemberUid });
                    }
                    // 推播
                    User u = await UserDataHelper.FindOneUser(targetMemberUid);
                    var firebase = Game.Scene.GetComponent<FirebaseComponent>();
                    var lang = Game.Scene.GetComponent<LanguageComponent>();
                    //多國3-'{0}'預約了比賽並邀請你參加!預約時間為{1}。
                    var str = lang.GetString(u.language, 3L);
                    //多國4-時。
                    var hourStr = lang.GetString(u.language, 4L);
                    //多國5-分。
                    var minuteStr = lang.GetString(u.language, 5L);
                    //多國6-秒。
                    var secondStr = lang.GetString(u.language, 6L);
                    DateTime date = new DateTime(reservationData.StartUTCTimeTick);
                    TimeSpan remainder = date - DateTime.UtcNow;
                    string dateStr = string.Empty;
                    List<string> list = new List<string>();
                    if (remainder.Hours > 0)
                    {
                        list.Add($"{remainder.Hours}{hourStr}");
                    }
                    if (remainder.Minutes > 0)
                    {
                        list.Add($"{remainder.Minutes}{minuteStr}");
                    }
                    //if (remainder.Seconds > 0)
                    //{
                    //    list.Add($"{remainder.Seconds}{secondStr}");
                    //}
                    dateStr += string.Join(" ", list);
                    await firebase.SendOneNotification(u.firebaseDeviceToken, string.Empty,
                        string.Format(str, user.name, dateStr));
                }

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
