using System;
using System.Collections.Generic;
using System.Net;
using ETModel;
using Google.Protobuf.Collections;

namespace ETHotfix
{
    [MessageHandler(AppType.Map)]
    public class L2M_RunRoomOnTeamHandler : AMRpcHandler<L2M_RunRoomOnTeam, M2L_RunRoomOnTeam>
    {
        protected override void Run(Session session, L2M_RunRoomOnTeam message, Action<M2L_RunRoomOnTeam> reply)
        {
            RunAsync(session, message, reply);
        }

        private async void RunAsync(Session session, L2M_RunRoomOnTeam message, Action<M2L_RunRoomOnTeam> reply)
        {
            M2L_RunRoomOnTeam response = new M2L_RunRoomOnTeam();
            try
            {
                var roomComponent = Game.Scene.GetComponent<RoomComponent>();
                var room = roomComponent.Get(message.RoomId);
                if (room == null)
                {
                    response.Error = ErrorCode.ERR_RoomIdNotFound;
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
                roomTeamComponent.TeamLeaderRun();
                await roomComponent.Update(room);

                RepeatedField<MapUnitInfo> mapUnitInfos = new RepeatedField<MapUnitInfo>();
                List<MapUnit> mapUnits = room.GetAll();
                for (int i = 0; i < mapUnits.Count; i++)
                {
                    mapUnits[i].Info.PathId = i % 4;
                    mapUnitInfos.Add(mapUnits[i].Info);
                }

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

                reply(response);
            }
            catch (Exception e)
            {
                ReplyError(response, e, reply);
            }
        }
    }
}
