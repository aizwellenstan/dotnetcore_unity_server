using System;
using System.Collections.Generic;
using System.Text;

namespace ETModel
{
    [ObjectSystem]
    public class CacheFindAllByIndexSystem : AwakeSystem<CacheFindAllByIndexTask<ComponentWithId>, CacheBase, Tuple<string, ComponentWithId>, ETTaskCompletionSource<List<ComponentWithId>>>
    {
        public override void Awake(CacheFindAllByIndexTask<ComponentWithId> self, CacheBase proxy, Tuple<string, ComponentWithId> para, ETTaskCompletionSource<List<ComponentWithId>> tcs)
        {
            self.proxy = proxy;
            self.Tcs = tcs;
            self.parameter = para;
        }
    }

    public class CacheFindAllByIndexTask<T> : DBTask where T : ComponentWithId
    {
        public CacheBase proxy { get; set; }

        public ETTaskCompletionSource<List<T>> Tcs { get; set; }

        public Tuple<string, T> parameter { get; set; }

        public override async ETTask Run()
        {
            try
            {
                List<T> res = await proxy.FindAllByIndex(parameter.Item1, parameter.Item2);
                this.Tcs.SetResult(res);
            }
            catch (Exception e)
            {
                this.Tcs.SetException(new Exception($"finds all from cache failed on collection = {typeof(T).Name} with" +
                    $" index name = {parameter.Item1}", e));
            }
        }
    }
}