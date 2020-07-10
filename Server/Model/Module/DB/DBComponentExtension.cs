using System;
using System.Collections.Generic;
using System.Reflection;
using MongoDB.Driver;

namespace ETModel
{
    public static class DBComponentExtension
    {
        public static IMongoCollection<T> GetCollection<T>(this DBComponent db, string name)
        {
            return db.GetCollection<T>(name);
        }

        public static ETTask<T> FindOneByUniqueIndex<T>(this DBComponent db, string collectionName, string indexName, T entity) where T : ComponentWithId
        {
            ETTaskCompletionSource<T> tcs = new ETTaskCompletionSource<T>();
            var parameters = DBHelper.GetParameterList(typeof(T), indexName, entity);
            DBQueryWithUniqueIndexTask task = ComponentFactory.Create<DBQueryWithUniqueIndexTask, string, List<Tuple<PropertyInfo, object>>, ETTaskCompletionSource<T>>
                (collectionName, parameters, tcs);
            db.tasks[(int)((ulong)task.Id % DBComponent.taskCount)].Add(task);
            return tcs.Task;
        }

        public static ETTask<List<T>> FindAllByIndex<T>(this DBComponent db, string collectionName, string indexName, T entity) where T : ComponentWithId
        {
            ETTaskCompletionSource<List<T>> tcs = new ETTaskCompletionSource<List<T>>();
            var parameters = DBHelper.GetParameterList(typeof(T), indexName, entity);
            DBQueryAllWithIndexTask task = ComponentFactory.Create<DBQueryAllWithIndexTask, string, List<Tuple<PropertyInfo, object>>, ETTaskCompletionSource<List<T>>>
                (collectionName, parameters, tcs);
            db.tasks[(int)((ulong)task.Id % DBComponent.taskCount)].Add(task);
            return tcs.Task;
        }

        public static ETTask<T> Create<T>(this DBComponent db, string collectionName, T entity) where T : ComponentWithId
        {
            ETTaskCompletionSource<T> tcs = new ETTaskCompletionSource<T>();
            DBCreateTask task = ComponentFactory.Create<DBCreateTask, string, T, ETTaskCompletionSource<T>>
                (collectionName, entity, tcs);
            db.tasks[(int)((ulong)task.Id % DBComponent.taskCount)].Add(task);
            return tcs.Task;
        }

        public static ETTask<T> Update<T>(this DBComponent db, string collectionName, T entity) where T : ComponentWithId
        {
            ETTaskCompletionSource<T> tcs = new ETTaskCompletionSource<T>();
            DBUpdateTask<T> task = ComponentFactory.Create<DBUpdateTask<T>, string, T, ETTaskCompletionSource<T>>
                (collectionName, entity, tcs);
            db.tasks[(int)((ulong)task.Id % DBComponent.taskCount)].Add(task);
            return tcs.Task;
        }

        public static ETTask<T> Upsert<T>(this DBComponent db, string collectionName, T entity) where T : ComponentWithId
        {
            ETTaskCompletionSource<T> tcs = new ETTaskCompletionSource<T>();
            DBUpsertTask<T> task = ComponentFactory.Create<DBUpsertTask<T>, string, T, ETTaskCompletionSource<T>>
                (collectionName, entity, tcs);
            db.tasks[(int)((ulong)task.Id % DBComponent.taskCount)].Add(task);
            return tcs.Task;
        }

        public static ETTask<bool> Delete<T>(this DBComponent db, string collectionName, long id) where T : ComponentWithId
        {
            ETTaskCompletionSource<bool> tcs = new ETTaskCompletionSource<bool>();
            DBDeleteTask<T> task = ComponentFactory.CreateWithId<DBDeleteTask<T>, string, ETTaskCompletionSource<bool>>
                (id, collectionName, tcs);
            db.tasks[(int)((ulong)task.Id % DBComponent.taskCount)].Add(task);
            return tcs.Task;
        }
    }
}
