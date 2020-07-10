using System;
using System.Collections.Generic;
using System.Net;
using ETModel;

namespace ETHotfix
{
    [ActorMessageHandler(AppType.Lobby)]
    public class C2L_RoamingEnterHandler : AMActorLocationRpcHandler<Player, C2L_RoamingEnter, L2C_RoamingEnter>
    {
        protected override async ETTask Run(Player player, C2L_RoamingEnter message, Action<L2C_RoamingEnter> reply)
        {
            await RunAsync(player, message, reply);
        }

        protected async ETTask RunAsync(Player player, C2L_RoamingEnter message, Action<L2C_RoamingEnter> reply)
        {
            L2C_RoamingEnter response = new L2C_RoamingEnter();
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
                Room room = lobbyComponent.GetRoom(message.RoamingRoomId);
                if (room == null)
                {
                    response.Error = ErrorCode.ERR_RoomIdNotFound;
                    reply(response);
                    return;
                }

                // 0518 Saitou add member Limit
                if (room.info.NowMemberCount >= room.info.MaxMemberCount)
                {
                    response.Error = ErrorCode.ERR_RoomRoamingMemberIsFull;
                    reply(response);
                    return;
                }

                // 回傳房間設定
                response.RoadSettingId = room.info.RoadSettingId;

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
                    RoomId = message.RoamingRoomId,
                    DistanceTravelled = 0,
                    SpeedMS = 0,
                    CharSetting = user.playerCharSetting,
                    PathId = room.info.NowMemberCount % 4,
                    StartUTCTick = DateTime.UtcNow.Ticks,
                    Uid = player.uid,
                };

                // 建立自身MapUnit
                M2L_MapUnitCreate createUnit = (M2L_MapUnitCreate)await mapSession.Call(g2M_MapUnitCreate);
                g2M_MapUnitCreate.MapUnitInfo.MapUnitId = createUnit.MapUnitId;

                // 進入房間
                player.EnterRoom(createUnit.MapUnitId, room);
                player.StartRoom();

                // 更新MapUnitId
                await Game.Scene.GetComponent<PlayerComponent>().Update(player);

                // TODO:紀錄
                // 回傳自己MapUnitInfo
                response.SelfInfo = g2M_MapUnitCreate.MapUnitInfo;
                response.GlobalInfos = await lobbyComponent.GetAllMapUnitGlobalInfoOnRoom(room.Id);
                reply(response);
            }
            catch (Exception e)
            {
                ReplyError(response, e, reply);
            }
        }
    }
}
