using System;
using System.Collections.Generic;
using System.Linq;
using StackExchange.Redis;
using MongoDB.Bson;

namespace ETModel
{
    [ObjectSystem]
    public class CacheComponentAwakeSystem : AwakeSystem<CacheComponent>
    {
        public override void Awake(CacheComponent self)
        {
            self.Awake();
        }
    }

    [ObjectSystem]
    public class CacheComponentDestroySystem : DestroySystem<CacheComponent>
    {
        public override void Destroy(CacheComponent self)
        {
            self.Destroy();
        }
    }

    public class CacheComponent : Component
    {
        public const string connectionString = "127.0.0.1:6379";

        public ConnectionMultiplexer redisClient;

        public IDatabase database { private set; get; }

        public const int taskCount = 32;

        public readonly List<DBTaskQueue> tasks = new List<DBTaskQueue>(taskCount);

        private readonly Dictionary<Type, CacheBase> caches =
            new Dictionary<Type, CacheBase>();

        private readonly Dictionary<string, Type> collectionNameToType =
            new Dictionary<string, Type>();

        public void Awake()
        {
            //DBConfig config = StartConfigComponent.Instance.StartConfig.GetComponent<DBConfig>();
            //string connectionString = config.ConnectionString;
            StartConfig dbStartConfig = StartConfigComponent.Instance.DBConfig;
            CacheConfig cacheConfig = dbStartConfig.GetComponent<CacheConfig>();
            if(cacheConfig == null)
            {
                redisClient = ConnectionMultiplexer.Connect(connectionString);
            }
            else
            {
                redisClient = ConnectionMultiplexer.Connect(cacheConfig.ConnectionString);
            }
            database = redisClient.GetDatabase();

            CollectCacheAttribute();

            ClearAllKey();

            for (int i = 0; i < taskCount; ++i)
            {
                DBTaskQueue taskQueue = ComponentFactory.Create<DBTaskQueue>();
                this.tasks.Add(taskQueue);
            }
        }

        public void Destroy()
        {
            redisClient.Close();
        }

        public ETTask<T> FindOne<T>(long id) where T : ComponentWithId
        {
            var type = typeof(T);
            CacheBase cache = null;
            caches.TryGetValue(type, out cache);
            ETTaskCompletionSource<T> tcs = new ETTaskCompletionSource<T>();
            CacheFindOneTask<T> task = ComponentFactory.CreateWithId<CacheFindOneTask<T>, CacheBase, ETTaskCompletionSource<T>>(id, cache, tcs);
            this.tasks[(int)((ulong)task.Id % taskCount)].Add(task);
            return tcs.Task;
        }

        public ETTask<T> FindOneByUniqueIndex<T>(string indexName, T entity) where T : ComponentWithId
        {
            var type = typeof(T);
            CacheBase cache = null;
            caches.TryGetValue(type, out cache);
            ETTaskCompletionSource<T> tcs = new ETTaskCompletionSource<T>();
            CacheFindOneByUniqueIndexTask<T> task = ComponentFactory.Create<CacheFindOneByUniqueIndexTask<T>, CacheBase, Tuple<string, T>, ETTaskCompletionSource<T>>
                (cache, new Tuple<string, T>(indexName, entity), tcs);
            this.tasks[(int)((ulong)task.Id % taskCount)].Add(task);
            return tcs.Task;
        }

        public ETTask<List<T>> FindAllByIndex<T>(string indexName, T entity) where T : ComponentWithId
        {
            var type = typeof(T);
            CacheBase cache = null;
            caches.TryGetValue(type, out cache);
            ETTaskCompletionSource<List<T>> tcs = new ETTaskCompletionSource<List<T>>();
            CacheFindAllByIndexTask<T> task = ComponentFactory.Create<CacheFindAllByIndexTask<T>, CacheBase, Tuple<string, T>, ETTaskCompletionSource<List<T>>>
                (cache, new Tuple<string, T>(indexName, entity), tcs);
            this.tasks[(int)((ulong)task.Id % taskCount)].Add(task);
            return tcs.Task;
        }

        public ETTask<T> Create<T>(T entity) where T : ComponentWithId
        {
            var type = typeof(T);
            CacheBase cache = null;
            caches.TryGetValue(type, out cache);
            ETTaskCompletionSource<T> tcs = new ETTaskCompletionSource<T>();
            CacheCreateTask<T> task = ComponentFactory.Create<CacheCreateTask<T>, CacheBase, T, ETTaskCompletionSource<T>>(cache, entity, tcs);
            this.tasks[(int)((ulong)task.Id % taskCount)].Add(task);
            return tcs.Task;
        }

        public ETTask<T> Update<T>(T entity) where T : ComponentWithId
        {
            var type = typeof(T);
            CacheBase cache = null;
            caches.TryGetValue(type, out cache);
            ETTaskCompletionSource<T> tcs = new ETTaskCompletionSource<T>();
            CacheUpdateTask<T> task = ComponentFactory.Create<CacheUpdateTask<T>, CacheBase, T, ETTaskCompletionSource<T>>(cache, entity, tcs);
            this.tasks[(int)((ulong)task.Id % taskCount)].Add(task);
            return tcs.Task;
        }

        public ETTask<T> Upsert<T>(T entity) where T : ComponentWithId
        {
            var type = typeof(T);
            CacheBase cache = null;
            caches.TryGetValue(type, out cache);
            ETTaskCompletionSource<T> tcs = new ETTaskCompletionSource<T>();
            CacheUpsertTask<T> task = ComponentFactory.Create<CacheUpsertTask<T>, CacheBase, T, ETTaskCompletionSource<T>>(cache, entity, tcs);
            this.tasks[(int)((ulong)task.Id % taskCount)].Add(task);
            return tcs.Task;
        }

        public ETTask<bool> Delete<T>(long id) where T : ComponentWithId
        {
            var type = typeof(T);
            CacheBase cache = null;
            caches.TryGetValue(type, out cache);
            ETTaskCompletionSource<bool> tcs = new ETTaskCompletionSource<bool>();
            CacheDeleteTask<T> task = ComponentFactory.CreateWithId<CacheDeleteTask<T>, CacheBase, ETTaskCompletionSource<bool>>
                (id, cache, tcs);
            this.tasks[(int)((ulong)task.Id % taskCount)].Add(task);
            return tcs.Task;
        }

        public string GetCollectionName(Type type)
        {
            CacheBase cache = null;
            caches.TryGetValue(type, out cache);
            return cache?.collectionName;
        }

        public CacheBase GetCache(string collectionName)
        {
            collectionNameToType.TryGetValue(collectionName, out var type);
            if(type == null)
            {
                return null;
            }
            caches.TryGetValue(type, out var cache);
            return cache;
        } 

        /// <summary>
        /// 蒐集快取屬性
        /// </summary>
        private void CollectCacheAttribute()
        {
            var asm = typeof(Game).Assembly;
            foreach (Type type in asm.GetTypes())
            {
                collectionNameToType.TryAdd(type.Name.ToLower(), type);

                var flag = DBHelper.GetCacheUseMode(type);

                switch (flag)
                {
                    case CacheUseMode.DBCache:
                        break;
                    case CacheUseMode.MemorySyncByDB:
                        break;
                    case CacheUseMode.MemorySyncByRedis:
                        caches.Add(type, new RedisCache(type, database));
                        break;
                    case CacheUseMode.RedisCacheOnly:
                        caches.Add(type, new RedisCache(type, database));
                        break;
                }
            }
        }

        /// <summary>
        /// 清除全部跟RedisCache相關的Key
        /// </summary>
        public void ClearAllKey()
        {
            var endPoints = redisClient.GetEndPoints();
            foreach(var endPoint in endPoints)
            {
                var server = redisClient.GetServer(endPoint);
                if (server != null)
                {
                    foreach (var cache in caches)
                    {
                        foreach (var key in server.Keys(pattern: $"{cache.Value.collectionName}:*"))
                        {
                            database.KeyDelete(key);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 用主鍵查詢
        /// </summary>
        /// <param name="collectionName"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public ETTask<ComponentWithId> QueryById(string collectionName, long id, List<string> fields = null)
        {
            ETTaskCompletionSource<ComponentWithId> tcs = new ETTaskCompletionSource<ComponentWithId>();
            CacheQueryByIdTask cacheQueryByIdTask = ComponentFactory.CreateWithId<CacheQueryByIdTask, string, List<string>, ETTaskCompletionSource<ComponentWithId>>(id, collectionName, fields, tcs);
            this.tasks[(int)((ulong)id % taskCount)].Add(cacheQueryByIdTask);

            return tcs.Task;
        }

        /// <summary>
        /// 用複合唯一索引查詢
        /// </summary>
        /// <param name="collectionName"></param>
        /// <param name="uniqueName"></param>
        /// <param name="json"></param>
        /// <returns></returns>
        public ETTask<ComponentWithId> QueryByUnique(string collectionName, string uniqueName, string json)
        {
            ETTaskCompletionSource<ComponentWithId> tcs = new ETTaskCompletionSource<ComponentWithId>();
            CacheQueryByUniqueTask cacheQueryByUniqueTask = ComponentFactory.Create<CacheQueryByUniqueTask, string, ETTaskCompletionSource<ComponentWithId>>(collectionName, tcs);
            cacheQueryByUniqueTask.UniqueName = uniqueName;
            cacheQueryByUniqueTask.Json = json;
            this.tasks[(int)((ulong)cacheQueryByUniqueTask.Id % taskCount)].Add(cacheQueryByUniqueTask);

            return tcs.Task;
        }

        /// <summary>
        /// 創造
        /// </summary>
        /// <param name="collectionName"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        public ETTask<ComponentWithId> Create(string collectionName, ComponentWithId entity)
        {
            ETTaskCompletionSource<ComponentWithId> tcs = new ETTaskCompletionSource<ComponentWithId>();
            CacheCreateTask cacheCreateTask = ComponentFactory.Create<CacheCreateTask, string, ComponentWithId, ETTaskCompletionSource<ComponentWithId>>(collectionName, entity, tcs);
            this.tasks[(int)((ulong)cacheCreateTask.Id % taskCount)].Add(cacheCreateTask);

            return tcs.Task;
        }

        /// <summary>
        /// 更新
        /// </summary>
        /// <param name="collectionName"></param>
        /// <param name="id"></param>
        /// <param name="jsonData"></param>
        /// <returns></returns>
        public ETTask<ComponentWithId> UpdateById(string collectionName, long id, string jsonData)
        {
            ETTaskCompletionSource<ComponentWithId> tcs = new ETTaskCompletionSource<ComponentWithId>();
            CacheUpdateByIdTask cacheUpdateByIdTask = ComponentFactory.CreateWithId<CacheUpdateByIdTask, string, string, ETTaskCompletionSource<ComponentWithId>>(id, collectionName, jsonData, tcs);
            this.tasks[(int)((ulong)id % taskCount)].Add(cacheUpdateByIdTask);

            return tcs.Task;
        }

        /// <summary>
        /// 刪除
        /// </summary>
        /// <param name="collectionName"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public ETTask<bool> Delete(string collectionName, long id) 
        {
            ETTaskCompletionSource<bool> tcs = new ETTaskCompletionSource<bool>();
            CacheDeleteByIdTask cacheDeleteTask = ComponentFactory.CreateWithId<CacheDeleteByIdTask, string, ETTaskCompletionSource<bool>>(id, collectionName, tcs);
            this.tasks[(int)((ulong)id % taskCount)].Add(cacheDeleteTask);

            return tcs.Task;
        }

        /// <summary>
        /// 用複合唯一索引刪除
        /// </summary>
        /// <param name="collectionName"></param>
        /// <param name="uniqueName"></param>
        /// <param name="json"></param>
        /// <returns></returns>
        public ETTask<bool> DeleteByUnique(string collectionName, string uniqueName, string json)
        {
            ETTaskCompletionSource<bool> tcs = new ETTaskCompletionSource<bool>();
            CacheDeleteByUniqueTask cacheDeleteByUniqueTask = ComponentFactory.Create<CacheDeleteByUniqueTask, string, ETTaskCompletionSource<bool>>(collectionName, tcs);
            cacheDeleteByUniqueTask.UniqueName = uniqueName;
            cacheDeleteByUniqueTask.Json = json;
            this.tasks[(int)((ulong)cacheDeleteByUniqueTask.Id % taskCount)].Add(cacheDeleteByUniqueTask);

            return tcs.Task;
        }

        /// <summary>
        /// 取得該集合全部的主鍵
        /// </summary>
        /// <param name="collectionName"></param>
        /// <returns></returns>
        public ETTask<List<long>> GetAllIds(string collectionName)
        {
            ETTaskCompletionSource<List<long>> tcs = new ETTaskCompletionSource<List<long>>();
            CacheGetAllIdsTask cacheGetAllIdsTask = ComponentFactory.Create<CacheGetAllIdsTask, string, ETTaskCompletionSource<List<long>>>(collectionName, tcs);
            this.tasks[(int)((ulong)cacheGetAllIdsTask.Id % taskCount)].Add(cacheGetAllIdsTask);

            return tcs.Task;
        }
    }
}
