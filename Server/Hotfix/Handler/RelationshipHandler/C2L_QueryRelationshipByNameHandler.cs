using System;
using ETModel;

namespace ETHotfix
{
    [ActorMessageHandler(AppType.Lobby)]
    public class C2L_QueryRelationshipByNameHandler : AMActorLocationRpcHandler<Player, C2L_QueryRelationshipByName, L2C_QueryRelationshipByName>
    {
        protected override async ETTask Run(Player player, C2L_QueryRelationshipByName message, Action<L2C_QueryRelationshipByName> reply)
        {
            await ETTask.CompletedTask;
            RunAsync(player, message, reply).Coroutine();
        }

        private async ETTask RunAsync(Player player, C2L_QueryRelationshipByName message, Action<L2C_QueryRelationshipByName> reply)
        {
            L2C_QueryRelationshipByName response = new L2C_QueryRelationshipByName();
            try
            {
                long uid = player.uid;
                var result = await RelationshipDataHelper.QueryLikeName(uid, message.Name);
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
