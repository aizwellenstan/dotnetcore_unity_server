using ETModel;
using System;

namespace ETHotfix
{
    [MessageHandler(AppType.Gate)]
    public class C2G_TeamInviteGetListHandler : AMRpcHandler<C2G_TeamInviteGetList, G2C_TeamInviteGetList>
    {
        protected override void Run(Session session, C2G_TeamInviteGetList message, Action<G2C_TeamInviteGetList> reply)
        {
            G2C_TeamInviteGetList response = new G2C_TeamInviteGetList();
            try
            {
                //取得自身資料
                Player player = session.GetComponent<SessionPlayerComponent>().Player;

                //取得邀請資料
                var inviteList = Game.Scene.GetComponent<InviteComponent>().GetByUid(player.uid);
                for (int i = 0; i < inviteList?.Count; i++)
                {
                    response.InviteIds.Add(inviteList[i].data.InviteId);
                    response.SenderNames.Add(inviteList[i].data.SenderName);
                }
                response.Error = 0;
                reply(response);
            }
            catch (Exception e)
            {
                ReplyError(response, e, reply);
            }
        }
    }
}
