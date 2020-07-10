using System;
using ETModel;

namespace ETHotfix
{
    [MessageHandler(AppType.Gate)]
    public class C2G_TeamInviteRefuseAllHandler : AMRpcHandler<C2G_TeamInviteRefuseAll, G2C_TeamInviteRefuseAll>
    {
        protected override void Run(Session session, C2G_TeamInviteRefuseAll message, Action<G2C_TeamInviteRefuseAll> reply)
        {
            RunAsync(session, message, reply).Coroutine();
        }

        protected async ETTask RunAsync(Session session, C2G_TeamInviteRefuseAll message, Action<G2C_TeamInviteRefuseAll> reply)
        {
            G2C_TeamInviteRefuseAll response = new G2C_TeamInviteRefuseAll();
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

                var inviteComponent = Game.Scene.GetComponent<InviteComponent>();
                var inviteList = inviteComponent.GetByUid(player.uid);
                var proxy = Game.Scene.GetComponent<CacheProxyComponent>();
                var playerSync = proxy.GetMemorySyncSolver<Player>();

                for (int i = 0; i < inviteList.Count; i++)
                {
                    //告知對方拒絕邀請
                    Player senderTarget = playerSync.Get<Player>(inviteList[i].data.SenderUid);
                    if (senderTarget != null)
                    {
                        G2C_TeamInviteTargerRefuse g2c_TeamInviteTargerRefuse = new G2C_TeamInviteTargerRefuse();
                        g2c_TeamInviteTargerRefuse.RefuseUid = player.uid;
                        GateMessageHelper.BroadcastTarget(g2c_TeamInviteTargerRefuse, inviteList[i].data.SenderUid);
                    }
                }

                //刪除自身全部邀請
                await inviteComponent.DestroyByUid(player.uid);

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
