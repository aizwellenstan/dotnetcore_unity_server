using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using MongoDB.Driver;

namespace ETModel
{
    [ObjectSystem]
    public class DBQueryWithUniqueIndexSystem : AwakeSystem<DBQueryWithUniqueIndexTask, string, List<Tuple<PropertyInfo, object>>, ETTaskCompletionSource<ComponentWithId>>
    {
        public override void Awake(DBQueryWithUniqueIndexTask self, string collectionName, List<Tuple<PropertyInfo, object>> parameters, ETTaskCompletionSource<ComponentWithId> tcs)
        {
            self.CollectionName = collectionName;
            self.Tcs = tcs;
            self.parameters = parameters;
        }
    }

    public sealed class DBQueryWithUniqueIndexTask : DBExTask
    {
        public string CollectionName { get; set; }

        public ETTaskCompletionSource<ComponentWithId> Tcs { get; set; }

        public override async ETTask Run()
        {
            DBEXComponent dbComponent = Game.Scene.GetComponent<DBEXComponent>();
            try
            {
                var filter = CreateFilter();
                IAsyncCursor<ComponentWithId> cursor = await dbComponent.GetCollection<ComponentWithId>(this.CollectionName).FindAsync(filter);
                ComponentWithId component = await cursor.FirstOrDefaultAsync();
                this.Tcs.SetResult(component);
            }
            catch (Exception e)
            {
                this.Tcs.SetException(new Exception($"find one failed on collection = {CollectionName} with" +
                    $" {GetCondition()}", e));
            }
        }
    }
}
