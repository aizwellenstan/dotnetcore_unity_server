using System;
using System.Collections.Generic;
using ETModel;
using Google.Protobuf.Collections;

namespace ETHotfix
{
    [MessageHandler(AppType.Gate)]
    public class C2G_SyncPlayerStateHandler : AMRpcHandler<C2G_SyncPlayerState, G2C_SyncPlayerState>
    {
        protected override void Run(Session session, C2G_SyncPlayerState message, Action<G2C_SyncPlayerState> reply)
        {
            RunAsync(session, message, reply).Coroutine();
        }

        private async ETTask RunAsync(Session session, C2G_SyncPlayerState message, Action<G2C_SyncPlayerState> reply)
        {
            G2C_SyncPlayerState response = new G2C_SyncPlayerState();
            try
            {
                // 取得自身資料
                Player player = session.GetComponent<SessionPlayerComponent>().Player;
                User user = await UserDataHelper.FindOneUser((player?.uid).GetValueOrDefault());
                if (user == null)
                {
                    response.Error = ErrorCode.ERR_AccountDoesntExist;
                    reply(response);
                    return;
                }
                response.Error = ErrorCode.ERR_SyncPlayerStateError;
                switch (message.StateData.Type)
                {
                    case PlayerStateData.Types.StateType.Lobby:
                        {
                            // TODO 		
                            // Team = 2;  Map = 3; 問要不要回來
                            response.Type = G2C_SyncPlayerState.Types.OptionType.Nothing;
                            response.Error = ErrorCode.ERR_Success;
                        }
                        break;
                    case PlayerStateData.Types.StateType.EnterRoom:
                        {
                            switch (player.playerStateData.Type)
                            {
                                case PlayerStateData.Types.StateType.EnterRoom:
                                    {
                                        var roomTeamComponent = player.Room.GetComponent<RoomTeamComponent>();
                                        for (int i = 0; i < player.Room.MapUnitList.Count; i++)
                                        {
                                            if (player.Room.MapUnitList[i].Uid == player.uid)
                                            {
                                                player.Room.MapUnitList[i].GetComponent<MapUnitGateComponent>().GateSessionActorId = session.InstanceId;
                                                break;
                                            }
                                        }

                                        // 回傳資料
                                        response.Type = G2C_SyncPlayerState.Types.OptionType.GetInfoEnterRoom;
                                        response.Error = ErrorCode.ERR_Success;
                                        response.Info = player.Room.info;
                                        response.Data = roomTeamComponent.Data;
                                        for (int i = 0; i < roomTeamComponent.MemberDatas.Length; i++)
                                        {
                                            if (roomTeamComponent.MemberDatas[i] != null)
                                                response.MemberDatas.Add(roomTeamComponent.MemberDatas[i]);
                                        }
                                        for (int i = 0; i < roomTeamComponent.ReservationMembers?.Count; i++)
                                        {
                                            if (roomTeamComponent.ReservationMembers[i] != null)
                                                response.ReservationMemberDatas.Add(roomTeamComponent.ReservationMembers[i]);
                                        }
                                    }
                                    break;
                            }
                        }
                        break;
                    case PlayerStateData.Types.StateType.StartRoom:
                        {
                            switch (player.playerStateData.Type)
                            {
                                case PlayerStateData.Types.StateType.StartRoom:
                                    {
                                        var roomTeamComponent = player.Room.GetComponent<RoomTeamComponent>();
                                        for (int i = 0; i < player.Room.MapUnitList.Count; i++)
                                        {
                                            if (player.Room.MapUnitList[i].Uid == player.uid)
                                            {
                                                response.MapUnitId = player.Room.MapUnitList[i].Id;
                                                player.Room.MapUnitList[i].GetComponent<MapUnitGateComponent>().GateSessionActorId = session.InstanceId;
                                                break;
                                            }
                                        }

                                        // 紀錄所有MapUnit
                                        RepeatedField<MapUnitInfo> mapUnitInfos = new RepeatedField<MapUnitInfo>();
                                        List<MapUnit> mapUnits = player.Room.GetAll();
                                        for (int i = 0; i < mapUnits.Count; i++)
                                        {
                                            mapUnitInfos.Add(mapUnits[i].Info);
                                        }

                                        // 回傳資料
                                        response.Type = G2C_SyncPlayerState.Types.OptionType.GetInfoStartRoom;
                                        response.Error = ErrorCode.ERR_Success;
                                        response.MapUnitInfos = mapUnitInfos;
                                    }
                                    break;
                            }
                        }
                        break;
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
