﻿using System;
using System.Collections.Generic;
using System.Reflection;
using MongoDB.Driver;

namespace ETModel
{
    [ObjectSystem]
    public class DBUpsertSystem : AwakeSystem<DBUpsertTask<ComponentWithId>, string, ComponentWithId, ETTaskCompletionSource<ComponentWithId>>
    {
        public override void Awake(DBUpsertTask<ComponentWithId> self, string collectionName, ComponentWithId entity, ETTaskCompletionSource<ComponentWithId> tcs)
        {
            self.CollectionName = collectionName;
            self.Tcs = tcs;
            self.entity = entity;
        }
    }

    public sealed class DBUpsertTask<T> : DBTask where T : ComponentWithId
    {
        public string CollectionName { get; set; }

        public T entity { get; set; }

        public ETTaskCompletionSource<T> Tcs { get; set; }

        public override async ETTask Run()
        {
            DBComponent dbComponent = Game.Scene.GetComponent<DBComponent>();
            try
            {
                var result = await dbComponent.GetCollection(this.CollectionName).ReplaceOneAsync(s => s.Id == entity.Id, entity, new UpdateOptions { IsUpsert = true });
                //enforces to throw a exception when 'result.IsAcknowledged' is equal to false
                this.Tcs.SetResult(result.ModifiedCount > 0 ? entity : null);
            }
            catch (Exception e)
            {
                this.Tcs.SetException(new Exception($"upserts document failed on collection = {CollectionName} with id = {entity.Id}", e));
            }
        }
    }
}
