using StackExchange.Redis;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Threading;

namespace ETModel
{
    public class RedisEventSolverComponent : Component
    {
        public CacheProxyComponent cacheProxyComponent { set; get; }

        public ConnectionMultiplexer redisClient => cacheProxyComponent.redisClient;

        public ConnectionMultiplexer redisSubClient => cacheProxyComponent.redisSubClient;

        public ConnectionMultiplexer redisPubClient => cacheProxyComponent.redisPubClient;

        public Dictionary<long, ComponentWithId> Data { private set; get; } = new Dictionary<long, ComponentWithId>();

        public ConcurrentQueue<KeyValuePair<RedisChannel, RedisValue>> receivedQueue { private set; get; } = new ConcurrentQueue<KeyValuePair<RedisChannel, RedisValue>>();

        public ETTaskCompletionSource<KeyValuePair<RedisChannel, RedisValue>> tcs { set; get; }

        public string channelDeleteKey => GetChannelName("delete");

        public string channelUpdateKey => GetChannelName("update");

        public string channelCreateKey => GetChannelName("create");

        public string channelRefreshKey => GetChannelName("refresh");

        public string key => CacheHelper.key;

        public string collectionName { set; get; }

        public string GetChannelName(string tag)
        {
            return $"{collectionName}:memorysync:{tag}";
        }

        public HashSet<long> mineSet = new HashSet<long>();

        /// <summary>
        /// 在任何異動後觸發
        /// 在這個回調下，Get跟IsMine不一定能使用，畢竟是異動後的結果
        /// </summary>
        public Action<long?> onRefresh;

        /// <summary>
        /// 在創建物件之後執行
        /// 在這個回調下，Get跟IsMine都能使用，且Get必有值
        /// </summary>
        public Action<long> onCreate;

        /// <summary>
        /// 在更新物件之後執行
        /// 在這個回調下，Get跟IsMine都能使用，且Get必有值
        /// </summary>
        public Action<long> onUpdate;

        /// <summary>
        /// 在刪除之後，要做些什麼
        /// 在這個回調下，Get必為Null且IsMine必為False
        /// </summary>
        public Action<long> onDelete;

        /// <summary>
        /// 在刪除之前，要做些什麼
        /// 在這個回調下，Get跟IsMine都能使用
        /// </summary>
        public Action<long> onWillDelete;

        public Type type { set; get; }

        public bool isToUseObjectSystem { set; get; }

        public bool flag { set; get; } = true;

        public SpinLock _spinlock = new SpinLock();

        public bool isDebug { set; get; } = false;

        public T Get<T>(long id) where T : ComponentWithId
        {
            Data.TryGetValue(id, out var val);
            return (T)val;
        }

        public bool IsMine(long id)
        {
            return mineSet.Contains(id);
        }
    }

    public class CacheProxyComponent : Component
    {
        public IPEndPoint dbAddress;

        public string connectionString { set; get; }

        public ConnectionMultiplexer redisClient { set; get; }

        public ConnectionMultiplexer redisSubClient { set; get; }

        public ConnectionMultiplexer redisPubClient { set; get; }

        public readonly Dictionary<Type, RedisEventSolverComponent> redisEventSolvers = new Dictionary<Type, RedisEventSolverComponent>();
       
        public RedisEventSolverComponent GetMemorySyncSolver<T>() where T : ComponentWithId
        {
            redisEventSolvers.TryGetValue(typeof(T), out var solver);
            return solver;
        }

        public string GetLockName(string key)
        {
            return $"_lockevent:{key}";
        }
    }
}
