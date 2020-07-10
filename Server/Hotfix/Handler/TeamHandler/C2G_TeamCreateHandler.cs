using System;
using System.Net;
using ETModel;

namespace ETHotfix
{
    [MessageHandler(AppType.Gate)]
    public class C2G_TeamCreateHandler : AMRpcHandler<C2G_TeamCreate, G2C_TeamCreate>
    {
        protected override void Run(Session session, C2G_TeamCreate message, Action<G2C_TeamCreate> reply)
        {
            RunAsync(session, message, reply).Coroutine();
        }

        private async ETTask RunAsync(Session session, C2G_TeamCreate message, Action<G2C_TeamCreate> reply)
        {
            G2C_TeamCreate response = new G2C_TeamCreate();
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

                var roomComponent = Game.Scene.GetComponent<RoomComponent>();
                var room = await roomComponent.CreateTeamRoom(roomInfo, teamRoomData);
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

                var roomTeamComponent = room.GetComponent<RoomTeamComponent>();
                if (roomTeamComponent == null)
                {
                    response.Error = ErrorCode.ERR_RoomTeamComponentNull;
                    reply(response);
                    return;
                }

                // 連接到Map伺服器，並創建Unit實體
                IPEndPoint mapAddress = StartConfigComponent.Instance.Get(player.mapAppId).GetComponent<InnerConfig>().IPEndPoint;
                Session mapSession = Game.Scene.GetComponent<NetInnerComponent>().Get(mapAddress);

                //建立Map實體並進入房間
                G2M_MapUnitCreate g2M_MapUnitCreate = new G2M_MapUnitCreate();
                g2M_MapUnitCreate.Uid = player.uid;
                g2M_MapUnitCreate.GateSessionId = session.InstanceId;
                g2M_MapUnitCreate.MapUnitInfo = new MapUnitInfo()
                {
                    Name = user.name,
                    Location = user.location,
                    RoomId = room.Id,
                    DistanceTravelled = 0,
                    CharSetting = user.playerCharSetting,
                    //PathId 一般組隊入場再決定
                };

                //建立自身MapUnit
                M2G_MapUnitCreate createUnit = (M2G_MapUnitCreate)await mapSession.Call(g2M_MapUnitCreate);
                g2M_MapUnitCreate.MapUnitInfo.MapUnitId = createUnit.MapUnitId;
                player.EnterRoom(createUnit.MapUnitId, room);

                //回傳資料
                response.Info = room.info;
                response.Data = roomTeamComponent.Data;
                for (int i = 0; i < roomTeamComponent.MemberDatas.Length; i++)
                {
                    if (roomTeamComponent.MemberDatas[i] != null)
                        response.MemberDatas.Add(roomTeamComponent.MemberDatas[i]);
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
