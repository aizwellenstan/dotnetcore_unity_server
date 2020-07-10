using System;
using System.Collections.Generic;
using System.Reflection;
using MongoDB.Driver;

namespace ETModel
{
    [ObjectSystem]
    public class DBQueryAllWithIndexSystem : AwakeSystem<DBQueryAllWithIndexTask, string, List<Tuple<PropertyInfo, object>>, ETTaskCompletionSource<List<ComponentWithId>>>
    {
        public override void Awake(DBQueryAllWithIndexTask self, string collectionName, List<Tuple<PropertyInfo, object>> parameters, ETTaskCompletionSource<List<ComponentWithId>> tcs)
        {
            self.CollectionName = collectionName;
            self.Tcs = tcs;
            self.parameters = parameters;
        }
    }

    public sealed class DBQueryAllWithIndexTask : DBExTask
    {
        public string CollectionName { get; set; }

        public ETTaskCompletionSource<List<ComponentWithId>> Tcs { get; set; }

        public override async ETTask Run()
        {
            DBEXComponent dbComponent = Game.Scene.GetComponent<DBEXComponent>();
            try
            {
                // 执行查询数据库任务
                var filter = CreateFilter();
                IAsyncCursor<ComponentWithId> cursor = await dbComponent.GetCollection<ComponentWithId>(this.CollectionName).FindAsync(filter);
                List<ComponentWithId> component = await cursor.ToListAsync();
                this.Tcs.SetResult(component);
            }
            catch (Exception e)
            {
                this.Tcs.SetException(new Exception($"查询数据库异常! {CollectionName} {Id}", e));
            }
        }
    }
}
