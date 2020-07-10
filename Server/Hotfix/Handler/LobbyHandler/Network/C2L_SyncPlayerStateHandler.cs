using System;
using System.Collections.Generic;
using ETModel;
using Google.Protobuf.Collections;

namespace ETHotfix
{
    [ActorMessageHandler(AppType.Lobby)]
    public class C2L_SyncPlayerStateHandler : AMActorLocationRpcHandler<Player, C2L_SyncPlayerState, L2C_SyncPlayerState>
    {
        protected override async ETTask Run(Player player, C2L_SyncPlayerState message, Action<L2C_SyncPlayerState> reply)
        {
            await RunAsync(player, message, reply);
        }

        private async ETTask RunAsync(Player player, C2L_SyncPlayerState message, Action<L2C_SyncPlayerState> reply)
        {
            L2C_SyncPlayerState response = new L2C_SyncPlayerState();
            try
            {
                var lobbyComponent = Game.Scene.GetComponent<LobbyComponent>();
                // 取得自身資料
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
                    case PlayerStateData.Types.StateType.Start:
                        // 砍掉MapUnit
                        await lobbyComponent.DestroyMapUnit(player.mapUnitId);
                        break;
                    case PlayerStateData.Types.StateType.Lobby:
                        {
                            // TODO 		
                            // Team = 2;  Map = 3; 問要不要回來
                            response.Type = L2C_SyncPlayerState.Types.OptionType.Nothing;
                            response.Error = ErrorCode.ERR_Success;
                        }
                        break;
                    case PlayerStateData.Types.StateType.EnterRoom:
                        {
                            switch (player.playerStateData.Type)
                            {
                                case PlayerStateData.Types.StateType.EnterRoom:
                                    {
                                        var teamInfo = await lobbyComponent.GetTeamInfo(player.roomID);

                                        // 回傳資料
                                        response.Type = L2C_SyncPlayerState.Types.OptionType.GetInfoEnterRoom;
                                        response.Error = ErrorCode.ERR_Success;
                                        response.Info = player.Room.info;
                                        response.Data = teamInfo.Item1;
                                        for (int i = 0; i < teamInfo.Item2.Count; i++)
                                        {
                                            if (teamInfo.Item2[i] != null)
                                                response.MemberDatas.Add(teamInfo.Item2[i]);
                                        }
                                        for (int i = 0; i < teamInfo.Item3?.Count; i++)
                                        {
                                            if (teamInfo.Item3[i] != null)
                                                response.ReservationMemberDatas.Add(teamInfo.Item3[i]);
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
                                        // TODO:改用Player同步的方式，底層已經做過了(未測試先保留邏輯方便除錯)
                                        //for (int i = 0; i < player.Room.MapUnitList.Count; i++)
                                        //{
                                        //    if (player.Room.MapUnitList[i].Uid == player.uid)
                                        //    {
                                        //        response.MapUnitId = player.Room.MapUnitList[i].Id;
                                        //        player.Room.MapUnitList[i].GetComponent<MapUnitGateComponent>().GateSessionActorId = session.InstanceId;
                                        //        break;
                                        //    }
                                        //}

                                        // 回傳資料
                                        response.MapUnitId = player.mapUnitId;
                                        response.Type = L2C_SyncPlayerState.Types.OptionType.GetInfoStartRoom;
                                        response.Error = ErrorCode.ERR_Success;
                                        response.MapUnitInfos = await lobbyComponent.GetAllMapUnitInfoOnRoom(player.roomID);
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
