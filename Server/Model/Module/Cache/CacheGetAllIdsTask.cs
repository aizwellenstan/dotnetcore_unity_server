using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using System;
using System.Collections.Generic;
using System.Text;

namespace ETModel
{
    [ObjectSystem]
    public class CacheGetAllIdsTaskAwakeSystem : AwakeSystem<CacheGetAllIdsTask, string, ETTaskCompletionSource<List<long>>>
    {
        public override void Awake(CacheGetAllIdsTask self, string collectionName, ETTaskCompletionSource<List<long>> tcs)
        {
            self.Tcs = tcs;
            self.CollectionName = collectionName;
        }
    }

    public class CacheGetAllIdsTask : DBTask 
    {
        public ETTaskCompletionSource<List<long>> Tcs { get; set; }

        public string CollectionName { get; set; }

        public override async ETTask Run()
        {
            var cacheComponent = Game.Scene.GetComponent<CacheComponent>();
            var cache = cacheComponent.GetCache(CollectionName);
            try
            {
                List<long> res = await cache.GetAllIds();
                this.Tcs.SetResult(res);
            }
            catch (Exception e)
            {
                this.Tcs.SetException(new Exception($"gets cache all ids failed on class = {CollectionName}", e));
            }
        }
    }
}