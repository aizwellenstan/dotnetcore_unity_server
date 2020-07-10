using System;
using System.Collections.Generic;
using ETModel;

namespace ETHotfix
{
    [ActorMessageHandler(AppType.Lobby)]
    public class C2L_GetRelationshipApplyListHandler : AMActorLocationRpcHandler<Player, C2L_GetRelationshipApplyList, L2C_GetRelationshipApplyList>
    {
        protected override async ETTask Run(Player player, C2L_GetRelationshipApplyList message, Action<L2C_GetRelationshipApplyList> reply)
        {
            await ETTask.CompletedTask;
            RunAsync(player, message, reply).Coroutine();
        }

        private async ETVoid RunAsync(Player player, C2L_GetRelationshipApplyList message, Action<L2C_GetRelationshipApplyList> reply)
        {
            L2C_GetRelationshipApplyList response = new L2C_GetRelationshipApplyList();
            try
            {
                long uid = player.uid;
                if (uid == 0)
                {
                    //未被Gate授權的帳戶
                    response.Error = ErrorCode.ERR_ConnectGateKeyError;
                }
                else
                {
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
                }
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
