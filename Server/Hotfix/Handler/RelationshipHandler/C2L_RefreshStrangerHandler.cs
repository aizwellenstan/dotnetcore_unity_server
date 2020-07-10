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
    public class C2L_RefreshStrangerHandler : AMActorLocationRpcHandler<Player, C2L_RefreshStranger, L2C_RefreshStranger>
    {
        protected override async ETTask Run(Player player, C2L_RefreshStranger message, Action<L2C_RefreshStranger> reply)
        {
            await ETTask.CompletedTask;
            RunAsync(player, message, reply).Coroutine();
        }

        private async ETTask RunAsync(Player player, C2L_RefreshStranger message, Action<L2C_RefreshStranger> reply)
        {
            L2C_RefreshStranger response = new L2C_RefreshStranger();
            try
            {
                long uid = player.uid;
                Tuple<List<RelationshipSimpleInfo>, long> tuple = await RelationshipDataHelper.GetStrangers(uid, message.Skip, message.Limit);
                List<RelationshipSimpleInfo> relationships = tuple.Item1;
                long totalCount = tuple.Item2;
                RepeatedField<RelationshipSimpleInfo> list = new RepeatedField<RelationshipSimpleInfo>();
                foreach(var info in relationships)
                {
                    list.Add(info);
                }

                response.TotalCount = totalCount;
                response.RelationshipList = list;
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
