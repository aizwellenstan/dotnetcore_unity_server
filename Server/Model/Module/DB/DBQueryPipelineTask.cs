using System;
using System.Collections.Generic;
using MongoDB.Driver;

namespace ETModel
{
	[ObjectSystem]
	public class DBQueryPipelineTaskAwakeSystem : AwakeSystem<DBQueryPipelineTask, string, PipelineDefinition<ComponentWithId, ComponentWithId>, ETTaskCompletionSource<List<ComponentWithId>>>
	{
		public override void Awake(DBQueryPipelineTask self, string collectionName, PipelineDefinition<ComponentWithId, ComponentWithId> pipeline, ETTaskCompletionSource<List<ComponentWithId>> tcs)
		{
			self.CollectionName = collectionName;
            self.pipeline = pipeline;
            self.Tcs = tcs;
		}
	}

	public sealed class DBQueryPipelineTask : DBTask
	{
		public string CollectionName { get; set; }

        public PipelineDefinition<ComponentWithId, ComponentWithId> pipeline { get; set; }

        public ETTaskCompletionSource<List<ComponentWithId>> Tcs { get; set; }
		
		public override async ETTask Run()
		{
			DBComponent dbComponent = Game.Scene.GetComponent<DBComponent>();
			try
			{
				IAsyncCursor<ComponentWithId> cursor = await dbComponent.GetCollection(this.CollectionName).AggregateAsync(pipeline);
				List<ComponentWithId> components = await cursor.ToListAsync();
				this.Tcs.SetResult(components);
			}
			catch (Exception e)
			{
				this.Tcs.SetException(new Exception($"查询数据库异常! {CollectionName} {this.pipeline.ToString()}", e));
			}
		}
	}
}