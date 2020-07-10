using ETModel;
using System;

namespace ETHotfix
{
    [ActorMessageHandler(AppType.Lobby)]
    public class C2L_TeamInviteGetListHandler : AMActorLocationRpcHandler<Player, C2L_TeamInviteGetList, L2C_TeamInviteGetList>
    {
        protected override async ETTask Run(Player player, C2L_TeamInviteGetList message, Action<L2C_TeamInviteGetList> reply)
        {
            await ETTask.CompletedTask;

            L2C_TeamInviteGetList response = new L2C_TeamInviteGetList();
            try
            {
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
