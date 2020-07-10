using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MongoDB.Bson;
using MongoDB.Driver;

namespace ETModel
{
	[ObjectSystem]
	public class DBComponentAwakeSystem : AwakeSystem<DBComponent>
	{
		public override void Awake(DBComponent self)
		{
			self.Awake();
		}
	}

    [ObjectSystem]
    public class DBComponentDestroySystem : DestroySystem<DBComponent>
    {
        public override void Destroy(DBComponent self)
        {
            self.Destroy();
        }
    }

    /// <summary>
    /// 用来缓存数据
    /// </summary>
    public class DBComponent : Component
	{
		public MongoClient mongoClient { private set; get; }

		public IMongoDatabase database { private set; get; }

		public const int taskCount = 32;
		public List<DBTaskQueue> tasks { private set; get; } = new List<DBTaskQueue>(taskCount);

		public void Awake()
		{
			DBConfig config = StartConfigComponent.Instance.StartConfig.GetComponent<DBConfig>();
			string connectionString = config.ConnectionString;
			mongoClient = new MongoClient(connectionString);
			this.database = this.mongoClient.GetDatabase(config.DBName);
			
			for (int i = 0; i < taskCount; ++i)
			{
				DBTaskQueue taskQueue = ComponentFactory.Create<DBTaskQueue>();
				this.tasks.Add(taskQueue);
			}
		    BuildDBSchema();
		}

        public void Destroy()
        {
            
        }
		
		public IMongoCollection<ComponentWithId> GetCollection(string name)
		{
			return this.database.GetCollection<ComponentWithId>(name);
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

        public ETTask<List<ComponentWithId>> GetJson(string collectionName, string json, long skip, long limit)
        {
            ETTaskCompletionSource<List<ComponentWithId>> tcs = new ETTaskCompletionSource<List<ComponentWithId>>();
            PipelineStageDefinition<ComponentWithId, ComponentWithId> _match = PipelineStageDefinitionBuilder.Match<ComponentWithId>(json);
            PipelineStageDefinition<ComponentWithId, ComponentWithId> _skip = PipelineStageDefinitionBuilder.Skip<ComponentWithId>((int)skip);
            PipelineStageDefinition<ComponentWithId, ComponentWithId> _limit = PipelineStageDefinitionBuilder.Limit<ComponentWithId>((int)limit);
            PipelineDefinition<ComponentWithId, ComponentWithId> pipeline = new PipelineStagePipelineDefinition<ComponentWithId, ComponentWithId>(
                new PipelineStageDefinition<ComponentWithId, ComponentWithId>[]{ _match, _skip, _limit });
            DBQueryPipelineTask dbQueryPipelineTask = ComponentFactory.Create<DBQueryPipelineTask, string, PipelineDefinition<ComponentWithId, ComponentWithId>, ETTaskCompletionSource<List<ComponentWithId>>>
                (collectionName, pipeline, tcs);
            this.tasks[(int)((ulong)dbQueryPipelineTask.Id % taskCount)].Add(dbQueryPipelineTask);
            return tcs.Task;
        }

        public ETTask<long> GetCountByJson(string collectionName, string json)
        {
            ETTaskCompletionSource<long> tcs = new ETTaskCompletionSource<long>();
            DBQueryCountTask dbQueryCountTask = ComponentFactory.Create<DBQueryCountTask, string, string, ETTaskCompletionSource<long>>(collectionName, json, tcs);
            this.tasks[(int)((ulong)dbQueryCountTask.Id % taskCount)].Add(dbQueryCountTask);
            return tcs.Task;
        }

        public ETTask DeleteJson(string collectionName, string json)
        {
            ETTaskCompletionSource tcs = new ETTaskCompletionSource();
            DBDeleteJsonTask dbDeleteJsonTask = ComponentFactory.Create<DBDeleteJsonTask, string , string, ETTaskCompletionSource>(collectionName, json, tcs);
            this.tasks[(int)((ulong)dbDeleteJsonTask.Id % taskCount)].Add(dbDeleteJsonTask);
            return tcs.Task;
        }

        /// <summary>
        /// 建立資料庫結構
        /// </summary>
        private void BuildDBSchema()
        {
            var collectionNames = this.database.ListCollectionNames().ToEnumerable().Select(e => e.ToLower()).ToArray();
            foreach (var pair in DBHelper.GetDBSchemaIndicesIter())
            {
                DBSchemaAttribute dbArre = pair.Key.GetCustomAttribute<DBSchemaAttribute>();
                if (dbArre == null)
                {
                    continue;
                }

                // TODO:表的名稱要統一，這邊先用開頭大寫的規則
                var collectionName = pair.Key.Name;
                if (!collectionNames.Contains(collectionName.ToLower()))
                {
                    // 建立表
                    this.database.CreateCollection(collectionName);
                }
                else
                {
                    if (!dbArre.isNeedToAlter)
                    {
                        continue;
                    }
                }
                var collection = database.GetCollection<ComponentWithId>(collectionName);
                // 先刪除全部索引
                collection.Indexes.DropAll();
                // 建立索引
                foreach (var index in pair.Value)
                {
                    var options = new CreateIndexOptions() { Unique = index.Value.isUnique };
                    var indexDoc = new BsonDocument();
                    foreach (var column in index.Value.dBIndexOrders)
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
