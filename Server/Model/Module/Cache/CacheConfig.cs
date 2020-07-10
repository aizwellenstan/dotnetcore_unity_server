using MongoDB.Bson.Serialization.Attributes;

namespace ETModel
{
	[BsonIgnoreExtraElements]
	public class CacheConfig : AConfigComponent
	{
		public string ConnectionString { get; set; }
	}
}