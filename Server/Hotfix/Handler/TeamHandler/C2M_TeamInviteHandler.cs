using ETModel;
using MongoDB.Bson.Serialization;
using System;

namespace ETHotfix
{
    [ActorMessageHandler(AppType.Map)]
    public class C2M_TeamInviteHandler : AMActorLocationRpcHandler<MapUnit, C2M_TeamInvite, M2C_TeamInvite>
    {
        protected override async ETTask Run(MapUnit mapUnit, C2M_TeamInvite message, Action<M2C_TeamInvite> reply)
        {
            await ETTask.CompletedTask;

            M2C_TeamInvite response = new M2C_TeamInvite();
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

                if (mapUnit.Room.State != RoomState.Start)
                {
                    response.Error = ErrorCode.ERR_RoomTeamStateCanNotInvite;
                    reply(response);
                    return;
                }

                User user = await UserDataHelper.FindOneUser(mapUnit.Uid);
                if (user == null)
                {
                    response.Error = ErrorCode.ERR_AccountDoesntExist;
                    reply(response);
                    return;
                }

                // 建立邀請
                InviteData inviteData = new InviteData()
                {
                    SenderName = user.name,
                    SenderUid = mapUnit.Uid,
                    ReceiverUid = message.ReceiverUid,
                    TeamRoomId = mapUnit.Room.Id,
                    RoadSettingId = mapUnit.Room.info.RoadSettingId
                };

                var player = CacheExHelper.GetFromCache<Player>(mapUnit.Uid);
                var lobbySession = SessionHelper.GetLobbySession(player.lobbyAppId);
                L2M_CreateInvite l2M_CreateInvite = (L2M_CreateInvite)await lobbySession.Call(new M2L_CreateInvite
                {
                    InviteInfo = inviteData,
                });

                if(l2M_CreateInvite.Error != ErrorCode.ERR_Success)
                {
                    response.Error = ErrorCode.ERR_InviteIdNotFind;
                    reply(response);
                    return;
                }

                Invite inviteEntity = BsonSerializer.Deserialize<Invite>(l2M_CreateInvite.Json);
                CacheExHelper.WriteInCache(inviteEntity, out inviteEntity);

                // 告知對方
                Player inviteTarget = CacheExHelper.GetFromCache<Player>(message.ReceiverUid);
                if (inviteTarget != null)
                {
                    //在線上 廣播給指定玩家邀請訊息
                    G2C_TeamInviteReceiver g2c_TeamInviteReceiver = new G2C_TeamInviteReceiver();
                    g2c_TeamInviteReceiver.InviteId = inviteEntity.Id;
                    g2c_TeamInviteReceiver.SenderName = inviteEntity.data.SenderName;
                    GateMessageHelper.BroadcastTarget(g2c_TeamInviteReceiver, message.ReceiverUid);
                }
                // 推播
                User u = await UserDataHelper.FindOneUser(message.ReceiverUid);
                var firebase = Game.Scene.GetComponent<FirebaseComponent>();
                var lang = Game.Scene.GetComponent<LanguageComponent>();
                // 多國2-'{0}'邀請你一起組隊!
                var body = lang.GetString(u.language, 2L);
                await firebase.SendOneNotification(u.firebaseDeviceToken, string.Empty, string.Format(body, user.name));

                response.Error = ErrorCode.ERR_Success;
                reply(response);
            }
            catch (Exception e)
            {
                ReplyError(response, e, reply);
            }
        }
    }
}
