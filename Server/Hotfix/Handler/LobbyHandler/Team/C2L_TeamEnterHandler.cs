using System;
using System.Collections.Generic;
using System.Net;
using ETModel;

namespace ETHotfix
{
    [ActorMessageHandler(AppType.Lobby)]
    public class C2L_TeamEnterHandler : AMActorLocationRpcHandler<Player, C2L_TeamEnter, L2C_TeamEnter>
    {
        protected override async ETTask Run(Player player, C2L_TeamEnter message, Action<L2C_TeamEnter> reply)
        {
            await RunAsync(player, message, reply);
        }

        protected async ETTask RunAsync(Player player, C2L_TeamEnter message, Action<L2C_TeamEnter> reply)
        {
            L2C_TeamEnter response = new L2C_TeamEnter();
            try
            {
                var lobbyComponent = Game.Scene.GetComponent<LobbyComponent>();

                // 判斷是否還在舊的房間
                if (player.roomID != 0L)
                {
                    // 如果有就強制退房
                    await lobbyComponent.LeaveRoom(player.roomID, player.uid);
                }

                // 判斷房間是否合法
                Room room = lobbyComponent.GetRoom(message.TeamRoomId);
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

                if (room.State != RoomState.Start)
                {
                    response.Error = ErrorCode.ERR_RoomTeamStateCanNotEnter;
                    reply(response);
                    return;
                }

                if (room.info.NowMemberCount >= room.info.MaxMemberCount)
                {
                    response.Error = ErrorCode.ERR_RoomTeamMemberIsFull;
                    reply(response);
                    return;
                }

                // 取得自身資料
                User user = await UserDataHelper.FindOneUser((player?.uid).GetValueOrDefault());
                if (user == null)
                {
                    response.Error = ErrorCode.ERR_AccountDoesntExist;
                    reply(response);
                    return;
                }

                // 連接到Map伺服器，並創建Unit實體
                Session mapSession = SessionHelper.GetMapSession(IdGenerater.GetAppId(room.Id));

                // 建立Map實體並進入房間
                L2M_MapUnitCreate g2M_MapUnitCreate = new L2M_MapUnitCreate();
                g2M_MapUnitCreate.Uid = player.uid;
                g2M_MapUnitCreate.GateSessionId = player.gateSessionActorId;
                g2M_MapUnitCreate.MapUnitInfo = new MapUnitInfo()
                {
                    Name = user.name,
                    Location = user.location,
                    RoomId = room.Id,
                    DistanceTravelled = 0,
                    CharSetting = user.playerCharSetting,
                    Uid = player.uid,
                    // PathId 一般組隊入場再決定
                };

                // 建立自身MapUnit
                M2L_MapUnitCreate createUnit = (M2L_MapUnitCreate)await mapSession.Call(g2M_MapUnitCreate);
                g2M_MapUnitCreate.MapUnitInfo.MapUnitId = createUnit.MapUnitId;
                player.EnterRoom(createUnit.MapUnitId, room);

                // 更新MapUnitId
                await Game.Scene.GetComponent<PlayerComponent>().Update(player);

                // 對全體廣播自己剛建立的MapUnitInfo(不包含自己)
                await lobbyComponent.BroadcastTeamModifyMember(player.uid, room.Id);

                // 回傳資料
                var result = await lobbyComponent.GetTeamInfo(room.Id);
                response.Info = room.info;
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
