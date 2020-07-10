using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using ETHotfix;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace ETModel
{
    //TODO:RelationshipComponent尚未設計完畢
    public class RelationshipComponent : Component
	{
        private static DBComponent db
        {
            get
            {
                return Game.Scene.GetComponent<DBComponent>();
            }
        }

        private Dictionary<long, Relationship> relationshipDict = new Dictionary<long, Relationship>();
        private Dictionary<long, User> userDict = new Dictionary<long, User>();

        public void Awake()
        {
            //List<User> users = await DumpAllUser();
            //List<Relationship> relations = await DumpAllRelationship();
            //userDict = users.ToDictionary(e => e.Id, e => e);
            //relationshipDict = relations.ToDictionary(e => e.uid, e => e);
        }

        //public static List<RelationshipSimpleInfo> GetStrangers(long uid)
        //{

        //}

        /// <summary>
        /// 抓出所有使用者資料
        /// </summary>
        /// <returns></returns>
        public static async ETTask<List<User>> DumpAllUser()
        {
            var json = Expression2Json<User>(entity => true);
            List<ComponentWithId> components = await Game.Scene.GetComponent<DBComponent>().GetJson(typeof(User).Name, json);
            return components.OfType<User>().ToList();
        }

        /// <summary>
        /// 抓出所有關係資料
        /// </summary>
        /// <returns></returns>
        public static async ETTask<List<Relationship>> DumpAllRelationship()
        {
            var json = Expression2Json<Relationship>(entity => true);
            List<ComponentWithId> components = await Game.Scene.GetComponent<DBComponent>().GetJson(typeof(Relationship).Name, json);
            return components.OfType<Relationship>().ToList();
        }

        private static string Expression2Json<T>(Expression<Func<T, bool>> exp)
        {
            ExpressionFilterDefinition<T> filter = new ExpressionFilterDefinition<T>(exp);
            IBsonSerializerRegistry serializerRegistry = BsonSerializer.SerializerRegistry;
            IBsonSerializer<T> documentSerializer = serializerRegistry.GetSerializer<T>();
            string json = filter.Render(documentSerializer, serializerRegistry).ToJson();
            return json;
        }

        public override void Dispose()
		{
			if (this.IsDisposed)
			{
				return;
			}
			base.Dispose();
			foreach (Relationship rel in relationshipDict.Values)
			{
                rel.Dispose();
			}
            relationshipDict.Clear();
		}
    }
}