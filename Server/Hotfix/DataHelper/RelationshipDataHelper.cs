using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
using ETModel;
using Google.Protobuf.Collections;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;

namespace ETHotfix
{
    public static class RelationshipDataHelper
    {
        private const long getStrangerLimitCount = 20L;

        private static DBProxyComponent dbProxy
        {
            get
            {
                return Game.Scene.GetComponent<DBProxyComponent>();
            }
        }

        /// <summary>
        /// 查詢使用者的所有關係
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        public static async ETTask<List<Relationship>> GetUserRelationshipList(long uid)
        {
            var list = await dbProxy.Query<Relationship>(entity => entity.uid == uid);
            return list.OfType<Relationship>().ToList();
        }

        /// <summary>
        /// 查詢使用者的所有關係，包含User的基礎資料
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        public static async ETTask<List<RelationshipSimpleInfo>> GetUserRelationshipSimpleInfoList(long uid)
        {
            var relations = await GetUserRelationshipList(uid);
            var relationDict = relations.ToDictionary(e => e.targetUid, e => e);
            var users = await UserDataHelper.FindUsers(relations.Select(e => e.targetUid).ToArray());
            return users.Select(user =>
            {
                Relationship rel = null;
                RelationshipSimpleInfo info = new RelationshipSimpleInfo();
                info.Name = user.name;
                info.Location = user.location;
                info.Mileage = user.playerRideTotalInfo.Mileage;
                info.Uid = user.Id;
                info.DisconnectTime = 0;
                if (relationDict.TryGetValue(user.Id, out rel))
                {
                    info.RelationshipType = rel.relationshipType;
                }
                else
                {
                    info.RelationshipType = (int)Relationship.RelationType.Stranger;
                }
                return info;
            }).ToList();
        }

        #region Apply

        /// <summary>
        /// 查詢申請列表，用ApplyId
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        public static async ETTask<RelationshipApplyInfo> GetRelationshipApplyInfoByApplyId(long applyId)
        {
            var relationshipApplys = await dbProxy.Query<RelationshipApply>(entity => entity.applyId == applyId);
            if (relationshipApplys != null && relationshipApplys.Count > 0)
            {
                var targetApply = relationshipApplys[0] as RelationshipApply;
                if (targetApply != null)
                    return RelationshipApply.ConvertToRelationshipApplyInfo(targetApply);
            }
            return null;
        }

        /// <summary>
        /// 查詢申請列表，用ReceiverUid
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        public static async ETTask<List<RelationshipApplyInfo>> GetRelationshipApplyInfoByReceiverUid(long receiverUid)
        {
            var relationshipApplys = await dbProxy.Query<RelationshipApply>(entity => entity.receiverUid == receiverUid);
            var infos = new List<RelationshipApplyInfo>();
            for (int i = 0; i < relationshipApplys.Count; i++)
            {
                infos.Add(RelationshipApply.ConvertToRelationshipApplyInfo(relationshipApplys[i] as RelationshipApply));
            }
            return infos;
        }

        /// <summary>
        /// 查詢申請列表，用SenderUid
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        public static async ETTask<List<RelationshipApplyInfo>> GetRelationshipApplyInfoBySenderUid(long senderUid)
        {
            var relationshipApplys = await dbProxy.Query<RelationshipApply>(entity => entity.senderUid == senderUid);
            var infos = new List<RelationshipApplyInfo>();
            for (int i = 0; i < relationshipApplys.Count; i++)
            {
                infos.Add(RelationshipApply.ConvertToRelationshipApplyInfo(relationshipApplys[i] as RelationshipApply));
            }
            return infos;
        }

        /// <summary>
        /// 是否已存在關係申請
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="targetUid"></param>
        /// <returns></returns>
        public static async ETTask<bool> ExistRelationshipApply(long senderUid, long receiverUid)
        {
            var list = await dbProxy.Query<RelationshipApply>(entity => entity.senderUid == senderUid && entity.receiverUid == receiverUid);
            return list.Count != 0;
        }

        /// <summary>
        /// 建立關係申請
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="targetUid"></param>
        /// <returns></returns>
        public static async ETTask<RelationshipApply> AddRelationshipApply(long senderUid, long receiverUid)
        {
            bool exist = await ExistRelationshipApply(senderUid, receiverUid);
            if (exist)
            {
                return null;
            }

            var relationshipApply = ComponentFactory.CreateWithId<RelationshipApply>(IdGenerater.GenerateId());
            relationshipApply.applyId = relationshipApply.Id;

            var senderUser = await UserDataHelper.FindOneUser(senderUid);
            relationshipApply.senderUid = senderUid;
            relationshipApply.senderName = senderUser.name;
            relationshipApply.senderLocation = senderUser.location;
            relationshipApply.senderMileage = senderUser.playerRideTotalInfo.Mileage;

            var receiverUser = await UserDataHelper.FindOneUser(receiverUid);
            relationshipApply.receiverUid = receiverUid;
            relationshipApply.receiverName = receiverUser.name;
            relationshipApply.receiverLocation = receiverUser.location;
            relationshipApply.receiverMileage = receiverUser.playerRideTotalInfo.Mileage;

            await dbProxy.Save(relationshipApply);
            await dbProxy.SaveLog(senderUid, DBLog.LogType.RelationshipApply, relationshipApply);
            return relationshipApply;
        }

        /// <summary>
        /// 刪除關係申請
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="targetUid"></param>
        /// <returns></returns>
        public static async ETTask RemoveRelationship(long applyId)
        {
            var relationshipApplys = await dbProxy.Query<RelationshipApply>(entity => entity.applyId == applyId);
            if (relationshipApplys == null || relationshipApplys.Count == 0)
            {
                Log.Error($"RemoveRelationship failed, RelationshipApply doesnt exist!, applyId:{applyId}");
                return;
            }
            Expression<Func<RelationshipApply, bool>> exp = entity => (entity.applyId == applyId);
            await dbProxy.DeleteJson(exp);
            await dbProxy.SaveLog(applyId, DBLog.LogType.RelationshipApply, relationshipApplys[0]);
        }

        #endregion

        /// <summary>
        /// 根據筆數跟偏移，尋找跟自己沒有關係的人們
        /// 回傳資料集合跟DB表的總長度
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="skip"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        public static async ETTask<Tuple<List<RelationshipSimpleInfo>, long>> GetStrangers(long uid, long skip = 0L, long limit = getStrangerLimitCount)
        {
            var relations = await GetUserRelationshipList(uid);
            var excludes = relations.Select(e => e.targetUid).ToList();

            // 排除自己
            excludes.Add(uid);

            // 排除已申請的玩家
            var relationApplyInfSenders = await GetRelationshipApplyInfoBySenderUid(uid);
            for (int i = 0; i < relationApplyInfSenders.Count; i++)
            {
                excludes.Add(relationApplyInfSenders[i].ReceiverUid);
            }

            // 排除向自己申請的玩家
            var relationApplyInfoReceivers = await GetRelationshipApplyInfoByReceiverUid(uid);
            for (int i = 0; i < relationApplyInfoReceivers.Count; i++)
            {
                excludes.Add(relationApplyInfoReceivers[i].SenderUid);
            }

            List<User> users = await UserDataHelper.FindUsers(entity => !excludes.Contains(entity.Id), skip, limit);
            long totalCount = await dbProxy.QueryCount<User>(entity => !excludes.Contains(entity.Id));
            List<RelationshipSimpleInfo> list = users.Select(user =>
            {
                RelationshipSimpleInfo info = new RelationshipSimpleInfo();
                info.Name = user.name;
                info.Location = user.location;
                info.Mileage = user.playerRideTotalInfo.Mileage;
                info.Uid = user.Id;
                info.DisconnectTime = 0;
                info.RelationshipType = (int)Relationship.RelationType.Stranger;
                return info;
            }).ToList();
            return new Tuple<List<RelationshipSimpleInfo>, long>(list, totalCount);
        }

        /// <summary>
        /// 更新或插入一筆關係
        /// </summary>
        /// <param name="relationship"></param>
        /// <returns></returns>
        private static async ETTask UpsertRelationship(Relationship relationship)
        {
            await dbProxy.Save(relationship);
            await dbProxy.SaveLog(relationship.uid, DBLog.LogType.Relationship, relationship);
        }

        /// <summary>
        /// 彼此是否有關係?
        /// TODO:用指令優化
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="targetUid"></param>
        /// <returns></returns>
        public static async ETTask<bool> ExistRelationship(long uid, long targetUid)
        {
            var list = await dbProxy.Query<Relationship>(entity => entity.uid == uid && entity.targetUid == targetUid);
            return list.Count != 0;
        }

        /// <summary>
        /// 建立關係
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="targetUid"></param>
        /// <returns></returns>
        public static async ETTask<Relationship> AddRelationship(long uid, long targetUid)
        {
            bool exist = await ExistRelationship(uid, targetUid);
            if (exist)
            {
                return null;
            }
            long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            Relationship relationship1 = ComponentFactory.CreateWithId<Relationship>(IdGenerater.GenerateId());
            relationship1.uid = uid;
            relationship1.targetUid = targetUid;
            relationship1.relationshipType = (int)Relationship.RelationType.Friend;
            relationship1.confirmedAt = now;
            relationship1.createAt = now;
            Relationship relationship2 = ComponentFactory.CreateWithId<Relationship>(IdGenerater.GenerateId());
            relationship2.uid = targetUid;
            relationship2.targetUid = uid;
            relationship2.relationshipType = (int)Relationship.RelationType.Friend;
            relationship2.confirmedAt = now;
            relationship2.createAt = now;
            var list = new List<ComponentWithId> { relationship1, relationship2 };
            await dbProxy.SaveBatch(list);
            await dbProxy.SaveLogBatch(uid, DBLog.LogType.Relationship, list);
            return relationship1;
        }

        /// <summary>
        /// 取消關係
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="targetUid"></param>
        /// <returns></returns>
        public static async ETTask RemoveRelationship(long uid, long targetUid)
        {
            Expression<Func<Relationship, bool>> exp = entity => (entity.uid == uid && entity.targetUid == targetUid) ||
                (entity.uid == targetUid && entity.targetUid == uid);
            List<ComponentWithId> componentWithIds = await dbProxy.Query(exp);
            if (componentWithIds.Count == 0)
            {
                Log.Error($"Relationship:{uid}<-->{targetUid} doesnt exist!");
                return;
            }
            Relationship relationship = (Relationship)componentWithIds[0];
            await dbProxy.DeleteJson(exp);
            await dbProxy.SaveLog(uid, DBLog.LogType.Relationship, relationship);
        }

        /// <summary>
        /// 用uids查詢非好友的清單
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="queringUids"></param>
        /// <returns></returns>
        public static async ETTask<RepeatedField<RelationshipSimpleInfo>> QueryByUids(long uid, params long[] queringUids)
        {
            // 先查詢自己的關係
            var relations = await GetUserRelationshipList(uid);
            var excludes = relations.Select(e => e.targetUid).ToList();

            // 排除自己
            excludes.Add(uid);

            // 排除已申請的玩家
            var relationApplyInfSenders = await GetRelationshipApplyInfoBySenderUid(uid);
            for (int i = 0; i < relationApplyInfSenders.Count; i++)
            {
                excludes.Add(relationApplyInfSenders[i].ReceiverUid);
            }

            // 排除向自己申請的玩家
            var relationApplyInfoReceivers = await GetRelationshipApplyInfoByReceiverUid(uid);
            for (int i = 0; i < relationApplyInfoReceivers.Count; i++)
            {
                excludes.Add(relationApplyInfoReceivers[i].SenderUid);
            }

            // 用差集過濾下
            var finds = queringUids.Except(excludes);

            List<User> users = await UserDataHelper.FindUsers(finds.ToArray());
            return users.Aggregate(new RepeatedField<RelationshipSimpleInfo>(), (list, user) =>
            {
                RelationshipSimpleInfo info = new RelationshipSimpleInfo();
                info.Name = user.name;
                info.Location = user.location;
                info.Mileage = user.playerRideTotalInfo.Mileage;
                info.Uid = user.Id;
                info.DisconnectTime = 0;
                info.RelationshipType = (int)Relationship.RelationType.Stranger;
                list.Add(info);
                return list;
            });
        }

        /// <summary>
        /// 用name做模糊查詢非好友的清單
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="queringUids"></param>
        /// <returns></returns>
        public static async ETTask<RepeatedField<RelationshipSimpleInfo>> QueryLikeName(long uid, string userName, int count = 100)
        {
            if (string.IsNullOrEmpty(userName))
            {
                return new RepeatedField<RelationshipSimpleInfo>();
            }

            // 先查詢自己的關係
            var relations = await GetUserRelationshipList(uid);
            var excludes = relations.Select(e => e.targetUid).ToList();

            // 排除自己
            excludes.Add(uid);

            // 排除已申請的玩家
            var relationApplyInfSenders = await GetRelationshipApplyInfoBySenderUid(uid);
            for (int i = 0; i < relationApplyInfSenders.Count; i++)
            {
                excludes.Add(relationApplyInfSenders[i].ReceiverUid);
            }

            // 排除向自己申請的玩家
            var relationApplyInfoReceivers = await GetRelationshipApplyInfoByReceiverUid(uid);
            for (int i = 0; i < relationApplyInfoReceivers.Count; i++)
            {
                excludes.Add(relationApplyInfoReceivers[i].SenderUid);
            }

            // (?i)表示忽略大小寫
            List<User> users = await UserDataHelper.FindUsers(e =>
                !excludes.Contains(e.Id) && Regex.IsMatch(e.name, $"(?i){userName}")
            , 0, count);

            return users.Aggregate(new RepeatedField<RelationshipSimpleInfo>(), (list, user) =>
            {
                RelationshipSimpleInfo info = new RelationshipSimpleInfo();
                info.Name = user.name;
                info.Location = user.location;
                info.Mileage = user.playerRideTotalInfo.Mileage;
                info.Uid = user.Id;
                info.DisconnectTime = 0;
                info.RelationshipType = (int)Relationship.RelationType.Stranger;
                list.Add(info);
                return list;
            });
        }
    }
}
