using System;
using System.Collections.Generic;
using System.Text;

namespace ETModel
{
    [ObjectSystem]
    public class CacheFindOneByUniqueIndexSystem : AwakeSystem<CacheFindOneByUniqueIndexTask<ComponentWithId>, CacheBase, Tuple<string, ComponentWithId>, ETTaskCompletionSource<ComponentWithId>>
    {
        public override void Awake(CacheFindOneByUniqueIndexTask<ComponentWithId> self, CacheBase proxy, Tuple<string, ComponentWithId> para, ETTaskCompletionSource<ComponentWithId> tcs)
        {
            self.proxy = proxy;
            self.Tcs = tcs;
            self.parameter = para;
        }
    }

    public class CacheFindOneByUniqueIndexTask<T> : DBTask where T : ComponentWithId
    {
        public CacheBase proxy { get; set; }

        public ETTaskCompletionSource<T> Tcs { get; set; }

        public Tuple<string, T> parameter { get; set; }

        public override async ETTask Run()
        {
            try
            {
                T res = await proxy.FindOneByUniqueIndex<T>(parameter.Item1, parameter.Item2);
                this.Tcs.SetResult(res);
            }
            catch (Exception e)
            {
                this.Tcs.SetException(new Exception($"finds one from cache that throws exception on collection = {typeof(T).Name} with" +
                    $" index name = {parameter.Item1}", e));
            }
        }
    }
}

