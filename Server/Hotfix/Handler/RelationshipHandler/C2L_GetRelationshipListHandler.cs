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
    public class C2L_GetRelationshipListHandler : AMActorLocationRpcHandler<Player, C2L_GetRelationshipList, L2C_GetRelationshipList>
    {
        protected override async ETTask Run(Player player, C2L_GetRelationshipList message, Action<L2C_GetRelationshipList> reply)
        {
            await ETTask.CompletedTask;
            RunAsync(player, message, reply).Coroutine();
        }

        private async ETTask RunAsync(Player player, C2L_GetRelationshipList message, Action<L2C_GetRelationshipList> reply)
        {
            L2C_GetRelationshipList response = new L2C_GetRelationshipList();
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
                    List<RelationshipSimpleInfo> relations = await RelationshipDataHelper.GetUserRelationshipSimpleInfoList(uid);
                    Tuple<List<RelationshipSimpleInfo>, long> tuple = await RelationshipDataHelper.GetStrangers(uid);
                    List<RelationshipSimpleInfo> stranger = tuple.Item1;
                    long totalCount = tuple.Item2;
                    RepeatedField<RelationshipSimpleInfo> list = new RepeatedField<RelationshipSimpleInfo>();
                    var linq = Enumerable.Union(relations, stranger);
                    foreach (var info in linq)
                    {
                        list.Add(info);
                    }
                    response.TotalCount = totalCount;
                    response.RelationshipList = list;
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
