using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using System;
using System.Collections.Generic;
using System.Text;

namespace ETModel
{
    [ObjectSystem]
    public class CacheUpdateByIdTaskAwakeSystem : AwakeSystem<CacheUpdateByIdTask, string, string, ETTaskCompletionSource<ComponentWithId>>
    {
        public override void Awake(CacheUpdateByIdTask self, string collectionName, string dataJson, ETTaskCompletionSource<ComponentWithId> tcs)
        {
            self.CollectionName = collectionName;
            self.Tcs = tcs;
            self.DataJson = dataJson;
        }
    }

    public class CacheUpdateByIdTask : DBTask 
    {
        public ETTaskCompletionSource<ComponentWithId> Tcs { get; set; }

        public string CollectionName { get; set; }

        public string DataJson { get; set; }

        public override async ETTask Run()
        {
            var cacheComponent = Game.Scene.GetComponent<CacheComponent>();
            var cache = cacheComponent.GetCache(CollectionName);
            try
            {
                ComponentWithId res = await cache.Update(cache.type, Id, BsonSerializer.Deserialize<BsonDocument>(DataJson));
                this.Tcs.SetResult(res);
            }
            catch (Exception e)
            {
                this.Tcs.SetException(new Exception($"update cache by id failed on class = {CollectionName} with id = {this.Id}", e));
            }
        }
    }
}