using System;
using ETModel;

namespace ETHotfix
{
    [ActorMessageHandler(AppType.Lobby)]
    public class C2L_TeamInviteRefuseHandler : AMActorLocationRpcHandler<Player, C2L_TeamInviteRefuse, L2C_TeamInviteRefuse>
    {
        protected override async ETTask Run(Player player, C2L_TeamInviteRefuse message, Action<L2C_TeamInviteRefuse> reply)
        {
            await RunAsync(player, message, reply);
        }

        protected async ETTask RunAsync(Player player, C2L_TeamInviteRefuse message, Action<L2C_TeamInviteRefuse> reply)
        {
            L2C_TeamInviteRefuse response = new L2C_TeamInviteRefuse();
            try
            {
                User user = await UserDataHelper.FindOneUser((player?.uid).GetValueOrDefault());
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

                // 告知對方拒絕邀請
                var proxy = Game.Scene.GetComponent<CacheProxyComponent>();
                var playerSync = proxy.GetMemorySyncSolver<Player>();
                Player senderTarget = playerSync.Get<Player>(invite.data.SenderUid);
                if (senderTarget != null)
                {
                    G2C_TeamInviteTargerRefuse g2c_TeamInviteTargerRefuse = new G2C_TeamInviteTargerRefuse();
                    g2c_TeamInviteTargerRefuse.RefuseUid = player.uid;
                    GateMessageHelper.BroadcastTarget(g2c_TeamInviteTargerRefuse, invite.data.SenderUid);
                }

                // 刪除該邀請
                await inviteComponent.DestroyByInviteId(message.InviteId);

                // 回傳資料
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
