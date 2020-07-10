using System;
using System.Net;
using ETModel;

namespace ETHotfix
{
    [ActorMessageHandler(AppType.Lobby)]
    public class C2L_TeamCreateHandler : AMActorLocationRpcHandler<Player, C2L_TeamCreate, L2C_TeamCreate>
    {
        protected override async ETTask Run(Player player, C2L_TeamCreate message, Action<L2C_TeamCreate> reply)
        {
            await RunAsync(player, message, reply);
        }

        private async ETTask RunAsync(Player player, C2L_TeamCreate message, Action<L2C_TeamCreate> reply)
        {
            L2C_TeamCreate response = new L2C_TeamCreate();
            try
            {
                var lobbyComponent = Game.Scene.GetComponent<LobbyComponent>();

                User user = await UserDataHelper.FindOneUser((player?.uid).GetValueOrDefault());
                if (user == null)
                {
                    response.Error = ErrorCode.ERR_AccountDoesntExist;
                    reply(response);
                    return;
                }

                // 判斷是否還在舊的房間
                if(player.roomID != 0L)
                {
                    // 如果有就強制退房
                    await lobbyComponent.LeaveRoom(player.roomID, player.uid);
                }

                int roomcount = lobbyComponent.GetTeamRoomCount();
                if (roomcount >= lobbyComponent.GetMaxTeamRoomLimitCount())
                {
                    response.Error = ErrorCode.ERR_TooManyRooms;
                    reply(response);
                    return;
                }

                RoomInfo roomInfo = new RoomInfo()
                {
                    RoomId = 0,
                    Title = user.name,
                    RoadSettingId = message.RoadSettingId,
                    MaxMemberCount = 8,
                    NowMemberCount = 0,
                };

                TeamRoomData teamRoomData = new TeamRoomData()
                {
                    LeaderUid = player.uid,
                    LeaderName = user.name,
                    StartUTCTimeTick = -1,
                };

                // 隨機一個Map給要預約的房間
                var startConfig = NetworkHelper.GetRandomMap();
                var room = await lobbyComponent.CreateTeamRoom(startConfig.AppId, roomInfo, teamRoomData);
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

                // 連接到Map伺服器，並創建Unit實體
                Session mapSession = SessionHelper.GetMapSession(startConfig.AppId);

                // 建立Map實體並進入房間
                L2M_MapUnitCreate m2L_MapUnitCreate = new L2M_MapUnitCreate();
                m2L_MapUnitCreate.Uid = player.uid;
                m2L_MapUnitCreate.GateSessionId = player.gateSessionActorId;
                m2L_MapUnitCreate.MapUnitInfo = new MapUnitInfo()
                {
                    Name = user.name,
                    Location = user.location,
                    RoomId = room.Id,
                    DistanceTravelled = 0,
                    CharSetting = user.playerCharSetting,
                    // PathId 一般組隊入場再決定
                };

                // 建立自身MapUnit
                M2L_MapUnitCreate createUnit = (M2L_MapUnitCreate)await mapSession.Call(m2L_MapUnitCreate);
                player.EnterRoom(createUnit.MapUnitId, room);

                // 得到隊伍資訊
                var teamInfo = await lobbyComponent.GetTeamInfo(room.Id);
                if (teamInfo == null)
                {
                    response.Error = ErrorCode.ERR_RoomTeamComponentNull;
                    reply(response);
                    return;
                }

                // 更新PlayerUnit
                var playerComponent = Game.Scene.GetComponent<PlayerComponent>();
                await playerComponent.Update(player);

                // 回傳資料
                response.Info = room.info;
                response.Data = teamInfo.Item1;
                response.MemberDatas = teamInfo.Item2;
                reply(response);
            }
            catch (Exception e)
            {
                ReplyError(response, e, reply);
            }
        }
    }
}
