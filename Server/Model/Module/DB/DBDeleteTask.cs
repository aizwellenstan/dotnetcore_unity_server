using System;
using System.Collections.Generic;
using System.Reflection;
using MongoDB.Driver;

namespace ETModel
{
    [ObjectSystem]
    public class DBDeleteSystem : AwakeSystem<DBDeleteTask<ComponentWithId>, string, ETTaskCompletionSource<bool>>
    {
        public override void Awake(DBDeleteTask<ComponentWithId> self, string collectionName, ETTaskCompletionSource<bool> tcs)
        {
            self.CollectionName = collectionName;
            self.Tcs = tcs;
        }
    }

    public sealed class DBDeleteTask<T> : DBTask where T : ComponentWithId
    {
        public string CollectionName { get; set; }

        public ETTaskCompletionSource<bool> Tcs { get; set; }

        public override async ETTask Run()
        {
            DBComponent dbComponent = Game.Scene.GetComponent<DBComponent>();
            try
            {
                var result = await dbComponent.GetCollection(this.CollectionName).DeleteOneAsync(s => s.Id == this.Id);
                //enforces to throw a exception when 'result.IsAcknowledged' is equal to false
                this.Tcs.SetResult(result.DeletedCount > 0);
            }
            catch (Exception e)
            {
                this.Tcs.SetException(new Exception($"deletes document failed on collection = {CollectionName} with id = {this.Id}", e));
            }
        }
    }
}
