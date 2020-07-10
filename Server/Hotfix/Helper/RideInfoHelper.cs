using ETModel;
using Google.Protobuf.Collections;
using MongoDB.Bson;
using System;
using System.Linq;

namespace ETHotfix
{
    public class RideInfoHelper
    {
        public static async void SaveRoadAllInfo(MapUnit mapUnit, long teamId, bool saveRideRoadInfos = true, bool saveRideTotalInfos = true)
        {
            if (mapUnit == null)
            {
                Log.Error("SaveRoadAllInfo Failed, mapUnit == null");
                return;
            }

            if (mapUnit.MapUnitType == MapUnitType.Npc)
                return;

            var roadInfo = mapUnit.CreateRideRecord(teamId);
            var roadTotalInfo = mapUnit.CreateRideTotalInfo();
            var user = await UserDataHelper.FindOneUser(mapUnit.Uid);
            BsonDocument log = null;
            if (saveRideRoadInfos)
                SaveRideRoadInfos(roadInfo);
            if (saveRideTotalInfos)
                SaveRideTotalInfo(user, roadTotalInfo, out log);
            SaveUserAndBroadcastTarget(mapUnit, user, log);
        }

        public static long SaveRideTeamRecord(RepeatedField<BattleLeaderboardUnitInfo> data)
        {
            var record = ComponentFactory.CreateWithId<RideTeamRecord>(IdGenerater.GenerateId());
            record.members = data.Aggregate(new RepeatedField<MemberBrief>(), (list, item) => 
            {
                list.Add(item.ToMemberBrief());
                return list;
            });
            record.createAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            UserDataHelper.UpsertRideTeamRecord(record).Coroutine();
            return record.Id;
        }

        private static async void SaveRideRoadInfos(RideRecord rideRecord)
        {
            await UserDataHelper.UpsertRideRecord(rideRecord);
        }

        private static void SaveRideTotalInfo(User user, PlayerRideTotalInfo rideTotalInfo, out BsonDocument log)
        {
            log = new BsonDocument();

            if (user == null)
            {
                Log.Error("SaveRideTotalInfo Failed, user == null");
                return;
            }

            // null代表不紀錄
            if (rideTotalInfo == null)
                return;

            if (rideTotalInfo.Mileage != 0)
            {
                user.playerRideTotalInfo.Mileage += rideTotalInfo.Mileage;
                log["mileage"] = user.playerRideTotalInfo.Mileage;
            }
            if (rideTotalInfo.CumulativeTime != 0)
            {
                user.playerRideTotalInfo.CumulativeTime += rideTotalInfo.CumulativeTime;
                log["cumulativeTime"] = user.playerRideTotalInfo.CumulativeTime;
            }
            // m/s * 3.6 = km/h
            var averageSpeed = user.playerRideTotalInfo.Mileage * 3.6f / user.playerRideTotalInfo.CumulativeTime;
            if (user.playerRideTotalInfo.AverageSpeed != averageSpeed)
            {
                user.playerRideTotalInfo.AverageSpeed = averageSpeed;
                log["averageSpeed"] = user.playerRideTotalInfo.AverageSpeed;
            }
            if (user.playerRideTotalInfo.TopSpeed != rideTotalInfo.TopSpeed)
            {
                user.playerRideTotalInfo.TopSpeed = rideTotalInfo.TopSpeed;
                log["topSpeed"] = user.playerRideTotalInfo.TopSpeed;
            }
            if (rideTotalInfo.Calories != 0)
            {
                user.playerRideTotalInfo.Calories += rideTotalInfo.Calories;
                log["calories"] = user.playerRideTotalInfo.Calories;
            }
        }

        private static async void SaveUserAndBroadcastTarget(MapUnit mapUnit, User user, BsonDocument log)
        {
            await UserDataHelper.UpsertUser(user, DBLog.LogType.UpdateUserRideTotalRecord, log);

            // 如果玩家在線上 告知該玩家異動紀錄
            var proxy = Game.Scene.GetComponent<CacheProxyComponent>();
            var playerSync = proxy.GetMemorySyncSolver<Player>();
            Player selfPlayer = playerSync.Get<Player>(mapUnit.Uid);
            if (selfPlayer != null)
            {
                PlayerRideTotalInfo playerRideTotalInfo = await UserDataHelper.QueryUserRideAllRecord(user);
                G2C_UpdatePlayerRideTotalInfo g2c_UpdatePlayerRideInfo = new G2C_UpdatePlayerRideTotalInfo()
                {
                    TotalInfo = playerRideTotalInfo,
                };
                GateMessageHelper.BroadcastTarget(g2c_UpdatePlayerRideInfo, mapUnit.Uid);
            }
        }
    }
}
