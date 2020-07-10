using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using StackExchange.Redis;
using System.Reflection;
using MongoDB.Bson;

namespace ETModel
{
    public abstract class CacheBase
    {
        public string key => CacheHelper.key;

        public string mappingToAllIdKey => $"{collectionName}:all";

        public bool useRandomId { set; get; } = true;

        public Type type { private set; get; }

        protected readonly IDatabase database;

        public readonly string collectionName;

        protected readonly Dictionary<string, DBIndexAttribute> uniqueMap =
            new Dictionary<string, DBIndexAttribute>();

        protected readonly Dictionary<string, DBIndexAttribute> groupMap =
            new Dictionary<string, DBIndexAttribute>();

        protected readonly List<string> uniqueNames = new List<string>();

        protected readonly List<string> uniqueFields = new List<string>();

        protected readonly List<string> groupNames = new List<string>();

        protected readonly List<string> groupFields = new List<string>();

        public string[] propertyNames
        {
            get
            {
                return DBHelper.GetPropertyNames(type);
            }
        }

        public string[] indexNames
        {
            get
            {
                return DBHelper.GetIndexNames(type);
            }
        }

        public int propertyCount
        {
            get
            {
                return propertyNames.Length;
            }
        }

        public PropertyInfo GetPropertyInfo(string property)
        {
            return DBHelper.GetPropertyInfo(type, property);
        }

        public DBIndexAttribute GetCacheIndexAttribute(string indexName)
        {
            return DBHelper.GetCacheIndexAttribute(type, indexName);
        }

        public CacheBase(Type type, IDatabase database)
        {
            this.type = type;
            this.database = database;
            this.collectionName = this.type.Name.ToLower();

            foreach(var v in DBHelper.GetDBSchemaIndicesTypeMap(type))
            {
                if (v.Value.isUnique)
                {
                    uniqueMap.Add(v.Key, v.Value);
                    uniqueNames.Add(v.Key);
                    foreach(var k in v.Value.columnNames)
                    {
                        if (!uniqueFields.Contains(k))
                        {
                            uniqueFields.Add(k);
                        }
                    }
                }
                else
                {
                    groupMap.Add(v.Key, v.Value);
                    groupNames.Add(v.Key);
                    foreach (var k in v.Value.columnNames)
                    {
                        if (!groupFields.Contains(k))
                        {
                            groupFields.Add(k);
                        }
                    }
                }
            }
        }

        protected BsonDocument Deserialize(HashEntry[] entries)
        {
            return CacheHelper.Deserialize(type, entries);
        }

        protected List<HashEntry> Serialize(BsonDocument entity)
        {
            return CacheHelper.Serialize(type, entity);
        }

        protected string GetKey(long id)
        {
            return $"{collectionName}:{id}";
        }

        protected string GetIndexKey<T>(string indexName, T entity) where T : ComponentWithId
        {
            var indexAttr = GetCacheIndexAttribute(indexName);
            var columnNames = indexAttr.columnNames;
            if(columnNames.Length == 0)
            {
                return string.Empty;
            }
            string indexKey = collectionName;
            for (int i = 0; i < columnNames.Length; i++)
            {
                 var columnName = columnNames[i];
                 var propertyInfo = GetPropertyInfo(columnName);
                 var val = propertyInfo.GetValue(entity);
                 indexKey += $":{columnName}:{val.ToString()}";
            }
            return indexKey;
        }

        protected async Task<bool> ExistId(long id)
        {
            return await database.KeyExistsAsync(GetKey(id));
        }

        protected async Task<bool> ExistKey(string key)
        {
            return await database.KeyExistsAsync(key);
        }

        protected async Task<bool> ExistGroup(string groupName, BsonDocument data)
        {
            string groupKey = GetGroupKey(groupName, data);
            return await database.KeyExistsAsync(groupKey);
        }

        protected async Task<bool> ExistIndex(string indexName, BsonDocument data)
        {
            return await database.KeyExistsAsync(GetUniqueKey(indexName, data));
        }

        protected string GetUniqueKey(string uniqueName, BsonDocument data, BsonDocument extraData = null)
        {
            uniqueMap.TryGetValue(uniqueName, out var unique);
            if (unique == null)
                return string.Empty;

            string key = $"{collectionName}:{uniqueName}";
            for (int i = 0; i < unique.columnNames.Length; i++)
            {
                var col = unique.columnNames[i];
                //如果沒找到所有的Key則返回
                var v = data.Contains(col) ? data[col] : null;

                //額外讀取extraValue
                if (extraData != null && v == null)
                {
                    v = extraData.Contains(col) ? extraData[col] : null;
                }

                if (v == null)
                    return string.Empty;
                key = string.Concat(key, $":{v}");
            }

            return key;
        }

        protected string GetGroupKey(string groupName, BsonDocument data, BsonDocument extraData = null)
        {
            groupMap.TryGetValue(groupName, out var group);
            if (group == null)
                return string.Empty;

            string key = $"{collectionName}:{groupName}";
            for (int i = 0; i < group.columnNames.Length; i++)
            {
                var col = group.columnNames[i];
                var v = data.Contains(col) ? data[col] : null;

                //額外讀取extraValue
                if (extraData != null && v == null)
                {
                    v = extraData.Contains(col) ? extraData[col] : null;
                }

                if (v == null)
                    return string.Empty;
                key = string.Concat(key, $":{v}");
            }

            return key;
        }

        protected virtual async void _AddGroup(IBatch pipeline, List<Task> tasks, string groupKey, long id)
        {
            /************DBCache************/

            //TODO對DB做快取
            //bool existKey = await ExistKey(groupKey);
            //if (existKey)
            //{
            //    tasks.Add(pipeline.SetAddAsync(groupKey, id));
            //}

            /************DBCache-End************/

            /************RedisCache************/

            //直接做快取
            await Task.CompletedTask;
            tasks.Add(pipeline.SetAddAsync(groupKey, id));

            /************RedisCache-End************/
        }

        //TODO:砍掉
        /****old****/

        public virtual Task<T> FindOne<T>(long id, string[] fields) where T : ComponentWithId { return default; }
        public virtual Task<T> FindOneByUniqueIndex<T>(string indexName, T entity) where T : ComponentWithId { return default; }
        public virtual Task<List<T>> FindAllByIndex<T>(string indexName, T entity) where T : ComponentWithId { return default; }
        public virtual Task<T> Update<T>(T entity) where T : ComponentWithId { return default; }
        public virtual Task<T> Upsert<T>(T entity) where T : ComponentWithId { return default; }
        public virtual Task<bool> Delete<T>(long id) where T : ComponentWithId { return default; }

        /****new****/

        public virtual Task<T> FindOne<T>(long id) where T : ComponentWithId { return default; }
        public virtual Task<ComponentWithId> FindOne(Type type, long id, string[] fields = null) { return default; }
        public virtual Task<BsonDocument> FindOne(long id, string[] fields = null) { return default; }
        public virtual Task<BsonDocument> Create(BsonDocument entity) { return default; }
        public virtual Task<T> Create<T>(T entity) where T : ComponentWithId { return default; }
        public virtual Task<bool> Delete(long id) { return default; }
        public virtual Task<bool> DeleteByUnique(string uniqueName, BsonDocument condition) { return default; }
        public virtual Task<BsonDocument> Update(long id, BsonDocument entity) { return default; }
        public virtual Task<T> Update<T>(long id, BsonDocument entity) where T : ComponentWithId { return default; }
        public virtual Task<ComponentWithId> Update(Type type, long id, BsonDocument entity) { return default; }
        public virtual Task<List<BsonDocument>> FindAllByGroup(string groupName, BsonDocument condition, string[] fields = null) { return default; }
        public virtual Task<long> CountByGroup(string groupName, BsonDocument condition) { return default; }
        public virtual Task<BsonDocument> FindOneByUnique(string uniqueName, BsonDocument condition, string[] fields = null) { return default; }
        public virtual Task<T> FindOneByUnique<T>(string uniqueName, BsonDocument condition) where T : ComponentWithId { return default; }
        public virtual Task<ComponentWithId> FindOneByUnique(Type type, string uniqueName, BsonDocument condition) { return default; }
        public virtual Task<BsonDocument> FindOrCreateByIndex(string indexName, BsonDocument condition, BsonDocument defaultData, string[] fields = null) { return default; }
        public virtual Task<List<long>> GetAllIds() { return default; }
        public virtual Task<List<long>> GetRandomIds(int count) { return default; }
    }
}
