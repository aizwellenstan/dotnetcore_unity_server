using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using System;
using System.Collections.Generic;
using System.Text;

namespace ETModel
{
    [ObjectSystem]
    public class CacheDeleteByUniqueTaskAwakeSystem : AwakeSystem<CacheDeleteByUniqueTask, string, ETTaskCompletionSource<bool>>
    {
        public override void Awake(CacheDeleteByUniqueTask self, string collectionName, ETTaskCompletionSource<bool> tcs)
        {
            self.Tcs = tcs;
            self.CollectionName = collectionName;
        }
    }

    public class CacheDeleteByUniqueTask : DBTask 
    {
        public ETTaskCompletionSource<bool> Tcs { get; set; }

        public string CollectionName { get; set; }

        public string UniqueName { get; set; }

        public string Json { get; set; }

        public override async ETTask Run()
        {
            var cacheComponent = Game.Scene.GetComponent<CacheComponent>();
            var cache = cacheComponent.GetCache(CollectionName);
            try
            {
                bool res = await cache.DeleteByUnique(UniqueName, BsonSerializer.Deserialize<BsonDocument>(Json));
                this.Tcs.SetResult(res);
            }
            catch (Exception e)
            {
                this.Tcs.SetException(new Exception($"deletes cache failed on class = {CollectionName} with id = {this.Id}", e));
            }
        }
    }
}