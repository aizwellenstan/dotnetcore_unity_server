using System;
using System.Collections.Generic;
using System.Text;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;

namespace ETModel
{
    [ObjectSystem]
    public class CacheQueryByIdTaskAwakeSystem : AwakeSystem<CacheQueryByIdTask, string, List<string>, ETTaskCompletionSource<ComponentWithId>>
    {
        public override void Awake(CacheQueryByIdTask self, string collectionName, List<string> fields, ETTaskCompletionSource<ComponentWithId> tcs)
        {
            self.Tcs = tcs;
            self.CollectionName = collectionName;
            self.Fields = fields != null ? fields.ToArray() : null;
        }
    }

    public class CacheQueryByIdTask : DBTask 
    {
        public ETTaskCompletionSource<ComponentWithId> Tcs { get; set; }

        public string CollectionName { get; set; }

        public string[] Fields { get; set; }

        public override async ETTask Run()
        {
            var cacheComponent = Game.Scene.GetComponent<CacheComponent>();
            var cache = cacheComponent.GetCache(CollectionName);
            try
            {
                ComponentWithId res = await cache.FindOne(cache.type, Id, Fields);
                this.Tcs.SetResult(res);
            }
            catch (Exception e)
            {
                this.Tcs.SetException(new Exception($"deletes cache failed on class = {CollectionName} with id = {this.Id}", e));
            }
        }
    }
}