using System;
using System.Collections.Generic;
using System.Text;

namespace ETModel
{
    [ObjectSystem]
    public class CacheFindOneSystem : AwakeSystem<CacheFindOneTask<ComponentWithId>, CacheBase, ETTaskCompletionSource<ComponentWithId>>
    {
        public override void Awake(CacheFindOneTask<ComponentWithId> self, CacheBase proxy, ETTaskCompletionSource<ComponentWithId> tcs)
        {
            self.proxy = proxy;
            self.Tcs = tcs;
        }
    }

    public class CacheFindOneTask<T> : DBTask where T : ComponentWithId
    {
        public CacheBase proxy { get; set; }

        public ETTaskCompletionSource<T> Tcs { get; set; }

        public override async ETTask Run()
        {
            try
            {
                T res = await proxy.FindOne<T>(this.Id);
                this.Tcs.SetResult(res);
            }
            catch (Exception e)
            {
                this.Tcs.SetException(new Exception($"快取查詢例外! class: {typeof(T).Name} {this.Id}", e));
            }
        }
    }
}
