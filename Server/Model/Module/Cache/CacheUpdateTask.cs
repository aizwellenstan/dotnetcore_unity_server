using System;
using System.Collections.Generic;
using System.Text;

namespace ETModel
{
    [ObjectSystem]
    public class CacheUpdateSystem : AwakeSystem<CacheUpdateTask<ComponentWithId>, CacheBase, ComponentWithId, ETTaskCompletionSource<ComponentWithId>>
    {
        public override void Awake(CacheUpdateTask<ComponentWithId> self, CacheBase proxy, ComponentWithId entity, ETTaskCompletionSource<ComponentWithId> tcs)
        {
            self.proxy = proxy;
            self.Tcs = tcs;
            self.entity = entity;
        }
    }

    public class CacheUpdateTask<T> : DBTask where T : ComponentWithId
    {
        public CacheBase proxy { get; set; }

        public ETTaskCompletionSource<T> Tcs { get; set; }

        public T entity { get; set; }

        public override async ETTask Run()
        {
            try
            {
                T res = await proxy.Update(entity);
                this.Tcs.SetResult(res);
            }
            catch (Exception e)
            {
                this.Tcs.SetException(new Exception($"updates cache failed on class = {typeof(T).Name} with id = {entity.Id}", e));
            }
        }
    }
}