using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using ETModel;

namespace ETHotfix
{
    [DBUpgradeScript]
    public class DBUpgrade_1 : DBUpgradeScriptBase
	{
		public override int step { get => 1; }

		protected override async ETTask _Run()
		{
            var command = new BsonDocument
            {
                { "update", "User" },
                { "updates", new BsonArray
                    {
                        new BsonDocument
                        {
                            {
                                "q", new BsonDocument
                                {

                                }
                            },
                            {
                                "u", new BsonDocument
                                {
                                    {
                                        "$set", new BsonDocument
                                        {
                                            { "identity", User.Identity.Player },
                                        }
                                    }
                                }
                            },
                            {
                                "upsert", false
                            },
                            {
                                "multi", true
                            }
                        }
                    }
                },
            };
            var result = await db.database.RunCommandAsync<BsonDocument>(command);
            Console.WriteLine(result.ToJson());
        }

		protected override async ETTask<bool> _IsValid()
		{
            var command = new BsonDocument
            {
                { "find", "User" }
            };
            BsonDocument results = await db.database.RunCommandAsync<BsonDocument>(command);
            List<BsonDocument> users = BsonSerializer.Deserialize<List<BsonDocument>>(results["cursor"]["firstBatch"].ToJson());
            if (users.Count == 0)
            {
                return true;
            }
            bool valid = users.All(e => 
            {
                if(e.TryGetValue("identity", out BsonValue val))
                {
                    return Enum.IsDefined(typeof(User.Identity), val.ToInt32());
                }
                return false;
            });
            if (!valid)
            {
                failedReason = $"DBSchema.User.identity with invalid enum define or no the element on document!";
            }
            return valid;
        }
	}
}
