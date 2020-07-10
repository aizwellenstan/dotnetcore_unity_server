using System;
using ETModel;

namespace ETHotfix
{
    [ActorMessageHandler(AppType.Lobby)]
    public class C2L_AcceptRelationshipApplyHandler : AMActorLocationRpcHandler<Player, C2L_AcceptRelationshipApply, L2C_AcceptRelationshipApply>
    {
        protected override async ETTask Run(Player player, C2L_AcceptRelationshipApply message, Action<L2C_AcceptRelationshipApply> reply)
        {
            await ETTask.CompletedTask;
            RunAsync(player, message, reply).Coroutine();
        }

        private async ETTask RunAsync(Player player, C2L_AcceptRelationshipApply message, Action<L2C_AcceptRelationshipApply> reply)
        {
            L2C_AcceptRelationshipApply response = new L2C_AcceptRelationshipApply();
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

                //建立關係
                var relationship = await RelationshipDataHelper.AddRelationship(targetApplyInfo.SenderUid, targetApplyInfo.ReceiverUid);
                if(relationship == null)
                {
                    response.Error = ErrorCode.ERR_AddRelationshipRepeatedly;
                    reply(response);
                    return;
                }

                var receiverUser = await UserDataHelper.FindOneUser(targetApplyInfo.ReceiverUid);

                // 刪除關係申請
                await RelationshipDataHelper.RemoveRelationship(targetApplyInfo.ApplyId);

                // 確認senderUser在線, 傳送更新訊息
                var proxy = Game.Scene.GetComponent<CacheProxyComponent>();
                var playerSync = proxy.GetMemorySyncSolver<Player>();
                var senderUser = await UserDataHelper.FindOneUser(targetApplyInfo.SenderUid);
                var senderPlayer = playerSync.Get<Player>(targetApplyInfo.SenderUid);
                if(senderPlayer != null)
                {
                    // 更新好友列表
                    var notifyRelationshipState_Sender = new L2C_NotifyRelationshipState()
                    {
                        Info = new RelationshipSimpleInfo
                        {
                            DisconnectTime = 0,
                            Location = receiverUser.location,
                            Mileage = receiverUser.playerRideTotalInfo.Mileage,
                            Name = receiverUser.name,
                            RelationshipType = relationship.relationshipType,
                            Uid = uid
                        }
                    };
                    GateMessageHelper.BroadcastTarget(notifyRelationshipState_Sender, targetApplyInfo.SenderUid);

                    // 更新申請列表
                    var notifyRelationshipApplyStateIsRequested = new L2C_NotifyRelationshipApplyState()
                    {
                        DeleteApplyId = message.ApplyId,
                        IsRequested = true,
                    };
                    GateMessageHelper.BroadcastTarget(notifyRelationshipApplyStateIsRequested, targetApplyInfo.SenderUid);
                }

                // 推播告知senderUser
                var firebase = Game.Scene.GetComponent<FirebaseComponent>();
                var lang = Game.Scene.GetComponent<LanguageComponent>();
                // 1 = {0}接受你的好友邀請!
                var body = lang.GetString(senderUser.language, 1);
                await firebase.SendOneNotification(senderUser.firebaseDeviceToken, string.Empty, string.Format(body, receiverUser.name));

                response.Error = ErrorCode.ERR_Success;
                reply(response);

                // 傳送receiverUser更新申請列表
                var notifyRelationshipApplyStateNotRequested = new L2C_NotifyRelationshipApplyState()
                {
                    DeleteApplyId = message.ApplyId,
                    IsRequested = false,
                };
                GateMessageHelper.BroadcastTarget(notifyRelationshipApplyStateNotRequested, targetApplyInfo.ReceiverUid);
            }
            catch (Exception e)
            {
                ReplyError(response, e, reply);
            }
        }
    }
}
