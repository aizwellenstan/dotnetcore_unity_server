using System;
using ETModel;

namespace ETHotfix
{
    [ActorMessageHandler(AppType.Lobby)]
    public class C2L_RefuseRelationshipApplyHandler : AMActorLocationRpcHandler<Player, C2L_RefuseRelationshipApply, L2C_RefuseRelationshipApply>
    {
        protected override async ETTask Run(Player player, C2L_RefuseRelationshipApply message, Action<L2C_RefuseRelationshipApply> reply)
        {
            await ETTask.CompletedTask;
            RunAsync(player, message, reply).Coroutine();
        }

        private async ETTask RunAsync(Player player, C2L_RefuseRelationshipApply message, Action<L2C_RefuseRelationshipApply> reply)
        {
            L2C_RefuseRelationshipApply response = new L2C_RefuseRelationshipApply();
            try
            {
                long uid = player.uid;

                //尋找指定RelationshipApplyInfo
                var targetApplyInfo = await RelationshipDataHelper.GetRelationshipApplyInfoByApplyId(message.ApplyId);

                //判斷是否存在RelationshipApplyInfo
                if (targetApplyInfo == null)
                {
                    response.Error = ErrorCode.ERR_RelationshipApplyInfo_NotFind;
                    reply(response);
                    return;
                }

                //判斷是否為申請目標
                if (targetApplyInfo.ReceiverUid != uid)
                {
                    response.Error = ErrorCode.ERR_RelationshipApplyInfo_NotReceiver;
                    reply(response);
                    return;
                }

                // 刪除關係申請
                await RelationshipDataHelper.RemoveRelationship(targetApplyInfo.ApplyId);

                // 傳送senderUser更新申請列表
                var notifyRelationshipApplyStateIsRequested = new L2C_NotifyRelationshipApplyState()
                {
                    DeleteApplyId = message.ApplyId,
                    IsRequested = true,
                };
                GateMessageHelper.BroadcastTarget(notifyRelationshipApplyStateIsRequested, targetApplyInfo.SenderUid);

                // 傳送receiverUser更新申請列表
                var notifyRelationshipApplyStateNotRequested = new L2C_NotifyRelationshipApplyState()
                {
                    DeleteApplyId = message.ApplyId,
                    IsRequested = false,
                };
                GateMessageHelper.BroadcastTarget(notifyRelationshipApplyStateNotRequested, targetApplyInfo.ReceiverUid);

                var uidUser = await UserDataHelper.FindOneUser(uid);
                var senderUser = await UserDataHelper.FindOneUser(targetApplyInfo.SenderUid);
                // 推播告知senderUser
                var firebase = Game.Scene.GetComponent<FirebaseComponent>();
                var lang = Game.Scene.GetComponent<LanguageComponent>();
                // 8 = {0}拒絕你的好友邀請!
                var body = lang.GetString(senderUser.language, 8);
                await firebase.SendOneNotification(senderUser.firebaseDeviceToken, string.Empty, string.Format(body, uidUser.name));

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
