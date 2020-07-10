using System;
using System.Collections.Generic;
using System.Text;

namespace ETModel
{
    [ObjectSystem]
    public class CacheUpsertSystem : AwakeSystem<CacheUpsertTask<ComponentWithId>, CacheBase, ComponentWithId, ETTaskCompletionSource<ComponentWithId>>
    {
        public override void Awake(CacheUpsertTask<ComponentWithId> self, CacheBase proxy, ComponentWithId entity, ETTaskCompletionSource<ComponentWithId> tcs)
        {
            self.proxy = proxy;
            self.Tcs = tcs;
            self.entity = entity;
        }
    }

    public class CacheUpsertTask<T> : DBTask where T : ComponentWithId
    {
        public CacheBase proxy { get; set; }

        public ETTaskCompletionSource<T> Tcs { get; set; }

        public T entity { get; set; }

        public override async ETTask Run()
        {
            try
            {
                T res = await proxy.Upsert(entity);
                this.Tcs.SetResult(res);
            }
            catch (Exception e)
            {
                this.Tcs.SetException(new Exception($"upserts cache failed on class = {typeof(T).Name} with id = {entity.Id}", e));
            }
        }
    }
}