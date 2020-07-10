using System;
using System.Net;
using ETModel;

namespace ETHotfix
{
    [ActorMessageHandler(AppType.Lobby)]
    public class C2L_TeamReservationJoinHandler : AMActorLocationRpcHandler<Player, C2L_TeamReservationJoin, L2C_TeamReservationJoin>
    {
        protected override async ETTask Run(Player player, C2L_TeamReservationJoin message, Action<L2C_TeamReservationJoin> reply)
        {
            await RunAsync(player, message, reply);
        }

        private async ETTask RunAsync(Player player, C2L_TeamReservationJoin message, Action<L2C_TeamReservationJoin> reply)
        {
            L2C_TeamReservationJoin response = new L2C_TeamReservationJoin();
            try
            {
                //取得自身資料
                User user = await UserDataHelper.FindOneUser((player?.uid).GetValueOrDefault());
                if (user == null)
                {
                    response.Error = ErrorCode.ERR_AccountDoesntExist;
                    reply(response);
                    return;
                }

                //取得預約資料
                var reservationComponent = Game.Scene.GetComponent<ReservationComponent>();
                var reservation = reservationComponent.GetByReservationId(message.ReservationId);

                //判斷是否有該預約
                if (reservation == null)
                {
                    response.Error = ErrorCode.ERR_ReservationIdNotFind;
                    reply(response);
                    return;
                }

                //判斷是否為被邀請者
                if (!reservation.allData.MemberUid.Contains(player.uid))
                {
                    response.Error = ErrorCode.ERR_ReservationNotTheOwner;
                    reply(response);
                    return;
                }

                //判斷是否有該房間
                if (reservation.room == null)
                {
                    response.Error = ErrorCode.ERR_ReservationRoomNotFind;
                    reply(response);
                    return;
                }

                //判斷該房間是否可以進入
                if (reservation.room.State != RoomState.Start)
                {
                    response.Error = ErrorCode.ERR_RoomTeamStateCanNotEnter;
                    reply(response);
                    return;
                }

                //判斷是否人滿
                if (reservation.room.info.NowMemberCount >= reservation.room.info.MaxMemberCount)
                {
                    response.Error = ErrorCode.ERR_RoomTeamMemberIsFull;
                    reply(response);
                    return;
                }

                // 連接到Map伺服器，並創建Unit實體
                Session mapSession = SessionHelper.GetMapSession(IdGenerater.GetAppId(reservation.room.Id)); 

                //建立Map實體並進入房間
                L2M_MapUnitCreate g2M_MapUnitCreate = new L2M_MapUnitCreate();
                g2M_MapUnitCreate.Uid = player.uid;
                g2M_MapUnitCreate.GateSessionId = player.gateSessionActorId;
                g2M_MapUnitCreate.MapUnitInfo = new MapUnitInfo()
                {
                    Name = user.name,
                    Location = user.location,
                    RoomId = reservation.room.Id,
                    DistanceTravelled = 0,
                    CharSetting = user.playerCharSetting,
                    //PathId 一般組隊入場再決定
                };

                //建立自身MapUnit
                M2L_MapUnitCreate createUnit = (M2L_MapUnitCreate)await mapSession.Call(g2M_MapUnitCreate);
                g2M_MapUnitCreate.MapUnitInfo.MapUnitId = createUnit.MapUnitId;
                player.EnterRoom(createUnit.MapUnitId, reservation.room);
                // 更新MapUnitId
                await Game.Scene.GetComponent<PlayerComponent>().Update(player);

                var lobbyComponent = Game.Scene.GetComponent<LobbyComponent>();
                //對全體廣播自己剛建立的MapUnitInfo(不包含自己)
                await lobbyComponent.BroadcastTeamModifyMember(player.uid, reservation.room.Id);

                //回傳資料
                var result = await lobbyComponent.GetTeamInfo(reservation.room.Id);
                response.Error = ErrorCode.ERR_Success;
                response.Info = reservation.room.info;
                response.Data = result.Item1;
                for (int i = 0; i < result.Item2.Count; i++)
                {
                    if (result.Item2[i] != null)
                        response.MemberDatas.Add(result.Item2[i]);
                }
                for (int i = 0; i < result.Item3.Count; i++)
                {
                    if (result.Item3[i] != null)
                        response.ReservationMemberDatas.Add(result.Item3[i]);
                }
                reply(response);
            }
            catch (Exception e)
            {
                ReplyError(response, e, reply);
            }
        }
    }
}
