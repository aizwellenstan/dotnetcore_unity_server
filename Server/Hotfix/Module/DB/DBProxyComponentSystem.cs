using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using ETModel;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using System.Linq;

namespace ETHotfix
{
	[ObjectSystem]
	public class DbProxyComponentSystem : AwakeSystem<DBProxyComponent>
	{
		public override void Awake(DBProxyComponent self)
		{
			self.Awake();
		}
	}
	
	/// <summary>
	/// 用来与数据库操作代理
	/// </summary>
	public static class DBProxyComponentEx
	{
		public static void Awake(this DBProxyComponent self)
		{
			StartConfig dbStartConfig = StartConfigComponent.Instance.DBConfig;
			self.dbAddress = dbStartConfig.GetComponent<InnerConfig>().IPEndPoint;
		}

		public static async ETTask Save(this DBProxyComponent self, ComponentWithId component)
		{
			Session session = Game.Scene.GetComponent<NetInnerComponent>().Get(self.dbAddress);
			await session.Call(new DBSaveRequest { Component = component });
		}

		public static async ETTask SaveBatch(this DBProxyComponent self, List<ComponentWithId> components)
		{
			Session session = Game.Scene.GetComponent<NetInnerComponent>().Get(self.dbAddress);
			await session.Call(new DBSaveBatchRequest { Components = components });
		}

		public static async ETTask Save(this DBProxyComponent self, ComponentWithId component, CancellationToken cancellationToken)
		{
			Session session = Game.Scene.GetComponent<NetInnerComponent>().Get(self.dbAddress);
			await session.Call(new DBSaveRequest { Component = component }, cancellationToken);
		}

		public static async ETTask SaveLog(this DBProxyComponent self, ComponentWithId component)
		{
			Session session = Game.Scene.GetComponent<NetInnerComponent>().Get(self.dbAddress);
			await session.Call(new DBSaveRequest { Component = component, CollectionName = "DBLog" });
		}

		public static async ETTask SaveLog(this DBProxyComponent self, long uid, DBLog.LogType logType, ComponentWithId record)
		{
			Session session = Game.Scene.GetComponent<NetInnerComponent>().Get(self.dbAddress);
			DBLog dBLog = ComponentFactory.CreateWithId<DBLog>(IdGenerater.GenerateId());
			dBLog.uid = uid;
			dBLog.logType = (int)logType;
            BsonDocument doc = null;
            BsonDocument.TryParse(record.ToJson(), out doc);
            dBLog.document = doc;
            dBLog.createAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
			await self.SaveLog(dBLog);
		}

        public static async ETTask SaveLog(this DBProxyComponent self, long uid, DBLog.LogType logType, BsonDocument record)
        {
            DBLog dBLog = ComponentFactory.CreateWithId<DBLog>(IdGenerater.GenerateId());
            dBLog.uid = uid;
            dBLog.logType = (int)logType;
            dBLog.document = record;
            dBLog.createAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            await self.SaveLog(dBLog);
        }

        public static async ETTask SaveLogBatch(this DBProxyComponent self, long uid, DBLog.LogType logType, List<ComponentWithId> components)
        {
            Session session = Game.Scene.GetComponent<NetInnerComponent>().Get(self.dbAddress);
            List<ComponentWithId> list = new List<ComponentWithId>();
            for(int i = 0; i < components.Count; i++)
            {
                ComponentWithId record = null;
                DBLog dBLog = ComponentFactory.CreateWithId<DBLog>(IdGenerater.GenerateId());
                dBLog.uid = uid;
                dBLog.logType = (int)logType;
                BsonDocument doc = null;
                BsonDocument.TryParse(record.ToJson(), out doc);
                dBLog.document = doc;
                dBLog.createAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                list.Add(dBLog);
            }

            await self.SaveBatch(list);
        }

        public static async ETTask<T> Query<T>(this DBProxyComponent self, long id) where T: ComponentWithId
		{
			Session session = Game.Scene.GetComponent<NetInnerComponent>().Get(self.dbAddress);
			DBQueryResponse dbQueryResponse = (DBQueryResponse)await session.Call(new DBQueryRequest { CollectionName = typeof(T).Name, Id = id });
			return (T)dbQueryResponse.Component;
		}
		
		/// <summary>
		/// 根据查询表达式查询
		/// </summary>
		/// <param name="self"></param>
		/// <param name="exp"></param>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public static async ETTask<List<ComponentWithId>> Query<T>(this DBProxyComponent self, Expression<Func<T ,bool>> exp) where T: ComponentWithId
		{
			ExpressionFilterDefinition<T> filter = new ExpressionFilterDefinition<T>(exp);
			IBsonSerializerRegistry serializerRegistry = BsonSerializer.SerializerRegistry;
			IBsonSerializer<T> documentSerializer = serializerRegistry.GetSerializer<T>();
			string json = filter.Render(documentSerializer, serializerRegistry).ToJson();
			return await self.Query<T>(json);
		}

		public static async ETTask<List<ComponentWithId>> Query<T>(this DBProxyComponent self, List<long> ids) where T : ComponentWithId
		{
			Session session = Game.Scene.GetComponent<NetInnerComponent>().Get(self.dbAddress);
			DBQueryBatchResponse dbQueryBatchResponse = (DBQueryBatchResponse)await session.Call(new DBQueryBatchRequest { CollectionName = typeof(T).Name, IdList = ids });
			return dbQueryBatchResponse.Components;
		}

		/// <summary>
		/// 根据json查询条件查询
		/// </summary>
		/// <param name="self"></param>
		/// <param name="json"></param>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public static async ETTask<List<ComponentWithId>> Query<T>(this DBProxyComponent self, string json) where T : ComponentWithId
		{
			Session session = Game.Scene.GetComponent<NetInnerComponent>().Get(self.dbAddress);
			DBQueryJsonResponse dbQueryJsonResponse = (DBQueryJsonResponse)await session.Call(new DBQueryJsonRequest { CollectionName = typeof(T).Name, Json = json });
			return dbQueryJsonResponse.Components;
		}

        /// <summary>
        /// 根據查詢表達式查詢，並約束結果範圍，再取回相應筆數
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="self"></param>
        /// <param name="exp"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        public static async ETTask<List<ComponentWithId>> Query<T>(this DBProxyComponent self, Expression<Func<T, bool>> exp, long skip, long limit) where T : ComponentWithId
        {
            ExpressionFilterDefinition<T> filter = new ExpressionFilterDefinition<T>(exp);
            IBsonSerializerRegistry serializerRegistry = BsonSerializer.SerializerRegistry;
            IBsonSerializer<T> documentSerializer = serializerRegistry.GetSerializer<T>();
            string json = filter.Render(documentSerializer, serializerRegistry).ToJson();
            return await self.Query<T>(json, skip, limit);
        }

        /// <summary>
        /// 根據json查詢條件查詢，並約束結果範圍，再取回相應筆數
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="self"></param>
        /// <param name="json"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        public static async ETTask<List<ComponentWithId>> Query<T>(this DBProxyComponent self, string json, long skip, long limit) where T : ComponentWithId
        {
            Session session = Game.Scene.GetComponent<NetInnerComponent>().Get(self.dbAddress);
            DBQueryJsonResponse dbQueryJsonResponse = (DBQueryJsonResponse)await session.Call(new DBQueryJsonSkipLimitRequest { CollectionName = typeof(T).Name, Json = json, Skip = skip, Limit = limit });
            return dbQueryJsonResponse.Components;
        }

        /// <summary>
        /// 根據查詢表達式查詢，並返回筆數
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="self"></param>
        /// <param name="exp"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        public static async ETTask<long> QueryCount<T>(this DBProxyComponent self, Expression<Func<T, bool>> exp) where T : ComponentWithId
        {
            ExpressionFilterDefinition<T> filter = new ExpressionFilterDefinition<T>(exp);
            IBsonSerializerRegistry serializerRegistry = BsonSerializer.SerializerRegistry;
            IBsonSerializer<T> documentSerializer = serializerRegistry.GetSerializer<T>();
            string json = filter.Render(documentSerializer, serializerRegistry).ToJson();
            return await self.QueryCount<T>(json);
        }

        /// <summary>
        /// 根據json查詢條件查詢，並返回筆數
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="self"></param>
        /// <param name="json"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        public static async ETTask<long> QueryCount<T>(this DBProxyComponent self, string json) where T : ComponentWithId
        {
            Session session = Game.Scene.GetComponent<NetInnerComponent>().Get(self.dbAddress);
            DBQueryJsonCountResponse dbQueryJsonResponse = (DBQueryJsonCountResponse)await session.Call(new DBQueryJsonCountRequest { CollectionName = typeof(T).Name, Json = json });
            return dbQueryJsonResponse.Count;
        }

        /// <summary>
        /// 根據查詢表達式刪除一筆或多筆紀錄
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="self"></param>
        /// <param name="exp"></param>
        /// <returns></returns>
        public static async ETTask DeleteJson<T>(this DBProxyComponent self, Expression<Func<T, bool>> exp) where T : ComponentWithId
        {
            ExpressionFilterDefinition<T> filter = new ExpressionFilterDefinition<T>(exp);
            IBsonSerializerRegistry serializerRegistry = BsonSerializer.SerializerRegistry;
            IBsonSerializer<T> documentSerializer = serializerRegistry.GetSerializer<T>();
            string json = filter.Render(documentSerializer, serializerRegistry).ToJson();
            await self.DeleteJson<T>(json);
        }

        /// <summary>
        /// 根據json刪除一筆或多筆紀錄
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="self"></param>
        /// <param name="json"></param>
        /// <returns></returns>
        public static async ETTask DeleteJson<T>(this DBProxyComponent self, string json) where T : ComponentWithId
        {
            Session session = Game.Scene.GetComponent<NetInnerComponent>().Get(self.dbAddress);
            DBDeleteJsonResponse dbQueryJsonResponse = (DBDeleteJsonResponse)await session.Call(new DBDeleteJsonRequest { CollectionName = typeof(T).Name, Json = json });
        }
    }
}