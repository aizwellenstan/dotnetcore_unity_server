using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using ETModel;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using System.Linq;
using StackExchange.Redis;
using System.Threading.Tasks;
using System.Reflection;

namespace ETHotfix
{
    [ObjectSystem]
    public class RedisEventSolverComponentAwakeSystem : AwakeSystem<RedisEventSolverComponent, CacheProxyComponent, Type>
    {
        public override void Awake(RedisEventSolverComponent self, CacheProxyComponent cacheProxyComponent, Type type)
        {
            self.Awake(cacheProxyComponent, type);
        }
    }

    [ObjectSystem]
    public class RedisEventSolverComponentStartSystem : StartSystem<RedisEventSolverComponent>
    {
        public override async void Start(RedisEventSolverComponent self)
        {
            // 載入資料
            await self.Reload();
            //StartAsync(self).Coroutine();
        }

        public async ETTask StartAsync(RedisEventSolverComponent self)
        {
            long instanceId = self.InstanceId;

            while (true)
            {
                if (self.InstanceId != instanceId)
                {
                    return;
                }

                try
                {
                    var kp = await Get(self);
                    await HandleReceiveMessage(self, kp.Key, kp.Value);
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
        }

        public ETTask<KeyValuePair<RedisChannel, RedisValue>> Get(RedisEventSolverComponent self)
        {
            if (self.receivedQueue.TryDequeue(out var kp))
            {
                return ETTask.FromResult(kp);
            }

            ETTaskCompletionSource<KeyValuePair<RedisChannel, RedisValue>> t = new ETTaskCompletionSource<KeyValuePair<RedisChannel, RedisValue>>();
            self.tcs = t;
            return t.Task;
        }

        private async ETTask HandleReceiveMessage(RedisEventSolverComponent self, RedisChannel channel, RedisValue msg)
        {
            if (channel == self.channelUpdateKey)
            {
                CacheHelper.ConvertMessage2RedisSubscribe(msg, out long id, out long appId);
                // 物件擁有者不接收事件
                if (IdGenerater.AppId == appId)
                    return;
                await self.Load(id);
                self.onUpdate?.Invoke(id);
                Log.Info($"MemorySync> Update:{self.collectionName}:{id} on AppId:{appId}");
            }

            if (channel == self.channelDeleteKey)
            {
                CacheHelper.ConvertMessage2RedisSubscribe(msg, out long id, out long appId);
                // 物件擁有者不接收事件
                if (IdGenerater.AppId == appId)
                    return;
                self.onWillDelete?.Invoke(id);
                await self.Load(id);
                self.onDelete?.Invoke(id);
                Log.Info($"MemorySync> Delete:{self.collectionName}:{id} on AppId:{appId}");
            }

            if (channel == self.channelCreateKey)
            {
                CacheHelper.ConvertMessage2RedisSubscribe(msg, out long id, out long appId);
                // 物件擁有者不接收事件
                if (IdGenerater.AppId == appId)
                    return;
                await self.Load(id);
                self.onCreate?.Invoke(id);
                Log.Info($"MemorySync> Create:{self.collectionName}:{id} on AppId:{appId}");
            }

            if (channel == self.channelRefreshKey)
            {
                CacheHelper.ConvertMessage2RedisSubscribe(msg, out long id, out long appId);
                // 物件擁有者不接收事件
                if (IdGenerater.AppId == appId)
                    return;
                await self.Reload();
                Log.Info($"MemorySync> Reload:{self.collectionName} on AppId:{appId}");
            }
        }
    }

    //[ObjectSystem]
    //public class RedisEventSolverComponentUpdateSystem : UpdateSystem<RedisEventSolverComponent>
    //{
    //    public override void Update(RedisEventSolverComponent self)
    //    {
    //        self.Update();
    //    }
    //}

    [ObjectSystem]
    public class RedisEventSolverComponentDestroySystem : DestroySystem<RedisEventSolverComponent>
    {
        public override void Destroy(RedisEventSolverComponent self)
        {
            self.Destroy();
        }
    }

    public static class RedisEventSolverComponentSystem
    {
        public static void Awake(this RedisEventSolverComponent self, CacheProxyComponent cacheProxyComponent, Type type)
        {
            self.cacheProxyComponent = cacheProxyComponent;
            self.type = type;
            self.collectionName = type.Name.ToLower();

            // 訂閱頻道
            self.redisSubClient.GetSubscriber().Subscribe(self.channelDeleteKey, self.OnReceiveMessage);
            self.redisSubClient.GetSubscriber().Subscribe(self.channelUpdateKey, self.OnReceiveMessage);
            self.redisSubClient.GetSubscriber().Subscribe(self.channelCreateKey, self.OnReceiveMessage);
            self.redisSubClient.GetSubscriber().Subscribe(self.channelRefreshKey, self.OnReceiveMessage);
        }

        public static void Update(this RedisEventSolverComponent self)
        {
            if (!self.flag)
            {
                var t = self.tcs;
                self.tcs = null;
                if (self.receivedQueue.TryDequeue(out var kp))
                {
                    t.SetResult(kp);
                    self.flag = true;
                }
            }
        }

        public static void Destroy(this RedisEventSolverComponent self)
        {
            // 退訂頻道
            self.redisSubClient.GetSubscriber().Unsubscribe(self.channelDeleteKey);
            self.redisSubClient.GetSubscriber().Unsubscribe(self.channelUpdateKey);
            self.redisSubClient.GetSubscriber().Unsubscribe(self.channelCreateKey);
            self.redisSubClient.GetSubscriber().Unsubscribe(self.channelRefreshKey);
            foreach(var v in self.Data)
            {
                v.Value.Dispose();
            }
            self.Data.Clear();
            self.receivedQueue.Clear();
            self.onCreate = null;
            self.onUpdate = null;
            self.onDelete = null;
            self.onRefresh = null;
        }

        private static void OnReceiveMessage(this RedisEventSolverComponent self, RedisChannel channel, RedisValue id)
        {
            var kp = new KeyValuePair<RedisChannel, RedisValue>(channel, id);
            OneThreadSynchronizationContext.Instance.Post(self.QueueOnMainThread, kp);

            //var kp = new KeyValuePair<RedisChannel, RedisValue>(channel, id);
            //self.receivedQueue.Enqueue(kp);

            //bool lockTaken = false;
            //try
            //{
            //    self._spinlock.Enter(ref lockTaken);
            //    if (self.tcs != null && self.flag)
            //    {
            //        self.flag = false;
            //    }
            //}
            //finally
            //{
            //    if (lockTaken) self._spinlock.Exit(false);
            //}
        }

        /// <summary>
        /// 把Redis得到的訊息推到主Queue
        /// </summary>
        /// <param name="self"></param>
        /// <param name="para"></param>
        private static async void QueueOnMainThread(this RedisEventSolverComponent self, object para)
        {
            var kp = (KeyValuePair<RedisChannel, RedisValue>)para;
            var channel = kp.Key;
            var msg = kp.Value;
            if (channel == self.channelUpdateKey)
            {
                CacheHelper.ConvertMessage2RedisSubscribe(msg, out long id, out long appId);
                // 物件擁有者不接收事件
                if (IdGenerater.AppId == appId)
                    return;
                await self.Load(id);
                self.onUpdate?.Invoke(id);
                Log.Info($"MemorySync> Update:{self.collectionName}:{id} on AppId:{appId}");
            }

            if (channel == self.channelDeleteKey)
            {
                CacheHelper.ConvertMessage2RedisSubscribe(msg, out long id, out long appId);
                // 物件擁有者不接收事件
                if (IdGenerater.AppId == appId)
                    return;
                self.onWillDelete?.Invoke(id);
                await self.Load(id);
                self.onDelete?.Invoke(id);
                Log.Info($"MemorySync> Delete:{self.collectionName}:{id} on AppId:{appId}");
            }

            if (channel == self.channelCreateKey)
            {
                CacheHelper.ConvertMessage2RedisSubscribe(msg, out long id, out long appId);
                // 物件擁有者不接收事件
                if (IdGenerater.AppId == appId)
                    return;
                await self.Load(id);
                self.onCreate?.Invoke(id);
                Log.Info($"MemorySync> Create:{self.collectionName}:{id} on AppId:{appId}");
            }

            if (channel == self.channelRefreshKey)
            {
                CacheHelper.ConvertMessage2RedisSubscribe(msg, out long id, out long appId);
                // 物件擁有者不接收事件
                if (IdGenerater.AppId == appId)
                    return;
                await self.Reload();
                Log.Info($"MemorySync> Reload:{self.collectionName} on AppId:{appId}");
            }
        }

        public static async ETTask<T> Create<T>(this RedisEventSolverComponent self, T entity) where T : ComponentWithId
        {
            if (self.isDebug)
            {
                OtherHelper.LogCallStackMessage("Create");
            }

            var result = await self.cacheProxyComponent.Create(entity);
            T returnData = default;
            if (result != default)
            {
                var msg = CacheHelper.ConvertRedisPublish2Message(entity.Id, IdGenerater.AppId);
                await self.redisPubClient.GetSubscriber().PublishAsync(self.channelCreateKey, msg);
                // 誰創的誰就是物件的擁有者
                // 如果是自己產生的事件循環自己管，即你用系統CreateWithId產生的有事件循環，自己new的沒有，都依自己傳來的entity怎麼生的
                self.Data[entity.Id] = entity;
                self.mineSet.Add(entity.Id);
                returnData = entity;
                // 觸發事件
                self.onRefresh?.Invoke(entity.Id);
                self.onCreate?.Invoke(entity.Id);
            }
            return returnData;
        }

        public static async ETTask<T> Update<T>(this RedisEventSolverComponent self, T entity) where T : ComponentWithId
        {
            if (self.isDebug)
            {
                OtherHelper.LogCallStackMessage("Update");
            }

            var result = await self.cacheProxyComponent.UpdateById(entity);
            T returnData = default;
            if (result != default)
            {
                var msg = CacheHelper.ConvertRedisPublish2Message(entity.Id, IdGenerater.AppId);
                await self.redisPubClient.GetSubscriber().PublishAsync(self.channelUpdateKey, msg);
                // 保證底層Data的Reference一致
                self.Write(entity.Id, entity);
                returnData = (T)self.Data[entity.Id];
                // 觸發事件
                self.onRefresh?.Invoke(entity.Id);
                self.onUpdate?.Invoke(entity.Id);
            }
            return returnData;
        }

        /// <summary>
        /// 刪除快取同步物件
        /// 盡量在函式的最後呼叫，怕Dispose先呼叫後，把刪除後一些後續動作要用到的欄位給初始化掉了
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="self"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public static async ETTask<bool> Delete<T>(this RedisEventSolverComponent self, long id) where T : ComponentWithId
        {
            if (self.isDebug)
            {
                OtherHelper.LogCallStackMessage("Delete");
            }

            var result = await self.cacheProxyComponent.QueryById<T>(id);
            var isSucc = false;

            if (result != default)
            {
                isSucc = await self.cacheProxyComponent.DeleteById<T>(id);
                if (isSucc)
                {
                    var msg = CacheHelper.ConvertRedisPublish2Message(id, IdGenerater.AppId);
                    var count = await self.redisPubClient.GetSubscriber().PublishAsync(self.channelDeleteKey, msg);
                    self.onWillDelete?.Invoke(id);
                    if (self.Data.TryGetValue(id, out var component))
                    {
                        component.Dispose();
                        self.Data.Remove(id);
                        self.mineSet.Remove(id);
                    }
                    // 觸發事件
                    self.onRefresh?.Invoke(id);
                    self.onDelete?.Invoke(id);
                }
                else
                {
                    Log.Warning($"to delete cache is failed! object has deleted on {typeof(T).Name}[{id}]");
                }
            }

            return isSucc;
        }

        public static async ETTask<long> Refresh(this RedisEventSolverComponent self)
        {
            var msg = CacheHelper.ConvertRedisPublish2Message(CacheHelper.REFRESH_FLAG, IdGenerater.AppId);
            return await self.redisPubClient.GetSubscriber().PublishAsync(self.channelRefreshKey, msg);
        }

        public static async ETTask Load(this RedisEventSolverComponent self, long id)
        {
            var propNames = CacheHelper.GetSyncPropertyNames(self.type);
            ComponentWithId entity = await self.cacheProxyComponent.QueryById<ComponentWithId>(self.collectionName, id, propNames.Count == 0 ? null : propNames);
            if (entity != null)
            {
                self.Write(id, entity);
                self.onRefresh?.Invoke(id);
            }
            else
            {
                if (self.Data.TryGetValue(id, out var component))
                {
                    component.Dispose();
                    self.Data.Remove(id);
                    self.mineSet.Remove(id);
                    self.onRefresh?.Invoke(id);
                }
            }
        }

        /// <summary>
        /// 重新載入Cache裡的資料到記憶體
        /// 目的是當某個進程被開啟後要把現在Redis有的資料載入進來
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static async ETTask Reload(this RedisEventSolverComponent self)
        {
            var list = await self.cacheProxyComponent.GetAllIds(self.collectionName);
            var database = self.redisClient.GetDatabase();
            var tasks = new List<Task<HashEntry[]>>(list.Count);
            var batch = list.Aggregate(database.CreateBatch(), (pipeline, id) =>
            {
                tasks.Add(pipeline.HashGetAllAsync($"{self.collectionName}:{id}"));
                return pipeline;
            });
            batch.Execute();

            var results = await Task.WhenAll(tasks);

            for (int i = 0; i < results.GetLength(0); i++)
            {
                var result = results[i];
                if (result.Length != 0)
                {
                    var doc = CacheHelper.Deserialize(self.type, result);
                    self.Write(list[i], (ComponentWithId)BsonSerializer.Deserialize(doc, self.type));
                }
            }
            self.onRefresh?.Invoke(null);
        }

        public static void Write(this RedisEventSolverComponent self, long id, ComponentWithId src)
        {
            if (self.Data.TryGetValue(id, out var des))
            {
                var propNames = CacheHelper.GetSyncPropertyNames(self.type);
                for (int i = 0; i < propNames.Count; i++)
                {
                    string propName = propNames[i];
                    PropertyInfo property = CacheHelper.GetSyncPropertyInfo(self.type, propName);
                    object val = property.GetValue(src);
                    property.SetValue(des, val);
                }
            }
            else
            {
                // 保險起見，避免Redis同步來的資料沒有id
                src.Id = id;
                // 如果有定義[ObjectSystem]就附加事件(Awake[必需提供一個無參數Awake]、Update等等)
                // 但帶有SyncIgnore的欄位會被忽略，所以只有擁有者才可以預先對這先欄位指派值，並且
                // 也只有擁有者能使用這些值，其他人擁有的都是初始值:null、0等等，直接拿來使用會有問題的
                if (self.isToUseObjectSystem)
                {
                    var component = ComponentFactory.CreateWithId(self.type, src.Id);
                    var propNames = CacheHelper.GetSyncPropertyNames(self.type);
                    for (int i = 0; i < propNames.Count; i++)
                    {
                        string propName = propNames[i];
                        PropertyInfo property = CacheHelper.GetSyncPropertyInfo(self.type, propName);
                        object val = property.GetValue(src);
                        property.SetValue(component, val);
                    }
                    self.Data[id] = (ComponentWithId)component;
                }
                else
                {
                    self.Data[id] = src;
                }
            }
        }
    }

    [ObjectSystem]
    public class CacheProxyComponentAwakeSystem : AwakeSystem<CacheProxyComponent>
    {
        public override void Awake(CacheProxyComponent self)
        {
            try
            {
                self.Awake();
            }
            catch(Exception e)
            {
                Log.Error(e);
            }
        }
    }

    [ObjectSystem]
    public class CacheProxyComponentDestroySystem : DestroySystem<CacheProxyComponent>
    {
        public override void Destroy(CacheProxyComponent self)
        {
            self.Destroy();
        }
    }

    /// <summary>
    /// 快取數據用
    /// </summary>
    public static class CacheProxyComponentSystem
    {
        public static void Awake(this CacheProxyComponent self)
        {
            StartConfig dbStartConfig = StartConfigComponent.Instance.DBConfig;
            self.dbAddress = dbStartConfig.GetComponent<InnerConfig>().IPEndPoint;
            CacheConfig cacheConfig = dbStartConfig.GetComponent<CacheConfig>();
            if (cacheConfig == null)
            {
                self.connectionString = CacheComponent.connectionString;
            }
            else
            {
                self.connectionString = cacheConfig.ConnectionString;
            }

            Log.Info($"to prepare initializing redis......");

            // to establish connection with Redis
            self.redisClient = ConnectionMultiplexer.Connect(self.connectionString);
            self.redisSubClient = ConnectionMultiplexer.Connect(self.connectionString);
            self.redisPubClient = ConnectionMultiplexer.Connect(self.connectionString);

            Log.Info($"to connect redis is successful.");

            var startConfigComponent = Game.Scene.GetComponent<StartConfigComponent>();
            foreach (var v in DBHelper.GetCacheUseModesIter())
            {
                if (!CacheHelper.IsSyncOn(v.Key, startConfigComponent.StartConfig.AppType))
                {
                    continue;
                }
                switch (v.Value)
                {
                    case CacheUseMode.MemorySyncByDB:
                        break;
                    case CacheUseMode.MemorySyncByRedis:
                        var component = ComponentFactory.Create<RedisEventSolverComponent, CacheProxyComponent, Type>(self, v.Key);
                        self.redisEventSolvers.Add(v.Key, component);
                        break;
                }
            }
        }

        public static void Destroy(this CacheProxyComponent self)
        {
            self.redisClient.Close();
            self.redisSubClient.Close();
            self.redisPubClient.Close();
        }

        public static async ETTask<T> QueryById<T>(this CacheProxyComponent self, long id) where T : ComponentWithId
        {
            return await self.QueryById<T>(typeof(T).Name.ToLower(), id);
        }

        public static async ETTask<T> QueryById<T>(this CacheProxyComponent self, string collectionName, long id, List<string> fields = null) where T : ComponentWithId
        {
            Session session = Game.Scene.GetComponent<NetInnerComponent>().Get(self.dbAddress);
            CacheQueryResponse cacheQueryByIdResponse = (CacheQueryResponse)await session.Call(new CacheQueryByIdRequest
            {
                CollectionName = collectionName,
                Id = id,
                Fields = fields,
            });
            return (T)cacheQueryByIdResponse.Component;
        }

        public static async ETTask<T> QueryByUnique<T>(this CacheProxyComponent self, string uniqueName, BsonDocument condition)
            where T : ComponentWithId
        {
            Session session = Game.Scene.GetComponent<NetInnerComponent>().Get(self.dbAddress);
            CacheQueryResponse cacheQueryByUniqueResponse = (CacheQueryResponse)await session.Call(new CacheQueryByUniqueRequest
            {
                CollectionName = typeof(T).Name.ToLower(),
                Json = condition.ToJson(),
                UniqueName = uniqueName,
            });
            return (T)cacheQueryByUniqueResponse.Component;
        }

        public static async ETTask<T> Create<T>(this CacheProxyComponent self, T entity) where T : ComponentWithId
        {
            Session session = Game.Scene.GetComponent<NetInnerComponent>().Get(self.dbAddress);
            CacheQueryResponse cacheCreateResponse = (CacheQueryResponse)await session.Call(new CacheCreateRequest
            {
                CollectionName = typeof(T).Name.ToLower(),
                Component = entity,
            });
            return (T)cacheCreateResponse.Component;
        }

        public static async ETTask<T> UpdateById<T>(this CacheProxyComponent self, T entity) where T : ComponentWithId
        {
            // TODO:待觀察的Bug
            try
            {
                return await self.UpdateById<T>(entity.Id, entity.ToBsonDocument().ToJson());
            }
            catch (Exception ex)
            {
                Log.Error($"Type:{entity.GetType().Name}, Entity:{entity.Id}, Message:{ex.Message}, Stack:{ex.StackTrace}");
            }
            return null;
        }

        public static async ETTask<T> UpdateById<T>(this CacheProxyComponent self, long id, BsonDocument updatedData) where T : ComponentWithId
        {
            return await self.UpdateById<T>(id, updatedData.ToJson());
        }

        public static async ETTask<T> UpdateById<T>(this CacheProxyComponent self, long id, string updatedData) where T : ComponentWithId
        {
            Session session = Game.Scene.GetComponent<NetInnerComponent>().Get(self.dbAddress);
            CacheQueryResponse cacheUpdateByIdResponse = (CacheQueryResponse)await session.Call(new CacheUpdateByIdRequest
            {
                CollectionName = typeof(T).Name.ToLower(),
                Id = id,
                DataJson = updatedData,
            });
            return (T)cacheUpdateByIdResponse.Component;
        }

        public static async ETTask<bool> DeleteById<T>(this CacheProxyComponent self, long id) where T : ComponentWithId
        {
            Session session = Game.Scene.GetComponent<NetInnerComponent>().Get(self.dbAddress);
            CacheDeleteResponse cacheDeleteByIdResponse = (CacheDeleteResponse)await session.Call(new CacheDeleteByIdRequest
            { CollectionName = typeof(T).Name.ToLower(), Id = id });
            return cacheDeleteByIdResponse.IsSuccessful;
        }

        public static async ETTask<bool> DeleteByUnique<T>(this CacheProxyComponent self, string uniqueName, BsonDocument condition)
            where T : ComponentWithId
        {
            Session session = Game.Scene.GetComponent<NetInnerComponent>().Get(self.dbAddress);
            CacheDeleteResponse cacheDeleteResponse = (CacheDeleteResponse)await session.Call(new CacheDeleteByUniqueRequest
            { CollectionName = typeof(T).Name.ToLower(), UniqueName = uniqueName, Json = condition.ToJson() });
            return cacheDeleteResponse.IsSuccessful;
        }

        public static async ETTask<List<long>> GetAllIds(this CacheProxyComponent self, string collectionName)
        {
            Session session = Game.Scene.GetComponent<NetInnerComponent>().Get(self.dbAddress);
            CacheGetAllIdsResponse cacheDeleteResponse = (CacheGetAllIdsResponse)await session.Call(new CacheGetAllIdsRequest
            { CollectionName = collectionName });
            return cacheDeleteResponse.IdList;
        }

        public static async ETTask<bool> LockEvent(this CacheProxyComponent self, string key, string token, long timeout = 60000)
        {
            return await self.redisClient.GetDatabase().LockTakeAsync(self.GetLockName(key), token, TimeSpan.FromMilliseconds(timeout));
        }

        public static async ETTask<bool> UnlockEvent(this CacheProxyComponent self, string key, string token)
        {
            return await self.redisClient.GetDatabase().LockReleaseAsync(self.GetLockName(key), token);
        }
    }
}