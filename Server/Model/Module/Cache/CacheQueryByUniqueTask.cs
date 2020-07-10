using System;
using System.Collections.Generic;
using System.Text;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;

namespace ETModel
{
    [ObjectSystem]
    public class CacheQueryByUniqueTaskAwakeSystem : AwakeSystem<CacheQueryByUniqueTask, string, ETTaskCompletionSource<ComponentWithId>>
    {
        public override void Awake(CacheQueryByUniqueTask self, string collectionName, ETTaskCompletionSource<ComponentWithId> tcs)
        {
            self.Tcs = tcs;
            self.CollectionName = collectionName;
        }
    }

    public class CacheQueryByUniqueTask : DBTask 
    {
        public ETTaskCompletionSource<ComponentWithId> Tcs { get; set; }

        public string CollectionName { get; set; }

        public string UniqueName { get; set; }

        public string Json { get; set; }

        public override async ETTask Run()
        {
            var cacheComponent = Game.Scene.GetComponent<CacheComponent>();
            var cache = cacheComponent.GetCache(CollectionName);
            try
            {
                ComponentWithId res = await cache.FindOneByUnique(cache.type, UniqueName, BsonSerializer.Deserialize<BsonDocument>(Json));
                this.Tcs.SetResult(res);
            }
            catch (Exception e)
            {
                this.Tcs.SetException(new Exception($"deletes cache failed on class = {CollectionName} with id = {this.Id}", e));
            }
        }
    }
}