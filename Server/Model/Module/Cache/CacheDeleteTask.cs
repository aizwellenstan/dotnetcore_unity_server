using System;
using System.Collections.Generic;
using System.Text;

namespace ETModel
{
    [ObjectSystem]
    public class CacheDeleteSystem : AwakeSystem<CacheDeleteTask<ComponentWithId>, CacheBase, ETTaskCompletionSource<bool>>
    {
        public override void Awake(CacheDeleteTask<ComponentWithId> self, CacheBase proxy, ETTaskCompletionSource<bool> tcs)
        {
            self.proxy = proxy;
            self.Tcs = tcs;
        }
    }

    public class CacheDeleteTask<T> : DBTask where T : ComponentWithId
    {
        public CacheBase proxy { get; set; }

        public ETTaskCompletionSource<bool> Tcs { get; set; }

        public override async ETTask Run()
        {
            try
            {
                bool res = await proxy.Delete<T>(this.Id);
                this.Tcs.SetResult(res);
            }
            catch (Exception e)
            {
                this.Tcs.SetException(new Exception($"deletes cache failed on class = {typeof(T).Name} with id = {this.Id}", e));
            }
        }
    }
}