using System;
using System.Collections.Generic;
using ETModel;

namespace ETHotfix
{
    [ActorMessageHandler(AppType.Lobby)]
    public class C2L_RefreshRelationshipApplyHandler : AMActorLocationRpcHandler<Player, C2L_RefreshRelationshipApply, L2C_RefreshRelationshipApply>
    {
        protected override async ETTask Run(Player player, C2L_RefreshRelationshipApply message, Action<L2C_RefreshRelationshipApply> reply)
        {
            await ETTask.CompletedTask;
            RunAsync(player, message, reply).Coroutine();
        }

        private async ETTask RunAsync(Player player, C2L_RefreshRelationshipApply message, Action<L2C_RefreshRelationshipApply> reply)
        {
            L2C_RefreshRelationshipApply response = new L2C_RefreshRelationshipApply();
            try
            {
                long uid = player.uid;
                List<RelationshipApplyInfo> relationApplyInfos = null;
                if (message.IsRequested)
                {
                    relationApplyInfos = await RelationshipDataHelper.GetRelationshipApplyInfoBySenderUid(uid);
                }
                else
                {
                    relationApplyInfos = await RelationshipDataHelper.GetRelationshipApplyInfoByReceiverUid(uid);
                }
                response.TotalCount = relationApplyInfos.Count;
                response.RelationshipApplyList.AddRange(relationApplyInfos);
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
