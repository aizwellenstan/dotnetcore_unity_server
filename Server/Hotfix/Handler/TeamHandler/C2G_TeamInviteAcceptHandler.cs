using System;
using System.Net;
using ETModel;

namespace ETHotfix
{
    [MessageHandler(AppType.Gate)]
    public class C2G_TeamInviteAcceptHandler : AMRpcHandler<C2G_TeamInviteAccept, G2C_TeamInviteAccept>
    {
        protected override void Run(Session session, C2G_TeamInviteAccept message, Action<G2C_TeamInviteAccept> reply)
        {
            RunAsync(session, message, reply).Coroutine();
        }

        protected async ETTask RunAsync(Session session, C2G_TeamInviteAccept message, Action<G2C_TeamInviteAccept> reply)
        {
            G2C_TeamInviteAccept response = new G2C_TeamInviteAccept();
            try
            {
                //取得自身資料
                Player player = session.GetComponent<SessionPlayerComponent>().Player;
                User user = await UserDataHelper.FindOneUser((player?.uid).GetValueOrDefault(0));
                if (user == null)
                {
                    response.Error = ErrorCode.ERR_AccountDoesntExist;
                    reply(response);
                    return;
                }

                //判斷邀請是否合法
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

                //刪除該邀請
                await inviteComponent.DestroyByInviteId(message.InviteId);

                //判斷房間是否合法
                var room = Game.Scene.GetComponent<RoomComponent>().Get(invite.data.TeamRoomId);
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

                var roomTeamComponent = room.GetComponent<RoomTeamComponent>();
                if (roomTeamComponent == null)
                {
                    response.Error = ErrorCode.ERR_RoomTeamComponentNull;
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

                //對全體廣播自己剛建立的MapUnitInfo(不包含自己)
                TeamMemberData selfMemberData = roomTeamComponent.GetMember(player.uid);
                M2C_TeamModifyMember m2c_TeamModifyMember = new M2C_TeamModifyMember();
                m2c_TeamModifyMember.Uid = player.uid;
                m2c_TeamModifyMember.MemberData = selfMemberData;
                MapMessageHelper.BroadcastRoom(room.Id, m2c_TeamModifyMember, player.uid);

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
