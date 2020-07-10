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
	public class DBUpgrade_4 : DBUpgradeScriptBase
	{
		public override int step { get => 4; }

        private List<BsonDocument> rideRecords = new List<BsonDocument>();

        private List<BsonDocument> rideTeamRecords = new List<BsonDocument>();

        protected override async ETTask _Run()
		{
            rideRecords.Clear();
            rideTeamRecords.Clear();

            // 找出所有騎乘紀錄
            var command = new BsonDocument
            {
                { "find", "RideRecord" },
                { "projection", new BsonDocument
                    {
                        { "createAt", 1 },
                    }
                },
            };

            var result = await db.database.RunCommandAsync<BsonDocument>(command);
            List<BsonDocument> data = BsonSerializer.Deserialize<List<BsonDocument>>(result["cursor"]["firstBatch"].ToJson());
            for(int i = 0; i < data.Count; i++)
            {
                var rideRecord = data[i];
                var datetime = DateHelper.TimestampMillisecondToDateTimeUTC(rideRecord["createAt"].AsInt64);
                if(datetime.Year > 3000)
                {
                    // 轉換為正確的時間(毫秒)
                    var milisec = DateHelper.DateTimeUTCToTimestampMillisecond(new DateTime(rideRecord["createAt"].AsInt64 * TimeSpan.TicksPerMillisecond));
                    rideRecords.Add(new BsonDocument
                    {
                        {
                            "q", new BsonDocument
                            {
                                { "_id", rideRecord["_id"].AsInt64 }
                            }
                        },
                        {
                            "u", new BsonDocument
                            {
                                {
                                    "$set", new BsonDocument
                                    {
                                        { "createAt", milisec },
                                    }
                                }
                            }
                        },
                        {
                            "upsert", false
                        },
                        {
                            "multi", false
                        }
                    });
                }
            }

            // 找出所有騎乘組隊紀錄
            command = new BsonDocument
            {
                { "find", "RideTeamRecord" },
                { "projection", new BsonDocument
                    {
                        { "createAt", 1 },
                    }
                },
            };
            result = await db.database.RunCommandAsync<BsonDocument>(command);
            data = BsonSerializer.Deserialize<List<BsonDocument>>(result["cursor"]["firstBatch"].ToJson());
            for (int i = 0; i < data.Count; i++)
            {
                var rideTeamRecord = data[i];
                var datetime = DateHelper.TimestampMillisecondToDateTimeUTC(rideTeamRecord["createAt"].AsInt64);
                if (datetime.Year > 3000)
                {
                    // 轉換為正確的時間(毫秒)
                    var milisec = DateHelper.DateTimeUTCToTimestampMillisecond(new DateTime(rideTeamRecord["createAt"].AsInt64 * TimeSpan.TicksPerMillisecond));
                    rideTeamRecords.Add(new BsonDocument
                    {
                        {
                            "q", new BsonDocument
                            {
                                { "_id", rideTeamRecord["_id"].AsInt64 }
                            }
                        },
                        {
                            "u", new BsonDocument
                            {
                                {
                                    "$set", new BsonDocument
                                    {
                                        { "createAt", milisec },
                                    }
                                }
                            }
                        },
                        {
                            "upsert", false
                        },
                        {
                            "multi", false
                        }
                    });
                }
            }

            if (rideRecords.Count != 0)
            {
                // 更新騎乘時間
                command = new BsonDocument
                {
                    { "update", "RideRecord" },
                    { "updates", new BsonArray(rideRecords) },
                    { "ordered", false },
                    { "writeConcern", new BsonDocument
                        {
                            { "w", "majority" },
                            { "wtimeout", 5000 }
                        }
                    }
                };
                result = await db.database.RunCommandAsync<BsonDocument>(command);
                Console.WriteLine($"update RideTeamRecord> {result.ToJson()}");
            }

            if(rideTeamRecords.Count != 0)
            {
                // 更新組隊紀錄時間
                command = new BsonDocument
                {
                    { "update", "RideTeamRecord" },
                    { "updates", new BsonArray(rideTeamRecords) },
                    { "ordered", false },
                    { "writeConcern", new BsonDocument
                        {
                            { "w", "majority" },
                            { "wtimeout", 5000 }
                        }
                    }
                };
                result = await db.database.RunCommandAsync<BsonDocument>(command);
                Console.WriteLine($"update RideTeamRecord> {result.ToJson()}");
            }
        }

		protected override async ETTask<bool> _IsValid()
		{
            var command = new BsonDocument
            {
                { "find", "RideRecord" },
                { "projection", new BsonDocument
                    {
                        { "createAt", 1 },
                    }
                },
            };
            BsonDocument results = await db.database.RunCommandAsync<BsonDocument>(command);
            List<BsonDocument> records = BsonSerializer.Deserialize<List<BsonDocument>>(results["cursor"]["firstBatch"].ToJson());
            if (records.Count == 0)
            {
                return true;
            }

            if(records.Any(doc => DateHelper.TimestampMillisecondToDateTimeUTC(doc["createAt"].AsInt64).Year > 3000))
            {
                failedReason = $"field 'DBSchema.RideRecord.createAt' is invalid datetime format!";
                return false;
            }

            command = new BsonDocument
            {
                { "find", "RideTeamRecord" },
                { "projection", new BsonDocument
                    {
                        { "createAt", 1 },
                    }
                },
            };
            results = await db.database.RunCommandAsync<BsonDocument>(command);
            records = BsonSerializer.Deserialize<List<BsonDocument>>(results["cursor"]["firstBatch"].ToJson());
            if(records.Any(doc => DateHelper.TimestampMillisecondToDateTimeUTC(doc["createAt"].AsInt64).Year > 3000))
            {
                failedReason = $"field 'DBSchema.RideTeamRecord.createAt' is invalid datetime format!";
                return false;
            }
            return true;
        }
	}
}
