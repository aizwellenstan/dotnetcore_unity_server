using System;
using System.Linq;
using ETModel;

namespace ETHotfix
{
    [ActorMessageHandler(AppType.Lobby)]
    public class C2L_QueryRelationshipByUidsHandler : AMActorLocationRpcHandler<Player, C2L_QueryRelationshipByUids, L2C_QueryRelationshipByUids>
    {
        protected override async ETTask Run(Player player, C2L_QueryRelationshipByUids message, Action<L2C_QueryRelationshipByUids> reply)
        {
            await ETTask.CompletedTask;
            RunAsync(player, message, reply).Coroutine();
        }

        private async ETTask RunAsync(Player player, C2L_QueryRelationshipByUids message, Action<L2C_QueryRelationshipByUids> reply)
        {
            L2C_QueryRelationshipByUids response = new L2C_QueryRelationshipByUids();
            try
            {
                long uid = player.uid;
                var result = await RelationshipDataHelper.QueryByUids(uid, message.Uids.ToArray());
                response.RelationshipList = result;
                reply(response);
            }
            catch (Exception e)
            {
                ReplyError(response, e, reply);
            }
        }
    }
}
