using System;
using System.Collections.Generic;
using System.Net;
using ETModel;

namespace ETHotfix
{
    [MessageHandler(AppType.Gate)]
    public class C2G_RoamingEnterHandler : AMRpcHandler<C2G_RoamingEnter, G2C_RoamingEnter>
    {
        protected override void Run(Session session, C2G_RoamingEnter message, Action<G2C_RoamingEnter> reply)
        {
            RunAsync(session, message, reply).Coroutine();
        }

        protected async ETTask RunAsync(Session session, C2G_RoamingEnter message, Action<G2C_RoamingEnter> reply)
        {
            G2C_RoamingEnter response = new G2C_RoamingEnter();
            try
            {
                //判斷房間是否合法
                Room room = Game.Scene.GetComponent<RoomComponent>().Get(message.RoamingRoomId);
                if (room == null)
                {
                    response.Error = ErrorCode.ERR_RoomIdNotFound;
                    reply(response);
                    return;
                }

                //回傳房間設定
                response.RoadSettingId = room.info.RoadSettingId;

                //取得自身資料
                Player player = session.GetComponent<SessionPlayerComponent>().Player;
                User user = await UserDataHelper.FindOneUser((player?.uid).GetValueOrDefault());
                if (user == null)
                {
                    response.Error = ErrorCode.ERR_AccountDoesntExist;
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
                    RoomId = message.RoamingRoomId,
                    DistanceTravelled = 0,
                    SpeedMS = 0,
                    CharSetting = user.playerCharSetting,
                    PathId = room.info.NowMemberCount % 4,
                    StartUTCTick = DateTime.UtcNow.Ticks
                };

                //建立自身MapUnit
                M2G_MapUnitCreate createUnit = (M2G_MapUnitCreate)await mapSession.Call(g2M_MapUnitCreate);
                g2M_MapUnitCreate.MapUnitInfo.MapUnitId = createUnit.MapUnitId;

                //進入房間
                player.EnterRoom(createUnit.MapUnitId, room);
                player.StartRoom();

                //TODO:紀錄
                //回傳自己MapUnitInfo
                response.SelfInfo = g2M_MapUnitCreate.MapUnitInfo;
                var mapUnits = room.GetAll();
                for (int i = 0; i < mapUnits.Count; i++)
                {
                    response.GlobalInfos.Add(mapUnits[i].GlobalInfo);
                }
                reply(response);

                ////對全體廣播MapUnitInfo(不包含自己)
                //M2C_MapUnitCreate m2C_MapUnitCreate = new M2C_MapUnitCreate();
                //m2C_MapUnitCreate.MapUnitInfo = g2M_MapUnitCreate.MapUnitInfo;
                //MapMessageHelper.BroadcastRoom(g2M_MapUnitCreate.MapUnitInfo.RoomId, m2C_MapUnitCreate, player.Uid);

                // 紀錄所有MapUnit(包含自己)
                //List<MapUnit> mapUnits = room.GetAll();
                //for (int i = 0; i < mapUnits.Count; i++)
                //{
                //    response.MapUnitInfos.Add(mapUnits[i].Info);
                //}


            }
            catch (Exception e)
            {
                ReplyError(response, e, reply);
            }
        }
    }
}
