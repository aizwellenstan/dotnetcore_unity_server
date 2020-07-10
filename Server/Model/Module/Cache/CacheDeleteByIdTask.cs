using System;
using System.Collections.Generic;
using System.Text;

namespace ETModel
{
    [ObjectSystem]
    public class CacheDeleteByIdTaskAwakeSystem : AwakeSystem<CacheDeleteByIdTask, string, ETTaskCompletionSource<bool>>
    {
        public override void Awake(CacheDeleteByIdTask self, string collectionName, ETTaskCompletionSource<bool> tcs)
        {
            self.Tcs = tcs;
            self.CollectionName = collectionName;
        }
    }

    public class CacheDeleteByIdTask : DBTask 
    {
        public ETTaskCompletionSource<bool> Tcs { get; set; }

        public string CollectionName { get; set; }

        public override async ETTask Run()
        {
            var cacheComponent = Game.Scene.GetComponent<CacheComponent>();
            var cache = cacheComponent.GetCache(CollectionName);
            try
            {
                bool res = await cache.Delete(this.Id);
                this.Tcs.SetResult(res);
            }
            catch (Exception e)
            {
                this.Tcs.SetException(new Exception($"deletes cache failed on class = {CollectionName} with id = {this.Id}", e));
            }
        }
    }
}