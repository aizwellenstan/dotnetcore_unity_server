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
	public class DBUpgrade_3 : DBUpgradeScriptBase
	{
		public override int step { get => 3; }

        private List<BsonDocument> rideRecords = new List<BsonDocument>();

        private Dictionary<string, BsonDocument> teamRecordMap = new Dictionary<string, BsonDocument>();

        protected override async ETTask _Run()
		{
            rideRecords.Clear();
            teamRecordMap.Clear();

            // 找出所有使用者
            var command = new BsonDocument
            {
                { "find", "User" },
                { "projection", new BsonDocument
                    {
                        { "playerRideTotalInfo", 1 }
                    }
                },
            };

            var result = await db.database.RunCommandAsync<BsonDocument>(command);
            List<BsonDocument> users = BsonSerializer.Deserialize<List<BsonDocument>>(result["cursor"]["firstBatch"].ToJson());
            for(int i = 0; i < users.Count; i++)
            {
                var user = users[i];
                var array = user["playerRideTotalInfo"]["RideRoadInfos"].AsBsonArray;
                for(int j = 0; j < array.Count; j++)
                {
                    var element = array[j];
                    var rideRecord = new BsonDocument();
                    rideRecord["_id"] = element["InfoId"].AsInt64;
                    rideRecord["_t"] = "RideRecord";
                    rideRecord["C"] = new BsonArray();
                    rideRecord["uid"] = user["_id"].AsInt64;
                    rideRecord["roadConfigId"] = element["RoadId"].AsInt64;
                    rideRecord["rank"] = element["Rank"].AsInt32;
                    rideRecord["mileage"] = element["Mileage"].AsInt64;
                    rideRecord["cumulativeSecond"] = element["CumulativeTime"].AsInt64;
                    rideRecord["averageSpeed"] = element["AverageSpeed"].AsDouble;
                    rideRecord["topSpeed"] = element["TopSpeed"].AsDouble;
                    rideRecord["calories"] = element["Calories"].AsDouble;
                    rideRecord["power"] = element["Power"].AsDouble;
                    rideRecord["createAt"] = element["RecordUTCTick"].AsInt64 / TimeSpan.TicksPerMillisecond;
                    rideRecord["rideType"] = (int)RideRecord.RideType.Party;
                    rideRecord["teamId"] = IdGenerater.GenerateId();

                    rideRecords.Add(rideRecord);

                    var arr = element["BattleLeaderboardUnitInfos"].AsBsonArray;
                    if(arr.Count > 0)
                    {
                        // 整理需要寫入到DB的組隊資訊
                        teamRecordMap.TryAdd(arr.ToJson(), new BsonDocument
                        {
                            { "_id", rideRecord["teamId"].AsInt64 },
                            { "_t", "RideTeamRecord" },
                            { "C", new BsonArray() },
                            { "members", new BsonArray(arr.Select(e => new BsonDocument
                            {
                                { "uid", e["Uid"].AsInt64 },
                                { "name", e["Name"].AsString },
                                { "traveledDistance", e["DistanceTravelledTarget"].AsDouble },
                                { "location", e["Location"].AsInt32 }
                            })) },
                            { "createAt", rideRecord["createAt"].AsInt64 }
                        });
                    }
                }
            }

            if(rideRecords.Count != 0)
            {
                // 依時間戳排序競賽紀錄
                rideRecords.Sort((x, y) => x["createAt"].AsInt64.CompareTo(y["createAt"].AsInt64));
                // 寫入競賽紀錄到DB
                command = new BsonDocument
                {
                    { "insert", "RideRecord" },
                    { "documents", new BsonArray(rideRecords) },
                    { "ordered", false },
                    { "writeConcern", new BsonDocument
                        {
                            { "w", "majority" },
                            { "wtimeout", 5000 }
                        }
                    }
                };
                result = await db.database.RunCommandAsync<BsonDocument>(command);
                Console.WriteLine($"insert into RideRecord> {result.ToJson()}");
            }

            if(teamRecordMap.Count != 0)
            {
                // 依時間戳排序隊伍紀錄
                var teamList = teamRecordMap.Values.ToList();
                teamList.Sort((x, y) => x["createAt"].AsInt64.CompareTo(y["createAt"].AsInt64));
                // 寫入隊伍紀錄到DB
                command = new BsonDocument
                {
                    { "insert", "RideTeamRecord" },
                    { "documents", new BsonArray(teamList) },
                    { "ordered", false },
                    { "writeConcern", new BsonDocument
                        {
                            { "w", "majority" },
                            { "wtimeout", 5000 }
                        }
                    }
                };
                result = await db.database.RunCommandAsync<BsonDocument>(command);
                Console.WriteLine($"insert into RideTeamRecord> {result.ToJson()}");
            }

            // 移除過時的欄位
            command = new BsonDocument
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
                                        "$unset", new BsonDocument
                                        {
                                            {
                                                "playerRideTotalInfo.RideRoadInfos", 1
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
            result = await db.database.RunCommandAsync<BsonDocument>(command);
            Console.WriteLine($"remove field from User> {result.ToJson()}");
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

            if(users.Any(user => user["playerRideTotalInfo"].AsBsonDocument.Contains("RideRoadInfos")))
            {
                failedReason = $"field 'DBSchema.User.RideRoadInfos' is not removed on DBSchema.User!";
                return false;
            }

            command = new BsonDocument
            {
                { "find", "RideRecord" }
            };
            results = await db.database.RunCommandAsync<BsonDocument>(command);
            List<BsonDocument> records = BsonSerializer.Deserialize<List<BsonDocument>>(results["cursor"]["firstBatch"].ToJson());
            var ids = rideRecords.Select(e => e["_id"].AsInt64).ToList();
            if(ids.Any(e => records.FirstOrDefault(ee => ee["_id"].AsInt64 == e) == null))
            {
                failedReason = $"to insert data into DBSchema.RideRecord is failed!";
                return false;
            }

            command = new BsonDocument
            {
                { "find", "RideTeamRecord" }
            };
            results = await db.database.RunCommandAsync<BsonDocument>(command);
            records = BsonSerializer.Deserialize<List<BsonDocument>>(results["cursor"]["firstBatch"].ToJson());
            ids = teamRecordMap.Values.Select(e => e["_id"].AsInt64).ToList();
            if (ids.Any(e => records.FirstOrDefault(ee => ee["_id"].AsInt64 == e) == null))
            {
                failedReason = $"to insert data into DBSchema.RideTeamRecord is failed!";
                return false;
            }
            return true;
        }
	}
}
