using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ETModel;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace ETHotfix
{
    [DBUpgradeScript]
	public class DBUpgrade_2 : DBUpgradeScriptBase
	{
		public override int step { get => 2; }

		protected override async ETTask _Run()
		{
            var userBagCapacity = EquipmentDataHelper.GetDefaultUserBag();

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
                                            {
                                                "userBagCapacity", userBagCapacity
                                            },
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
            var configComponent = Game.Scene.GetComponent<ConfigComponent>();
            var bagLimitSettings = configComponent.GetAll(typeof(BagLimitSetting)).OfType<BagLimitSetting>().ToList();
            bool valid = users.All(e => 
            {
                if(e.TryGetValue("userBagCapacity", out BsonValue val))
                {
                    var doc = val.ToBsonDocument();
                    if(doc == null)
                        return false;
                    for (int i = 0; i < bagLimitSettings.Count; i++)
                    {
                        var bagLimitSetting = bagLimitSettings[i];
                        Equipment.EquipmentType equipmentType = (Equipment.EquipmentType)bagLimitSetting.Id;
                        var usedCountString = EquipmentDataHelper.GetUsedCountString(equipmentType);
                        var totalCountString = EquipmentDataHelper.GetTotalCountString(equipmentType);
                        if (doc[usedCountString].AsInt32 < 0 || doc[usedCountString].AsInt32 > doc[totalCountString].AsInt32)
                            return false;
                        if (doc[totalCountString].AsInt32 > bagLimitSetting.MaxSlotCount)
                            return false;
                    }
                    return true;
                }
                return false;
            });
            if (!valid)
            {
                failedReason = $"DBSchema.User.userBagCapacity with invalid slot count or no the element on document!";
            }
            return valid;
        }
	}
}
