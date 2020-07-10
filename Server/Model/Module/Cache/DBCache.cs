using System;
using System.Collections.Generic;
using System.IO;
using StackExchange.Redis;
using System.Threading.Tasks;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;

namespace ETModel
{
    public class DBCache : CacheBase
    {
        private object targetObj;

        private readonly HashEntry nullEntry;

        public DBCache(Type type, IDatabase database) : base(type, database)
        {
            this.targetObj = Activator.CreateInstance(type);
            this.nullEntry = new HashEntry();
        }

        public override async Task<T> Create<T>(T entity)
        {
            T res = await CreateCache(entity);
            //if(res == null)
            //{
            //    throw new Exception($"Create entity failed! Entity with key = " +
            //        $"{entity.Id} had existed on collection {typeof(T).Name}");
            //}
            return res;
        }

        public override async Task<T> Update<T>(T entity)
        {
            T res = await UpdateCache(entity);
            //if(res == null)
            //{
            //    throw new Exception($"Update entity failed! Entity with key = " +
            //        $"{entity.Id} hadn't existed on collection {typeof(T).Name}");
            //}
            return res;
        }

        public override async Task<T> Upsert<T>(T entity)
        {
            T res = await UpdateCache(entity);
            if(res == null)
            {
                res = await CreateCache(entity);
                //if(res == null)
                //{
                //    throw new Exception($"Upsert entity failed! Entity with key = " +
                //        $"{entity.Id} had existed on collection {typeof(T).Name}");
                //}
            }
            return res;
        }

        public override async Task<bool> Delete<T>(long id)
        {
            string key = GetKey(id);
            T res = await FindOneCache<T>(key);
            if(res != null)
            {
                bool isSuc = await DeleteIndex(res);
                isSuc &= await database.KeyDeleteAsync(key);
                return isSuc;
            }
            else
            {
                return false;
                //throw new Exception($"Destroy entity failed! Entity with key = " +
                //    $"{id} hadn't existed on collection {typeof(T).Name}");
            }
        }

        public override async Task<T> FindOne<T>(long id, params string[] fields)
        {
            T res = await FindOneCache<T>(id);
            return res;
        }

        public override async Task<T> FindOneByUniqueIndex<T>(string indexName, T entity)
        {
            var info = GetCacheIndexAttribute(indexName);
            if (!info.isUnique)
            {
                throw new Exception("given index is not unique");
            }
            string key = GetIndexKey(indexName, entity);
            if (string.IsNullOrEmpty(key))
                return null;
            bool isExist = await database.KeyExistsAsync(key);
            if (isExist)
            {
                string val = await database.StringGetAsync(key);
                return await FindOne<T>(long.Parse(val));
            }
            else
            {
                return null;
            }
        }

        public override async Task<List<T>> FindAllByIndex<T>(string indexName, T entity)
        {
            string key = GetIndexKey(indexName, entity);
            if (string.IsNullOrEmpty(key))
                return null;
            bool isExist = await database.KeyExistsAsync(key);
            if (isExist)
            {
                IBatch batch = database.CreateBatch();
                RedisValue[] member = await database.SetMembersAsync(key);
                if(member.Length == 0)
                {
                    return new List<T>();
                }
                Task<HashEntry[]>[] tasks = new Task<HashEntry[]>[member.Length];
                List<T> returnList = new List<T>(member.Length);
                for (int i = 0; i < member.Length; i++)
                {
                    var m = member[i];
                    var k = GetKey((long)m);
                    tasks[i] = batch.HashGetAllAsync(k);
                }
                batch.Execute();
                var results = await Task.WhenAll(tasks);
                for (int i = 0; i < results.GetLength(0); i++)
                {
                    var result = results[i];
                    returnList[i] = Deserialize<T>(result);
                }
                return returnList;
            }
            else
            {
                return new List<T>();
            }
        }

        private async Task<T> FindOneCache<T>(long id) where T : ComponentWithId
        {
            string key = GetKey(id);
            return await FindOneCache<T>(key);
        }

        private async Task<T> FindOneCache<T>(string key) where T : ComponentWithId
        {
            bool isExist = await database.KeyExistsAsync(key);
            if (isExist)
            {
                var hashSet = await database.HashGetAllAsync(key);
                return Deserialize<T>(hashSet);
            }
            else
            {
                return null;
            }
        }

        private async Task<T> CreateCache<T>(T entity) where T : ComponentWithId
        {
            string key = GetKey(entity.Id);
            bool isExist = await database.KeyExistsAsync(key);
            if (isExist)
            {
                return null;
            }
            else
            {
                return await SetHashSet(key, entity);
            }
        }

        private async Task<T> UpdateCache<T>(T entity) where T : ComponentWithId
        {
            string key = GetKey(entity.Id);
            T res = await FindOneCache<T>(key);
            if(res != null)
            {
                await DeleteIndex(res);
                return await SetHashSet(key, entity);
            }
            else
            {
                return null;
            }
        }

        private async Task<T> SetHashSet<T>(string key, T entity) where T : ComponentWithId
        {
            HashEntry[] hashEntry = Serialize(entity);
            await database.HashSetAsync(key, hashEntry);
            await UpsertIndex(entity);
            return entity;
        }

        private async Task<bool> UpsertIndex<T>(T entity) where T : ComponentWithId
        {
            var batch = database.CreateBatch();
            Task<bool>[] tasks = new Task<bool>[indexNames.Length];
            for (int i = 0; i < indexNames.Length; i++)
            {
                var indexName = indexNames[i];
                var info = GetCacheIndexAttribute(indexName);
                var indexKey = GetIndexKey(indexName, entity);
                if (string.IsNullOrEmpty(indexKey))
                    continue;
                if (info.isUnique)
                {
                    tasks[i] = batch.StringSetAsync(indexKey, entity.Id);
                }
                else
                {
                    tasks[i] = batch.SetAddAsync(indexKey, entity.Id);
                }
            }
            batch.Execute();
            var res = await Task.WhenAll(tasks);
            return res.All(e => e);
        }

        private async Task<bool> DeleteIndex<T>(T entity) where T : ComponentWithId
        {
            var batch = database.CreateBatch();
            Task<bool>[] tasks = new Task<bool>[indexNames.Length];
            for (int i = 0; i < indexNames.Length; i++)
            {
                var indexName = indexNames[i];
                var info = GetCacheIndexAttribute(indexName);
                var indexKey = GetIndexKey(indexName, entity);
                if (string.IsNullOrEmpty(indexKey))
                    continue;
                if (info.isUnique)
                {
                    tasks[i] = batch.KeyDeleteAsync(indexKey);
                }
                else
                {
                    tasks[i] = batch.SetRemoveAsync(indexKey, entity.Id);
                }
            }
            batch.Execute();
            var res = await Task.WhenAll(tasks);
            return res.All(e => e);
        }

        private HashEntry[] Serialize<T>(T entity)
        {
            Type type = typeof(T);
            if (this.type != type)
            {
                throw new Exception($"Type: {type} doesn't match with {this.type}");
            }
            var names = propertyNames;
            HashEntry[] hashEntry = new HashEntry[names.Length];
            for (int i = 0; i < names.Length; i++)
            {
                var name = names[i];
                var pro = GetPropertyInfo(name);
                var val = pro?.GetValue(entity);
                hashEntry[i] = new HashEntry(name, val == null ? string.Empty : val.ToString());
            }
            return hashEntry;
        }

        private T Deserialize<T>(HashEntry[] entries)
        {
            var names = propertyNames;
            for (int i = 0; i < names.Length; i++)
            {
                var name = names[i];
                var entry = entries.FirstOrDefault(e => e.Name == name);
                if (entry.Equals(nullEntry))
                {
                    SetPropertyValue(name, null);
                }
                else
                {
                    var info = GetPropertyInfo(name);
                    var val = Convert.ChangeType(entry.Value.ToString(), info.PropertyType);
                    SetPropertyValue(name, val);
                }
            }
            return GetTargetObject<T>();
        }

        private void SetPropertyValue(string property, object val)
        {
            var info = GetPropertyInfo(property);
            info?.SetValue(targetObj, val);
        }

        private T GetTargetObject<T>()
        {
            using (var stream = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(stream, targetObj);
                stream.Seek(0, SeekOrigin.Begin);
                var result = (T)formatter.Deserialize(stream);
                return result;
            }
        }
    }
}
