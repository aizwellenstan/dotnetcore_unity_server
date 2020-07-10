using System.Collections.Generic;
using MongoDB.Driver;
using System;
using System.Linq;
using MongoDB.Bson;
using System.Reflection;

namespace ETModel
{
	[ObjectSystem]
	public class DBEXComponentSystem : AwakeSystem<DBEXComponent>
	{
		public override void Awake(DBEXComponent self)
		{
			self.Awake();
		}
	}

	/// <summary>
	/// 用来缓存数据
	/// </summary>
	public class DBEXComponent : Component
	{
		public MongoClient mongoClient;
		public IMongoDatabase database;
		
		public const int taskCount = 32;
		public List<DBTaskQueue> tasks = new List<DBTaskQueue>(taskCount);

		public void Awake()
		{
            BuildConnection();
            BuildDBSchema();

            for (int i = 0; i < taskCount; ++i)
			{
				DBTaskQueue taskQueue = ComponentFactory.Create<DBTaskQueue>();
				this.tasks.Add(taskQueue);
			}
		}
		
		public IMongoCollection<ComponentWithId> GetCollection(string name)
		{
			return this.GetCollection<ComponentWithId>(name);
		}

        public IMongoCollection<T> GetCollection<T>(string name)
        {
            return this.database.GetCollection<T>(name);
        }

        public ETTask Add(ComponentWithId component, string collectionName = "")
		{
			ETTaskCompletionSource tcs = new ETTaskCompletionSource();

			if (string.IsNullOrEmpty(collectionName))
			{
				collectionName = component.GetType().Name;
			}
			DBSaveTask task = ComponentFactory.CreateWithId<DBSaveTask, ComponentWithId, string, ETTaskCompletionSource>(component.Id, component, collectionName, tcs);
			this.tasks[(int)((ulong)task.Id % taskCount)].Add(task);

			return tcs.Task;
		}

		public ETTask AddBatch(List<ComponentWithId> components, string collectionName)
		{
			ETTaskCompletionSource tcs = new ETTaskCompletionSource();
			DBSaveBatchTask task = ComponentFactory.Create<DBSaveBatchTask, List<ComponentWithId>, string, ETTaskCompletionSource>(components, collectionName, tcs);
			this.tasks[(int)((ulong)task.Id % taskCount)].Add(task);
			return tcs.Task;
		}

		public ETTask<ComponentWithId> Get(string collectionName, long id)
		{
			ETTaskCompletionSource<ComponentWithId> tcs = new ETTaskCompletionSource<ComponentWithId>();
			DBQueryTask dbQueryTask = ComponentFactory.CreateWithId<DBQueryTask, string, ETTaskCompletionSource<ComponentWithId>>(id, collectionName, tcs);
			this.tasks[(int)((ulong)id % taskCount)].Add(dbQueryTask);

			return tcs.Task;
		}

		public ETTask<List<ComponentWithId>> GetBatch(string collectionName, List<long> idList)
		{
			ETTaskCompletionSource<List<ComponentWithId>> tcs = new ETTaskCompletionSource<List<ComponentWithId>>();
			DBQueryBatchTask dbQueryBatchTask = ComponentFactory.Create<DBQueryBatchTask, List<long>, string, ETTaskCompletionSource<List<ComponentWithId>>>(idList, collectionName, tcs);
			this.tasks[(int)((ulong)dbQueryBatchTask.Id % taskCount)].Add(dbQueryBatchTask);

			return tcs.Task;
		}
		
		public ETTask<List<ComponentWithId>> GetJson(string collectionName, string json)
		{
			ETTaskCompletionSource<List<ComponentWithId>> tcs = new ETTaskCompletionSource<List<ComponentWithId>>();
			
			DBQueryJsonTask dbQueryJsonTask = ComponentFactory.Create<DBQueryJsonTask, string, string, ETTaskCompletionSource<List<ComponentWithId>>>(collectionName, json, tcs);
			this.tasks[(int)((ulong)dbQueryJsonTask.Id % taskCount)].Add(dbQueryJsonTask);

			return tcs.Task;
		}

        public async ETTask<T> FindOneByUniqueIndex<T>(string indexName, T entity, string collectionName = default) where T : ComponentWithId
        {
            Type type = typeof (T);
            if (string.IsNullOrEmpty(collectionName))
            {
                collectionName = type.Name.ToLower();
            }
            ETTaskCompletionSource<ComponentWithId> tcs = new ETTaskCompletionSource<ComponentWithId>();
            var parameters = DBHelper.GetParameterList(type, indexName, entity);
            DBQueryWithUniqueIndexTask task = ComponentFactory.Create<DBQueryWithUniqueIndexTask, string, List<Tuple<PropertyInfo, object>>, ETTaskCompletionSource<ComponentWithId>>
                    (collectionName, parameters, tcs);
            this.tasks[(int)((ulong)task.Id % taskCount)].Add(task);
            return (T)await tcs.Task;
        }

        //public static ETTask<List<T>> FindAllByIndex<T>(this DBComponent db, string collectionName, string indexName, T entity) where T : ComponentWithId
        //{
        //    ETTaskCompletionSource<List<T>> tcs = new ETTaskCompletionSource<List<T>>();
        //    var parameters = DBHelper.GetParameterList(typeof(T), indexName, entity);
        //    DBQueryAllWithIndexTask<T> task = ComponentFactory.Create<DBQueryAllWithIndexTask<T>, string, List<Tuple<PropertyInfo, object>>, ETTaskCompletionSource<List<T>>>
        //        (collectionName, parameters, tcs);
        //    db.tasks[(int)((ulong)task.Id % DBComponent.taskCount)].Add(task);
        //    return tcs.Task;
        //}

	    public async ETTask<T> Create<T>(T entity, string collectionName = default) where T : ComponentWithId
	    {
	        if (string.IsNullOrEmpty(collectionName))
	        {
	            collectionName = typeof (T).Name.ToLower();
	        }
	        ETTaskCompletionSource<ComponentWithId> tcs = new ETTaskCompletionSource<ComponentWithId>();
	        DBCreateTask task = ComponentFactory.Create<DBCreateTask, string, ComponentWithId, ETTaskCompletionSource<ComponentWithId>>
	                (collectionName, entity, tcs);
	        this.tasks[(int)((ulong)task.Id % taskCount)].Add(task);
	        return (T)await tcs.Task;
	    }

        //public static ETTask<T> Update<T>(this DBComponent db, string collectionName, T entity) where T : ComponentWithId
        //{
        //    ETTaskCompletionSource<T> tcs = new ETTaskCompletionSource<T>();
        //    DBUpdateTask<T> task = ComponentFactory.Create<DBUpdateTask<T>, string, T, ETTaskCompletionSource<T>>
        //        (collectionName, entity, tcs);
        //    db.tasks[(int)((ulong)task.Id % DBComponent.taskCount)].Add(task);
        //    return tcs.Task;
        //}

        //public static ETTask<T> Upsert<T>(this DBComponent db, string collectionName, T entity) where T : ComponentWithId
        //{
        //    ETTaskCompletionSource<T> tcs = new ETTaskCompletionSource<T>();
        //    DBUpsertTask<T> task = ComponentFactory.Create<DBUpsertTask<T>, string, T, ETTaskCompletionSource<T>>
        //        (collectionName, entity, tcs);
        //    db.tasks[(int)((ulong)task.Id % DBComponent.taskCount)].Add(task);
        //    return tcs.Task;
        //}

        //public static ETTask<bool> Delete<T>(this DBComponent db, string collectionName, long id) where T : ComponentWithId
        //{
        //    ETTaskCompletionSource<bool> tcs = new ETTaskCompletionSource<bool>();
        //    DBDeleteTask<T> task = ComponentFactory.CreateWithId<DBDeleteTask<T>, string, ETTaskCompletionSource<bool>>
        //        (id, collectionName, tcs);
        //    db.tasks[(int)((ulong)task.Id % DBComponent.taskCount)].Add(task);
        //    return tcs.Task;
        //}

        /// <summary>
        /// 建立連線
        /// </summary>
        private void BuildConnection()
        {
            DBConfig config = StartConfigComponent.Instance.StartConfig.GetComponent<DBConfig>();
            string connectionString = config.ConnectionString;
            mongoClient = new MongoClient(connectionString);
            this.database = this.mongoClient.GetDatabase(config.DBName);
        }

        /// <summary>
        /// 建立資料庫結構
        /// </summary>
        private void BuildDBSchema()
        {
            var collectionNames = this.database.ListCollectionNames().ToEnumerable().Select(e => e.ToLower()).ToArray();
            foreach (var pair in DBHelper.GetDBSchemaIndicesIter())
            {
                var collectionName = pair.Key.Name.ToLower();
                if (!collectionNames.Contains(collectionName))
                {
                    //建立表
                    this.database.CreateCollection(collectionName);
                }
                else
                {
                    DBSchemaAttribute dbArre = pair.Key.GetCustomAttribute<DBSchemaAttribute>();
                    if (!dbArre.isNeedToAlter)
                    {
                        continue;
                    }
                }
                var collection = database.GetCollection<ComponentWithId>(collectionName);
                //先刪除全部索引
                collection.Indexes.DropAll();
                //建立索引
                foreach (var index in pair.Value)
                {
                    var options = new CreateIndexOptions() { Unique = index.Value.isUnique };
                    var indexDoc = new BsonDocument();
                    foreach(var column in index.Value.dBIndexOrders)
                    {
                        indexDoc[column.columnName] = (int)column.order;
                    }
                    IndexKeysDefinition<ComponentWithId> keyCode = indexDoc;
                    var codeIndexModel = new CreateIndexModel<ComponentWithId>(keyCode, options);
                    collection.Indexes.CreateOne(codeIndexModel);
                }
            }
        }
	}
}
