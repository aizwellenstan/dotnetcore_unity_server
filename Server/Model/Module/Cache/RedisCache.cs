using System;
using System.Collections.Generic;
using System.IO;
using StackExchange.Redis;
using System.Threading.Tasks;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using MongoDB.Bson;
using System.Reflection;
using MongoDB.Bson.Serialization;

namespace ETModel
{
    public class RedisCache : CacheBase
    {
        public RedisCache(Type type, IDatabase database) : base(type, database)
        {

        }

        protected async Task<BsonDocument> _FindOneInCache(long id, string[] fields = null)
        {
            if (fields == null)
            {
                var data = await database.HashGetAllAsync(GetKey(id));

                if(data.Length == 0)
                {
                    return default;
                }
                else
                {
                    var doc = Deserialize(data);
                    doc[key] = id;
                    return doc;
                }
            }
            else
            {
                var data = await database.HashGetAsync(GetKey(id), fields.Select(e => (RedisValue)e).ToArray());
                if(data.Length == 0)
                {
                    return default;
                }

                HashEntry[] hashEntries = new HashEntry[fields.Length];
                for (int i = 0; i < fields.Length; i++)
                {
                    var name = fields[i];
                    if(name == key)
                    {
                        hashEntries[i] = new HashEntry(key, id);
                    }
                    else
                    {
                        hashEntries[i] = new HashEntry(name, data[i]);
                    }
                }
                return Deserialize(hashEntries);
            }
        }

        protected bool _IsGroup(BsonDocument entity, bool disallowNull = false)
        {
            if (groupMap.Count == 0)
                return false;

            for (int i = 0; i < groupFields.Count; i++)
            {
                var field = groupFields[i];
                if (!entity.Contains(field))
                {
                    continue;
                }

                if (disallowNull)
                {
                    if (entity[field] != null)
                        return true;
                }
                else
                {
                    return true;
                }
            }
            return false;
        }

        protected bool _IsUnique(BsonDocument entity)
        {
            if (uniqueMap.Count == 0)
                return false;

            for (int i = 0; i < uniqueFields.Count; i++)
            {
                var field = uniqueFields[i];
                if(entity.Contains(field))
                {
                    return true;
                }
            }
            return false;
        }

        protected async Task<BsonDocument> _WriteHashSet(long id, BsonDocument entity, bool initGroupAndIndex, IBatch pipeline, List<Task> tasks, bool notExecute = false)
        {
            List<HashEntry> updatedList = Serialize(entity);

            if(pipeline == null)
            {
                pipeline = database.CreateBatch();
            }

            if (initGroupAndIndex)
            {
                // 建立複合群組
                if (groupNames.Count != 0)
                {
                    for(int i = 0; i < groupNames.Count; i++)
                    {
                        string groupName = groupNames[i];
                        string redisKey = GetGroupKey(groupName, entity);
                        if (string.IsNullOrEmpty(redisKey))
                        {
                            continue;
                        }
                        tasks.Add(pipeline.SetAddAsync(redisKey, id));
                    }
                }

                // 建立複合唯一索引
                if (uniqueNames.Count != 0)
                {
                    for (int i = 0; i < uniqueNames.Count; i++)
                    {
                        string uniqueName = uniqueNames[i];
                        string uniqueKey = GetUniqueKey(uniqueName, entity);
                        if (string.IsNullOrEmpty(uniqueKey))
                        {
                            continue;
                        }

                        /************DBCache************/

                        //tasks.Add(pipeline.StringSetAsync(uniqueKey, id));

                        /************DBCache-End************/

                        /************RedisCache************/

                        bool exist = await database.KeyExistsAsync(uniqueKey);
                        if (exist)
                        {
                            Log.Error($"Collection[{collectionName}] with key[{uniqueName}]: {uniqueKey} is repeatly");
                            return null;
                        }
                        tasks.Add(pipeline.StringSetAsync(uniqueKey, id));

                        /************RedisCache-End************/
                    }
                }

                // 建立隨機IDSet
                if (useRandomId)
                {
                    tasks.Add(pipeline.SetAddAsync(mappingToAllIdKey, id));
                }
            }

            tasks.Add(pipeline.HashSetAsync(GetKey(id), updatedList.ToArray()));

            if (notExecute)
                return null;
            // 呼叫批次更新指令
            pipeline.Execute();
            await Task.WhenAll(tasks.ToArray());
            return entity;
        }

        protected async Task<BsonDocument> _CreateInCache(long id, BsonDocument entity)
        {
            bool existHash = await database.KeyExistsAsync(GetKey(id));
            if (existHash) return null;

            const string newIndexKey = "newIndexKey";
            List<BsonDocument> groupAddArray = new List<BsonDocument>();

            // 判斷修改的項目有沒有牽涉到複合群組
            if (_IsGroup(entity))
            {
                // 把要修改的群組增加到修改陣列
                for (int i = 0; i < groupNames.Count; i++)
                {
                    BsonDocument doc = new BsonDocument();
                    var groupName = groupNames[i];
                    var groupKey = GetGroupKey(groupName, entity);
                    if (!string.IsNullOrEmpty(groupKey))
                    {
                        doc[newIndexKey] = groupKey;
                        groupAddArray.Add(doc);
                    }
                }
            }

            IBatch batch = database.CreateBatch();
            List<Task> tasks = new List<Task>();

            // 更新複合唯一索引
            if (_IsUnique(entity))
            {
                for (int i = 0; i < uniqueNames.Count; i++)
                {
                    var uniqueName = uniqueNames[i];
                    var uniqueKey = GetUniqueKey(uniqueName, entity);
                    if (!string.IsNullOrEmpty(uniqueKey))
                    {
                        /************DBCache************/

                        //tasks.Add(batch.StringSetAsync(uniqueKey, id));

                        /************DBCache-End************/

                        /************RedisCache************/

                        bool exist = await database.KeyExistsAsync(uniqueKey);
                        if (exist)
                        {
                            Log.Error($"Collection[{collectionName}] with key[{uniqueName}]: {uniqueKey} is repeatly");
                            return null;
                        }
                        tasks.Add(batch.StringSetAsync(uniqueKey, id));

                        /************RedisCache-End************/
                    }
                }
            }

            // 建立隨機IDSet
            if (useRandomId)
            {
                tasks.Add(batch.SetAddAsync(mappingToAllIdKey, id));
            }

            // 蒐集全部要更新的群組
            for(int i = 0; i < groupAddArray.Count; i++)
            {
                BsonDocument doc = groupAddArray[i];
                _AddGroup(batch, tasks, doc[newIndexKey].AsString, id);
            }
            return await _WriteHashSet(id, entity, false, batch, tasks);
        }

        protected async Task _DeleteInCache(long id, BsonDocument entity)
        {
            List<string> groupDeleteArray = new List<string>();

            // 把要修改的群組增加到修改陣列

            if (groupNames.Count != 0)
            {
                for(int i = 0; i < groupNames.Count; i++)
                {
                    var groupName = groupNames[i];
                    var redisKey = GetGroupKey(groupName, entity);
                    if (!string.IsNullOrEmpty(redisKey))
                    {
                        groupDeleteArray.Add(redisKey);
                    }
                }
            }

            var pipeline = database.CreateBatch();
            var tasks = new List<Task>();

            if (uniqueNames.Count != 0)
            {
                // 更新複合索引
                for (int i = 0; i < uniqueNames.Count; i++)
                {
                    var indexName = uniqueNames[i];
                    var redisKey = GetUniqueKey(indexName, entity);
                    if (!string.IsNullOrEmpty(redisKey))
                    {
                        tasks.Add(pipeline.KeyDeleteAsync(redisKey));
                    }
                }
            }

            // 移除隨機IDSet
            if (useRandomId)
            {
                tasks.Add(pipeline.SetRemoveAsync(mappingToAllIdKey, id));
            }

            // 移除物件
            tasks.Add(pipeline.KeyDeleteAsync(GetKey(id)));

            for(int i = 0; i < groupDeleteArray.Count; i++)
            {
                var groupKey = groupDeleteArray[i];
                _RemoveGroup(pipeline, tasks, groupKey, id);
            }

            pipeline.Execute();
            await Task.WhenAll(tasks);
        }

        protected async Task _UpdateGroup(IBatch pipeline, List<Task> tasks, string oldGroupKey, string newGroupKey, long id)
        {
            var results = await Task.WhenAll(new Task<bool>[] { ExistKey(oldGroupKey), ExistKey(newGroupKey) });

            // 若舊的存在則移除舊的
            if (results[0])
            {
                tasks.Add(pipeline.SetRemoveAsync(oldGroupKey, id));
            }

            // 若新的存在則加入,不存在可能為載入不可以直接加入
            if (results[1])
            {
                tasks.Add(pipeline.SetAddAsync(newGroupKey, id));
            }
        }

        protected async Task<BsonDocument> _UpdateCache(long id, BsonDocument newDoc)
        {
            const string oldGroupKey = "oldGroupKey";
            const string newGroupKey = "newGroupKey";

            // 必須先取得舊的資料重新計算Group和MultiGroup
            var oldDoc = await _FindOneInCache(id);
            if(oldDoc == null)
            {
                return null;
            }

            List<BsonDocument> updatedGroupList = new List<BsonDocument>();

            if (_IsGroup(newDoc))
            {
                for(int i = 0; i < groupNames.Count; i++)
                {
                    var groupName = groupNames[i];
                    var columnName = groupMap[groupName].columnNames;
                    bool dirty = false;
                    for (int j = 0; j < columnName.Length; j++)
                    {
                        string key = columnName[j];
                        if (!newDoc.Contains(key) || oldDoc[key] == newDoc[key])
                            continue;
                        dirty = true;
                    }

                    if (dirty)
                    {
                        // 把要修改的群組增加到修改陣列
                        BsonDocument groupUpdate = new BsonDocument();
                        groupUpdate[oldGroupKey] = GetGroupKey(groupName, oldDoc);
                        groupUpdate[newGroupKey] = GetGroupKey(groupName, newDoc, oldDoc);
                        updatedGroupList.Add(groupUpdate);
                    }
                }
            }

            var pipeline = database.CreateBatch();
            var tasks = new List<Task>();

            // 更新複合索引
            if (_IsUnique(newDoc))
            {
                for (int i = 0; i < uniqueNames.Count; i++)
                {
                    var uniqueName = uniqueNames[i];
                    var columnName = uniqueMap[uniqueName].columnNames;
                    bool dirty = false;
                    for (int j = 0; j < columnName.Length; j++)
                    {
                        string key = columnName[j];
                        if (!newDoc.Contains(key) || oldDoc[key] == newDoc[key])
                            continue;
                        dirty = true;
                    }

                    if (dirty)
                    {
                        string oldUniqueKey = GetUniqueKey(uniqueName, oldDoc);
                        string newUniqueKey = GetUniqueKey(uniqueName, newDoc, oldDoc);
                        if (!string.IsNullOrEmpty(oldUniqueKey))
                        {
                            tasks.Add(pipeline.KeyDeleteAsync(oldUniqueKey));
                        }
                        if (!string.IsNullOrEmpty(newUniqueKey))
                        {
                            /************DBCache************/

                            //tasks.Add(pipeline.StringSetAsync(newUniqueKey, id));

                            /************DBCache-End************/

                            /************RedisCache************/

                            bool exist = await database.KeyExistsAsync(newUniqueKey);
                            if (exist)
                            {
                                Log.Error($"Collection[{collectionName}] with key[{uniqueName}]: {newUniqueKey} is repeatly");
                                return null;
                            }
                            tasks.Add(pipeline.StringSetAsync(newUniqueKey, id));

                            /************RedisCache-End************/
                        }
                    }
                }
            }

            // TODO 優化判斷oldDoc & newDoc的差別
            // 蒐集全部要更新的群組
            for (int i = 0; i < updatedGroupList.Count; i++)
            {
                var doc = updatedGroupList[i];
                await _UpdateGroup(pipeline, tasks, doc[oldGroupKey].AsString, doc[newGroupKey].AsString, id);
            }

            return await _WriteHashSet(id, newDoc, false, pipeline, tasks);
        }

        protected async void _RemoveGroup(IBatch pipeline, List<Task> tasks, string groupKey, long id)
        {
            var existKey = await ExistKey(groupKey);
            if (existKey)
            {
                tasks.Add(pipeline.SetRemoveAsync(groupKey, id));
            }
        }

        protected async Task<bool> _ExistAll(long[] idArray)
        {
            if (idArray == null || idArray.Length == 0)
                return false;

            RedisKey[] keyArray = idArray.Select((id) => (RedisKey)GetKey(id)).ToArray();
            long match = await database.KeyExistsAsync(keyArray);
            return match == keyArray.Length;
        }

        protected async Task<List<BsonDocument>> _FindAllInCache(long[] idArray, string[] fields = null)
        {
            List<BsonDocument> retData = new List<BsonDocument>();
            List<Task<HashEntry[]>> tasks = new List<Task<HashEntry[]>>();
            // 組合pipeline
            var batch = idArray.Aggregate(database.CreateBatch(), (pipeline, id) => 
            {
                if (fields == null)
                {
                    async Task<HashEntry[]> task()
                    {
                        var entries = await pipeline.HashGetAllAsync(GetKey(id));
                        return entries;
                    }
                    tasks.Add(task());
                }
                else
                {
                    async Task<HashEntry[]> task()
                    {
                        var entries = await pipeline.HashGetAsync(GetKey(id), fields.Select(e => (RedisValue)e).ToArray());
                        int counter = 0;
                        return entries.Select(e => 
                        {
                            return new HashEntry(fields[counter++], e);
                        }).ToArray();
                    }
                    tasks.Add(task());
                }
                return pipeline;
            });

            // 執行pipeline
            batch.Execute();
            var results = await Task.WhenAll(tasks);

            for(int i = 0; i < results.GetLength(0); i++)
            {
                var result = results[i];
                if (result.Length == 0)
                {
                    retData.Add(null);
                }
                else
                {
                    var doc = Deserialize(result);
                    retData.Add(doc);
                }
            }
            return retData;
        }


        protected async Task<List<BsonDocument>> FindAllByIdArray(long[] idArray, string[] fields = null)
        {
            if (idArray == null || idArray.Length == 0)
                return new List<BsonDocument>();

            // 判斷全部存在則批次抓取
            bool existAll = await _ExistAll(idArray);
            if (existAll)
            {
                return await _FindAllInCache(idArray, fields);
            }
            else
            {
                var documents = await Task.WhenAll(idArray.Select(id => FindOne(id, fields)));
                return documents.ToList();
            }
        }

        protected async Task<List<BsonDocument>> _FindAllByGroupInCache(string groupName, BsonDocument condition, string[] fields = null)
        {
            string groupKey = GetGroupKey(groupName, condition);

            var results = await database.SetMembersAsync(groupKey);
            if(results.Length == 0)
            {
                return new List<BsonDocument>();
            }
            return await FindAllByIdArray(results.Select(e => (long)e).ToArray(), fields);
        }

        public override async Task<T> FindOne<T>(long id)
        {
            var doc = await FindOne(id, null);
            if(doc != default)
            {
                return BsonSerializer.Deserialize<T>(doc);
            }
            return default;
        }

        public override async Task<ComponentWithId> FindOne(Type type, long id, string[] fields = null)
        {
            var doc = await FindOne(id, fields);
            if (doc != default)
            {
                return (ComponentWithId)BsonSerializer.Deserialize(doc, type);
            }
            return default;
        }

        public override async Task<BsonDocument> FindOne(long id, string[] fields = null)
        {
            bool exist = await this.ExistId(id);

            if (exist)
            {
                return await _FindOneInCache(id, fields);
            }
            else
            {
                return default;
            }
        }

        public override async Task<BsonDocument> Create(BsonDocument entity)
        {
            bool exist = await this.ExistId(entity[key].AsInt64);
            if (exist)
            {
                return null;
            }
            else
            {
                return await _CreateInCache(entity[key].AsInt64, entity);
            }
        }

        public override async Task<T> Create<T>(T entity)
        {
            bool exist = await this.ExistId(entity.Id);
            if (exist)
            {
                return null;
            }
            else
            {
                var result = await _CreateInCache(entity.Id, entity.ToBsonDocument());
                return result != null ? entity : null;
            }
        }

        public override async Task<bool> Delete(long id)
        {
            var doc = await _FindOneInCache(id);
            if (doc != null)
            {
                await _DeleteInCache(id, doc);
                return true;
            }
            else
            {
                return false;
            }
        }

        public override async Task<bool> DeleteByUnique(string uniqueName, BsonDocument condition)
        {
            var doc = await FindOneByUnique(uniqueName, condition);
            if (doc != null)
            {
                await _DeleteInCache(doc[key].AsInt64, doc);
                return true;
            }
            else
            {
                return false;
            }
        }

        public override async Task<BsonDocument> Update(long id, BsonDocument updatedData)
        {
            var doc = await FindOne(id);

            // redis找不到資料
            if (doc == null)
            {
                return null;
            }

            // 複製要更新的資料
            for (int i = 0; i < updatedData.ElementCount; i++)
            {
                var val = updatedData.GetElement(i);
                doc[val.Name] = val.Value;
            }

            await _UpdateCache(id, updatedData);

            return doc;
        }

        public override async Task<T> Update<T>(long id, BsonDocument updatedData)
        {
            var doc = await Update(id, updatedData);
            return doc != null ? BsonSerializer.Deserialize<T>(doc) : default;
        }

        public override async Task<ComponentWithId> Update(Type type, long id, BsonDocument updatedData)
        {
            var doc = await Update(id, updatedData);
            return doc != null ? (ComponentWithId)BsonSerializer.Deserialize(doc, type) : null;
        }

        public override async Task<List<BsonDocument>> FindAllByGroup(string groupName, BsonDocument condition, string[] fields = null)
        {
            if(!_IsGroup(condition, true))
            {
                return new List<BsonDocument>();
            }
            bool exist = await ExistGroup(groupName, condition);
            if (exist)
            {
                return await _FindAllByGroupInCache(groupName, condition, fields);
            }
            else
            {
                return new List<BsonDocument>();
            }
        }

        public override async Task<long> CountByGroup(string groupName, BsonDocument condition)
        {
            bool exist = await ExistGroup(groupName, condition);
            if (exist)
            {
                return await database.SetLengthAsync(GetGroupKey(groupName, condition));
            }
            else
            {
                return 0L;
            }
        }

        public override async Task<BsonDocument> FindOneByUnique(string uniqueName, BsonDocument condition, string[] fields = null)
        {
            var uniqueKey = GetUniqueKey(uniqueName, condition);
            if (string.IsNullOrEmpty(uniqueKey))
            {
                return null;
            }

            var id = await database.StringGetAsync(uniqueKey);
            if(id.HasValue)
            {
                return await FindOne((long)id, fields);
            }
            else
            {
                return null;
            }
        }

        public override async Task<T> FindOneByUnique<T>(string uniqueName, BsonDocument condition)
        {
            var doc = await FindOneByUnique(uniqueName, condition);
            if(doc == null)
            {
                return default;
            }
            else
            {
                return BsonSerializer.Deserialize<T>(doc);
            }
        }

        public override async Task<ComponentWithId> FindOneByUnique(Type type, string uniqueName, BsonDocument condition)
        {
            var doc = await FindOneByUnique(uniqueName, condition);
            if (doc == null)
            {
                return default;
            }
            else
            {
                return (ComponentWithId)BsonSerializer.Deserialize(doc, type);
            }
        }

        public override async Task<BsonDocument> FindOrCreateByIndex(string indexName, BsonDocument condition, BsonDocument defaultEntity, string[] fields = null)
        {
            var uniqueKey = GetUniqueKey(indexName, condition);
            if (string.IsNullOrEmpty(uniqueKey))
            {
                return null;
            }

            var id = await database.StringGetAsync(uniqueKey);
            if (id.HasValue)
            {
                return await FindOne((long)id, fields);
            }
            else
            {
                return await _CreateInCache(defaultEntity[key].AsInt64, defaultEntity);
            }
        }

        public override async Task<List<long>> GetAllIds()
        {
            if (useRandomId)
            {
                RedisValue[] results = await database.SetMembersAsync(mappingToAllIdKey);
                return results.Select(e => (long)e).ToList();
            }
            else
            {
                return new List<long>(0);
            }
        }

        public override async Task<List<long>> GetRandomIds(int count)
        {
            count = Math.Max(count, 1);
            if (useRandomId)
            {
                RedisValue[] results = await database.SetRandomMembersAsync(mappingToAllIdKey, count);
                return results.Select(e => (long)e).ToList();
            }
            else
            {
                return new List<long>(0);
            }
        }
    }
}
