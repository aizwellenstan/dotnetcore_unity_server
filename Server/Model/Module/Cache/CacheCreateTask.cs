using System;
using System.Collections.Generic;
using System.Text;

namespace ETModel
{
    [ObjectSystem]
    public class CacheCreateSystem : AwakeSystem<CacheCreateTask<ComponentWithId>, CacheBase, ComponentWithId, ETTaskCompletionSource<ComponentWithId>>
    {
        public override void Awake(CacheCreateTask<ComponentWithId> self, CacheBase proxy, ComponentWithId entity, ETTaskCompletionSource<ComponentWithId> tcs)
        {
            self.proxy = proxy;
            self.Tcs = tcs;
            self.entity = entity;
        }
    }

    public class CacheCreateTask<T> : DBTask where T : ComponentWithId
    {
        public CacheBase proxy { get; set; }

        public ETTaskCompletionSource<T> Tcs { get; set; }

        public T entity { get; set; }

        public override async ETTask Run()
        {
            try
            {
                T res = await proxy.Create(entity);
                this.Tcs.SetResult(res);
            }
            catch (Exception e)
            {
                this.Tcs.SetException(new Exception($"inserts cache failed on class = {typeof(T).Name} with id = {entity.Id}", e));
            }
        }
    }

    /**************************************/

    [ObjectSystem]
    public class CacheCreateAwakeSystem : AwakeSystem<CacheCreateTask, string, ComponentWithId, ETTaskCompletionSource<ComponentWithId>>
    {
        public override void Awake(CacheCreateTask self, string collectionName, ComponentWithId component, ETTaskCompletionSource<ComponentWithId> tcs)
        {
            self.Tcs = tcs;
            self.CollectionName = collectionName;
            self.Component = component;
        }
    }

    public class CacheCreateTask : DBTask
    {
        public ETTaskCompletionSource<ComponentWithId> Tcs { get; set; }

        public string CollectionName { get; set; }

        public ComponentWithId Component { get; set; }

        public override async ETTask Run()
        {
            var cacheComponent = Game.Scene.GetComponent<CacheComponent>();
            var cache = cacheComponent.GetCache(CollectionName);
            try
            {
                ComponentWithId res = await cache.Create(Component);
                this.Tcs.SetResult(res);
            }
            catch (Exception e)
            {
                this.Tcs.SetException(new Exception($"inserts cache failed on class = {CollectionName} with id = {Component.Id}", e));
            }
        }
    }
}