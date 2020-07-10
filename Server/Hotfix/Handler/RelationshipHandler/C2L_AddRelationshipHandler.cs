using System;
using ETModel;

namespace ETHotfix
{
    [ActorMessageHandler(AppType.Lobby)]
    public class C2L_AddRelationshipHandler : AMActorLocationRpcHandler<Player, C2L_AddRelationship, L2C_AddRelationship>
    {
        protected override async ETTask Run(Player player, C2L_AddRelationship message, Action<L2C_AddRelationship> reply)
        {
            await ETTask.CompletedTask;
            RunAsync(player, message, reply).Coroutine();
        }

        private async ETTask RunAsync(Player player, C2L_AddRelationship message, Action<L2C_AddRelationship> reply)
        {
            L2C_AddRelationship response = new L2C_AddRelationship();
            try
            {
                long uid = player.uid;
                //Apply
                var relationshipApply = await RelationshipDataHelper.AddRelationshipApply(uid, message.Uid);
                if(relationshipApply == null)
                {
                    response.Error = ErrorCode.ERR_RelationshipApplyInfo_AddFailed;
                    reply(response);
                    return;
                }
                response.Error = ErrorCode.ERR_Success;
                reply(response);

                // 確認玩家是在線，在線的話傳送好友資訊
                var relationshipApplyInfo = RelationshipApply.ConvertToRelationshipApplyInfo(relationshipApply);
                var uidUser = await UserDataHelper.FindOneUser(uid);
                var receiverUidUser = await UserDataHelper.FindOneUser(message.Uid);
                var proxy = Game.Scene.GetComponent<CacheProxyComponent>();
                var playerSync = proxy.GetMemorySyncSolver<Player>();
                Player target = playerSync.Get<Player>(message.Uid);
                if(target != null)
                {
                    var notifyRelationshipState_SenderNotRequested = new L2C_NotifyRelationshipApplyState()
                    {
                        AddApplyInfo = relationshipApplyInfo,
                        IsRequested = false,
                    };
                    GateMessageHelper.BroadcastTarget(notifyRelationshipState_SenderNotRequested, message.Uid);
                }

                //傳給自己更新Apply列表
                var notifyRelationshipState_SenderIsRequested = new L2C_NotifyRelationshipApplyState()
                {
                    AddApplyInfo = relationshipApplyInfo,
                    IsRequested = true,
                };
                GateMessageHelper.BroadcastTarget(notifyRelationshipState_SenderIsRequested, uid);

                // 推播告知receiverUser
                var firebase = Game.Scene.GetComponent<FirebaseComponent>();
                var lang = Game.Scene.GetComponent<LanguageComponent>();
                // 7 = {0}向你發出好友邀請!
                var body = lang.GetString(receiverUidUser.language, 7);
                await firebase.SendOneNotification(receiverUidUser.firebaseDeviceToken, string.Empty, string.Format(body, uidUser.name));
            }
            catch (Exception e)
            {
                ReplyError(response, e, reply);
            }
        }
    }
}
