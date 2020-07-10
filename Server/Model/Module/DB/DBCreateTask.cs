using System;
using System.Collections.Generic;
using System.Reflection;
using MongoDB.Driver;

namespace ETModel
{
    [ObjectSystem]
    public class DBCreateSystem : AwakeSystem<DBCreateTask, string, ComponentWithId, ETTaskCompletionSource<ComponentWithId>>
    {
        public override void Awake(DBCreateTask self, string collectionName, ComponentWithId entity, ETTaskCompletionSource<ComponentWithId> tcs)
        {
            self.CollectionName = collectionName;
            self.Tcs = tcs;
            self.entity = entity;
        }
    }

    public sealed class DBCreateTask : DBTask 
    {
        public string CollectionName { get; set; }

        public ComponentWithId entity { get; set; }

        public ETTaskCompletionSource<ComponentWithId> Tcs { get; set; }

        public override async ETTask Run()
        {
            DBEXComponent dbComponent = Game.Scene.GetComponent<DBEXComponent>();
            try
            {
                await dbComponent.GetCollection<ComponentWithId>(this.CollectionName).InsertOneAsync(entity);
                this.Tcs.SetResult(entity);
            }
            catch (Exception e)
            {
                this.Tcs.SetException(new Exception($"inserts document failed on collection = {CollectionName} with id = {entity.Id}", e));
            }
        }
    }
}
