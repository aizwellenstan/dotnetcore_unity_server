using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using ETModel;
using Google.Protobuf.Collections;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;

namespace ETHotfix
{
    [ActorMessageHandler(AppType.Lobby)]
    public class C2L_RemoveRelationshipHandler : AMActorLocationRpcHandler<Player, C2L_RemoveRelationship, L2C_RemoveRelationship>
    {
        protected override async ETTask Run(Player player, C2L_RemoveRelationship message, Action<L2C_RemoveRelationship> reply)
        {
            await ETTask.CompletedTask;
            RunAsync(player, message, reply).Coroutine();
        }

        private async ETTask RunAsync(Player player, C2L_RemoveRelationship message, Action<L2C_RemoveRelationship> reply)
        {
            L2C_RemoveRelationship response = new L2C_RemoveRelationship();
            try
            {
                long uid = player.uid;
                await RelationshipDataHelper.RemoveRelationship(uid, message.Uid);
                response.Error = ErrorCode.ERR_Success;
                reply(response);
                L2C_NotifyRelationshipState notifyRelationshipState = new L2C_NotifyRelationshipState();
                RelationshipSimpleInfo info = new RelationshipSimpleInfo();
                {
                    info.Uid = uid;
                }
                notifyRelationshipState.Info = info;
                GateMessageHelper.BroadcastTarget(notifyRelationshipState, message.Uid);
            }
            catch (Exception e)
            {
                ReplyError(response, e, reply);
            }
        }
    }
}
