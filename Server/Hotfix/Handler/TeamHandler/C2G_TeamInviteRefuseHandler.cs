using System;
using ETModel;

namespace ETHotfix
{
    [MessageHandler(AppType.Gate)]
    public class C2G_TeamInviteRefuseHandler : AMRpcHandler<C2G_TeamInviteRefuse, G2C_TeamInviteRefuse>
    {
        protected override void Run(Session session, C2G_TeamInviteRefuse message, Action<G2C_TeamInviteRefuse> reply)
        {
            RunAsync(session, message, reply).Coroutine();
        }

        protected async ETTask RunAsync(Session session, C2G_TeamInviteRefuse message, Action<G2C_TeamInviteRefuse> reply)
        {
            G2C_TeamInviteRefuse response = new G2C_TeamInviteRefuse();
            try
            {
                //取得自身資料
                Player player = session.GetComponent<SessionPlayerComponent>().Player;
                User user = await UserDataHelper.FindOneUser((player?.uid).GetValueOrDefault());
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

                //告知對方拒絕邀請
                var proxy = Game.Scene.GetComponent<CacheProxyComponent>();
                var playerSync = proxy.GetMemorySyncSolver<Player>();
                Player senderTarget = playerSync.Get<Player>(invite.data.SenderUid);
                if (senderTarget != null)
                {
                    G2C_TeamInviteTargerRefuse g2c_TeamInviteTargerRefuse = new G2C_TeamInviteTargerRefuse();
                    g2c_TeamInviteTargerRefuse.RefuseUid = player.uid;
                    GateMessageHelper.BroadcastTarget(g2c_TeamInviteTargerRefuse, invite.data.SenderUid);
                }

                //刪除該邀請
                await inviteComponent.DestroyByInviteId(message.InviteId);

                //回傳資料
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
