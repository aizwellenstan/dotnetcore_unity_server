using System;
using System.Collections.Generic;
using MongoDB.Driver;

namespace ETModel
{
	[ObjectSystem]
	public class DBQueryCountTaskAwakeSystem : AwakeSystem<DBQueryCountTask, string, string, ETTaskCompletionSource<long>>
	{
		public override void Awake(DBQueryCountTask self, string collectionName, string json, ETTaskCompletionSource<long> tcs)
		{
			self.CollectionName = collectionName;
            self.Json = json;
            self.Tcs = tcs;
		}
	}

	public sealed class DBQueryCountTask : DBTask
	{
		public string CollectionName { get; set; }

        public string Json { get; set; }

        public ETTaskCompletionSource<long> Tcs { get; set; }
		
		public override async ETTask Run()
		{
			DBComponent dbComponent = Game.Scene.GetComponent<DBComponent>();
			try
			{
                FilterDefinition<ComponentWithId> filterDefinition = new JsonFilterDefinition<ComponentWithId>(this.Json);
                long count = await dbComponent.GetCollection(this.CollectionName).CountDocumentsAsync(filterDefinition);
				this.Tcs.SetResult(count);
			}
			catch (Exception e)
			{
				this.Tcs.SetException(new Exception($"查询数据库异常! {CollectionName} {this.Json}", e));
			}
		}
	}
}