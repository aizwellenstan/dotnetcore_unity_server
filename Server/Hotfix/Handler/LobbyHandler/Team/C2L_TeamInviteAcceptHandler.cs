using System;
using System.Net;
using ETModel;

namespace ETHotfix
{
    [ActorMessageHandler(AppType.Lobby)]
    public class C2L_TeamInviteAcceptHandler : AMActorLocationRpcHandler<Player, C2L_TeamInviteAccept, L2C_TeamInviteAccept>
    {
        protected override async ETTask Run(Player player, C2L_TeamInviteAccept message, Action<L2C_TeamInviteAccept> reply)
        {
            await RunAsync(player, message, reply);
        }

        protected async ETTask RunAsync(Player player, C2L_TeamInviteAccept message, Action<L2C_TeamInviteAccept> reply)
        {
            L2C_TeamInviteAccept response = new L2C_TeamInviteAccept();
            try
            {
                var lobbyComponent = Game.Scene.GetComponent<LobbyComponent>();
                // 取得自身資料
                User user = await UserDataHelper.FindOneUser((player?.uid).GetValueOrDefault(0));
                if (user == null)
                {
                    response.Error = ErrorCode.ERR_AccountDoesntExist;
                    reply(response);
                    return;
                }

                // 判斷邀請是否合法
                var inviteComponent = Game.Scene.GetComponent<InviteComponent>();
                var invite = inviteComponent.GetByInviteId(message.InviteId);

                if (invite == null)
                {
                    response.Error = ErrorCode.ERR_InviteIdNotFind;
                    reply(response);
                    return;
                }

                if (invite.data.ReceiverUid != player?.uid)
                {
                    response.Error = ErrorCode.ERR_InviteNotSelf;
                    reply(response);
                    return;
                }

                // 刪除該邀請
                await inviteComponent.DestroyByInviteId(message.InviteId);

                // 判斷房間是否合法
                var room = lobbyComponent.GetRoom(invite.data.TeamRoomId);
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

                // 連接到Map伺服器，並創建Unit實體
                Session mapSession = SessionHelper.GetMapSession(IdGenerater.GetAppId(room.Id));

                // 建立Map實體並進入房間
                L2M_MapUnitCreate l2M_MapUnitCreate = new L2M_MapUnitCreate();
                l2M_MapUnitCreate.Uid = player.uid;
                l2M_MapUnitCreate.GateSessionId = player.gateSessionActorId;
                l2M_MapUnitCreate.MapUnitInfo = new MapUnitInfo()
                {
                    Name = user.name,
                    Location = user.location,
                    RoomId = room.Id,
                    DistanceTravelled = 0,
                    CharSetting = user.playerCharSetting,
                    // PathId 一般組隊入場再決定
                };

                // 建立自身MapUnit
                M2L_MapUnitCreate createUnit = (M2L_MapUnitCreate)await mapSession.Call(l2M_MapUnitCreate);
                l2M_MapUnitCreate.MapUnitInfo.MapUnitId = createUnit.MapUnitId;
                player.EnterRoom(createUnit.MapUnitId, room);

                // 更新Player的MapUnitId
                await Game.Scene.GetComponent<PlayerComponent>().Update(player);

                // 對全體廣播自己剛建立的MapUnitInfo(不包含自己)
                await lobbyComponent.BroadcastTeamModifyMember(player.uid, room.Id);

                // 回傳資料
                var teamData = await lobbyComponent.GetTeamInfo(room.Id);
                response.Info = room.info;
                response.Data = teamData.Item1;
                for (int i = 0; i < teamData.Item2.Count; i++)
                {
                    if (teamData.Item2[i] != null)
                        response.MemberDatas.Add(teamData.Item2[i]);
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
