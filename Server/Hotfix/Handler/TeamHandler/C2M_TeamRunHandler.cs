using ETModel;
using Google.Protobuf.Collections;
using System;
using System.Collections.Generic;

namespace ETHotfix
{
	[ActorMessageHandler(AppType.Map)]
	public class C2M_TeamRunHandler : AMActorLocationRpcHandler<MapUnit, C2M_TeamRun, M2C_TeamRun>
    {
        protected override async ETTask Run(MapUnit mapUnit, C2M_TeamRun message, Action<M2C_TeamRun> reply)
        {
            await ETTask.CompletedTask;

            M2C_TeamRun response = new M2C_TeamRun();
            try
            {
                if (mapUnit.Room == null)
                {
                    response.Error = ErrorCode.ERR_RoomIdNotFound;
                    reply(response);
                    return;
                }

                if (mapUnit.Room.Type != RoomType.Team)
                {
                    response.Error = ErrorCode.ERR_RoonTypeError;
                    reply(response);
                    return;
                }

                var roomTeamComponent = mapUnit.Room.GetComponent<RoomTeamComponent>();
                if (roomTeamComponent == null)
                {
                    response.Error = ErrorCode.ERR_RoomTeamComponentNull;
                    reply(response);
                    return;
                }

                if (roomTeamComponent.Data?.LeaderUid != mapUnit.Uid)
                {
                    response.Error = ErrorCode.ERR_RoomTeamIsNotLeader;
                    reply(response);
                    return;
                }

                if (mapUnit.Room.State != RoomState.Start)
                {
                    response.Error = ErrorCode.ERR_RoomTeamStateCanNotToRun;
                    reply(response);
                    return;
                }

                // 判斷是否所有成員都已準備
                bool haveNotReady = false;
                for (int i = 0; i < roomTeamComponent.MemberDatas.Length; i++)
                {
                    if (roomTeamComponent.MemberDatas[i] != null &&
                        roomTeamComponent.MemberDatas[i].Uid != mapUnit.Uid &&
                        !roomTeamComponent.MemberDatas[i].IsReady)
                    {
                        haveNotReady = true;
                        break;
                    }
                }
                if (haveNotReady)
                {
                    response.Error = ErrorCode.ERR_RoomTeamHaveNotReady;
                    reply(response);
                    return;
                }

                // 開始比賽
                roomTeamComponent.TeamLeaderRun();
                await Game.Scene.GetComponent<RoomComponent>().Update(mapUnit.Room);
                response.Error = ErrorCode.ERR_Success;
                reply(response);

                // 紀錄所有MapUnit
                RepeatedField<MapUnitInfo> mapUnitInfos = new RepeatedField<MapUnitInfo>();
                List<MapUnit> mapUnits = mapUnit.Room.GetAll();
                for (int i = 0; i < mapUnits.Count; i++)
                {
                    // PathId 入場時決定
                    mapUnits[i].Info.PathId = i % 4;
                    mapUnits[i].Info.DistanceTravelled = 0;
                    mapUnitInfos.Add(mapUnits[i].Info);
                }

                // 廣播給本隊全體
                ActorMessageSenderComponent actorLocationSenderComponent = Game.Scene.GetComponent<ActorMessageSenderComponent>();
                for (int i = 0; i < mapUnits.Count; i++)
                {
                    if (mapUnits[i].MapUnitType == MapUnitType.Npc)
                        continue;

                    if (mapUnits[i].GetComponent<MapUnitGateComponent>().IsDisconnect)
                        continue;

                    M2C_TeamGoBattle m2c_TeamGoBattle = new M2C_TeamGoBattle();
                    m2c_TeamGoBattle.MapUnitInfos = mapUnitInfos;
                    m2c_TeamGoBattle.MapUnitId = mapUnits[i].Id;

                    ActorMessageSender actorMessageSender = actorLocationSenderComponent.Get(mapUnits[i].GetComponent<MapUnitGateComponent>().GateSessionActorId);
                    actorMessageSender.Send(m2c_TeamGoBattle);
                }
            }
            catch (Exception e)
            {
                ReplyError(response, e, reply);
            }
        }
    }
}