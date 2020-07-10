using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using ETModel;
using Google.Protobuf.Collections;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;

namespace ETHotfix
{
    //TODO：要改用新的DB組件
    public static class UserDataHelper
    {
        public const string tagToken = "Token";
        public const string tagGuest = "Guest";
        public const string tagFB = "Facebook";
        public const string tagJPlay = "JPlay";
        public const string tagAppleId = "AppleId";

        private static DBProxyComponent dbProxy
        {
            get
            {
                return Game.Scene.GetComponent<DBProxyComponent>();
            }
        }

        private static void InvalidatePlayerCharSetting(ref User user, BsonDocument log = null)
        {
            if (!LoungeUtility.IsCharacterType(user.playerCharSetting.CharacterId))
            {
                // todo
                Log.Warning($"CharacterId:{user.playerCharSetting.CharacterId} is invalid.");
                user.playerCharSetting.CharacterId = 1; // default
                if(log != null)
                    log["characterId"] = user.playerCharSetting.CharacterId;
            }
            if (!LoungeUtility.IsBicycleType(user.playerCharSetting.BicycleId))
            {
                // todo
                Log.Warning($"BicycleId:{user.playerCharSetting.BicycleId} is invalid.");
                user.playerCharSetting.BicycleId = 100; // default
                if (log != null)
                    log["bicycleId"] = user.playerCharSetting.BicycleId;
            }
            if (!LoungeUtility.IsBodyType(user.playerCharSetting.BodyId))
            {
                // todo
                Log.Warning($"BodyId:{user.playerCharSetting.BodyId} is invalid.");
                user.playerCharSetting.BodyId = 200; // default
                if (log != null)
                    log["bodyId"] = user.playerCharSetting.BodyId;
            }
            // TODO:等需求再決定是否解鎖或移除
            //if (!LoungeUtility.IsDecorationType(user.playerCharSetting.DecorationId))
            //{
            //    //todo
            //    Log.Warning($"DecorationId:{user.playerCharSetting.DecorationId} is invalid.");
            //    user.playerCharSetting.DecorationId = 500; //default
            //}
            if (!LoungeUtility.IsMedalType(user.playerCharSetting.MedalId))
            {
                // todo
                Log.Warning($"MedalId:{user.playerCharSetting.MedalId} is invalid.");
                user.playerCharSetting.MedalId = 1000; // default
                if (log != null)
                    log["medalId"] = user.playerCharSetting.MedalId;
            }
        }

        /// <summary>
        /// 用帳戶ID查詢一位使用者
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        public static async ETTask<User> FindOneUser(long uid)
        {
            var list = await dbProxy.Query<User>(entity => entity.Id == uid);
            if (list.Any())
            {
                return (User) list[0];
            }
            return null;
        }

        /// <summary>
        /// 用帳戶ID查詢一位使用者
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        public static async ETTask<User> FindOneUser(string uid)
        {
            long userId;
            if(long.TryParse(uid, out userId))
            {
                return await FindOneUser(userId);
            }
            else
            {
                Log.Warning($"invalid long integer 'userId'!");
                return null;
            }
        }

        /// <summary>
        /// 用帳戶名稱查詢一位第三方使用者
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="party"></param>
        /// <returns></returns>
        public static async ETTask<ThirdPartyUser> FindOneThirdPartyUser(string userId, string party)
        {
            var list = await dbProxy.Query<ThirdPartyUser>(entity => entity.userId == userId && entity.party == party);
            if (list.Count != 0)
            {
                return (ThirdPartyUser)list[0];
            }
            return null;
        }

        /// <summary>
        /// 用UID查詢所有第三方使用者
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="party"></param>
        /// <returns></returns>
        public static async ETTask<List<ThirdPartyUser>> FindAllThirdPartyUser(long uid)
        {
            var list = await dbProxy.Query<ThirdPartyUser>(entity => entity.uid == uid);
            return list.OfType<ThirdPartyUser>().ToList();
        }

        /// <summary>
        /// 更新或插入一位使用者
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public static async ETTask SinUserUp(User user)
        {
            InvalidatePlayerCharSetting(ref user);
            await dbProxy.Save(user);
            await dbProxy.SaveLog(user.Id, DBLog.LogType.SignUserUp, user);
        }

        /// <summary>
        /// 更新或插入一位使用者
        /// </summary>
        /// <param name="user"></param>
        /// <param name="logType"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        public static async ETTask UpsertUser(User user, DBLog.LogType logType, BsonDocument log)
        {
            BsonDocument charSettingLog = new BsonDocument();
            InvalidatePlayerCharSetting(ref user, charSettingLog);
            var needLog1 = log.Count() != 0;
            var needLog2 = charSettingLog.Count() != 0;
            if (needLog1 || needLog2)
            {
                await dbProxy.Save(user);
                if (needLog1)
                {
                    await dbProxy.SaveLog(user.Id, logType, log);
                }
                if(needLog2)
                {
                    await dbProxy.SaveLog(user.Id, DBLog.LogType.UpdateUserCharacterSetting, charSettingLog);
                }
            }
        }

        /// <summary>
        /// 更新或插入一位第三方使用者
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public static async ETTask UpsertThirdPartyUser(ThirdPartyUser user)
        {
            await dbProxy.Save(user);
            await dbProxy.SaveLog(user.Id, DBLog.LogType.BindThirdPartyUser, user);
        }

        /// <summary>
        /// 根據條件、筆數跟偏移，尋找使用者們
        /// </summary>
        /// <param name="limit"></param>
        /// <returns></returns>
        public static async ETTask<List<User>> FindUsers(Expression<Func<User, bool>> predicate, long skip, long limit)
        {
            List<ComponentWithId> list = await dbProxy.Query(predicate, skip, limit);
            return list.OfType<User>().ToList();
        }

        
        /// <summary>
        /// 查詢複數使用者資料
        /// </summary>
        /// <param name="uids"></param>
        /// <returns></returns>
        public static async ETTask<List<User>> FindUsers(long[] uids)
        {
            var list = await dbProxy.Query<User>(entity => uids.Contains(entity.Id));
            return list.OfType<User>().ToList();
        }

        /// <summary>
        /// 使用帳號密碼註冊
        /// </summary>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static async ETTask<int> SignUp(string email, string password, User.Identity identity = User.Identity.Player)
        {
            ThirdPartyUser thirdPartyUser = await FindOneThirdPartyUser(email, tagJPlay);
            if (thirdPartyUser != null)
            {
                return ErrorCode.ERR_SignUpFailed;
            }
            else
            {
                long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                string salt = CryptographyHelper.GenerateRandomId();
                string hashPassword = CryptographyHelper.MD5Encoding(password, salt);
                User user = ComponentFactory.CreateWithId<User>(IdGenerater.GenerateId());
                user.salt = salt;
                user.email = email;
                user.hashPassword = hashPassword;
                user.createAt = now;
                user.name = $"{user.Id}";
                user.playerCharSetting = new PlayerCharSetting();
                user.playerCharSetting.CharacterId = 1L;
                user.playerRideTotalInfo = new RideTotalInfo();
                user.identity = (int)identity;
                await SinUserUp(user);
                thirdPartyUser = ComponentFactory.CreateWithId<ThirdPartyUser>(IdGenerater.GenerateId());
                thirdPartyUser.uid = user.Id;
                thirdPartyUser.party = tagJPlay;
                thirdPartyUser.userId = user.email;
                thirdPartyUser.name = "";
                thirdPartyUser.gender = "";
                thirdPartyUser.location = "";
                thirdPartyUser.email = email;
                thirdPartyUser.birthday = "";
                thirdPartyUser.createAt = now;
                await UpsertThirdPartyUser(thirdPartyUser);
                return ErrorCode.ERR_Success;
            }
        }

        /// <summary>
        /// 更新或插入一筆騎乘紀錄
        /// </summary>
        /// <param name="record"></param>
        /// <returns></returns>
        public static async ETTask UpsertRideRecord(RideRecord record)
        {
            await dbProxy.Save(record);
        }

        /// <summary>
        /// 更新或插入一筆組隊紀錄
        /// </summary>
        /// <param name="record"></param>
        /// <returns></returns>
        public static async ETTask UpsertRideTeamRecord(RideTeamRecord record)
        {
            await dbProxy.Save(record);
        }

        /// <summary>
        /// 查詢多筆騎乘紀錄
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        public static async ETTask<List<RideRecord>> QueryUserRideRecords(long uid)
        {
            var result = await dbProxy.Query<RideRecord>(entity => entity.uid == uid);
            return result.OfType<RideRecord>().ToList();
        }

        /// <summary>
        /// 查詢一筆組隊紀錄
        /// </summary>
        /// <param name="teamId"></param>
        /// <returns></returns>
        public static async ETTask<RideTeamRecord> QueryRideTeamRecord(long teamId)
        {
            return await dbProxy.Query<RideTeamRecord>(teamId);
        }

        /// <summary>
        /// 查詢使用者全部騎乘紀錄
        /// </summary>
        /// <param name="teamId"></param>
        /// <returns></returns>
        public static async ETTask<PlayerRideTotalInfo> QueryUserRideAllRecord(long uid)
        {
            var user = await FindOneUser(uid);
            return await QueryUserRideAllRecord(user);
        }

        /// <summary>
        /// 查詢使用者全部騎乘紀錄
        /// </summary>
        /// <param name="teamId"></param>
        /// <returns></returns>
        public static async ETTask<PlayerRideTotalInfo> QueryUserRideAllRecord(User user)
        {
            var userRideRecords = await QueryUserRideRecords(user.Id);
            var rideAllInfo = new RepeatedField<PlayerRideRoadInfo>();
            for (int i = 0; i < userRideRecords.Count; i++)
            {
                var playerRideRoadInfo = new PlayerRideRoadInfo();
                var userRideRecord = userRideRecords[i];
                playerRideRoadInfo.InfoId = userRideRecord.Id;
                playerRideRoadInfo.RoadId = userRideRecord.roadConfigId;
                playerRideRoadInfo.Rank = userRideRecord.rank;
                playerRideRoadInfo.Mileage = userRideRecord.mileage;
                playerRideRoadInfo.CumulativeTime = userRideRecord.cumulativeSecond;
                playerRideRoadInfo.AverageSpeed = (float)userRideRecord.averageSpeed;
                playerRideRoadInfo.TopSpeed = (float)userRideRecord.topSpeed;
                playerRideRoadInfo.Calories = (float)userRideRecord.calories;
                playerRideRoadInfo.Power = (float)userRideRecord.power;
                playerRideRoadInfo.RecordUTCTick = DateHelper.TimestampMillisecondToDateTimeUTC(userRideRecord.createAt).Ticks;
                if (userRideRecord.rideType == (int)RideRecord.RideType.Party)
                {
                    var teamRecord = await QueryRideTeamRecord(userRideRecord.teamId);
                    playerRideRoadInfo.BattleLeaderboardUnitInfos = teamRecord.members.Aggregate(new RepeatedField<BattleLeaderboardUnitInfo>(), (list, item) =>
                    {
                        list.Add(item.ToBattleLeaderboardUnitInfo());
                        return list;
                    });
                }
                rideAllInfo.Add(playerRideRoadInfo);
            }

            return new PlayerRideTotalInfo
            {
                Mileage = user.playerRideTotalInfo.Mileage,
                CumulativeTime = user.playerRideTotalInfo.CumulativeTime,
                AverageSpeed = user.playerRideTotalInfo.AverageSpeed,
                TopSpeed = user.playerRideTotalInfo.TopSpeed,
                Calories = user.playerRideTotalInfo.Calories,
                RideRoadInfos = rideAllInfo
            };
        }

        public static MemberBrief ToMemberBrief(this BattleLeaderboardUnitInfo battleLeaderboardUnitInfo)
        {
            return new MemberBrief
            {
                uid = battleLeaderboardUnitInfo.Uid,
                name = battleLeaderboardUnitInfo.Name,
                traveledDistance = battleLeaderboardUnitInfo.DistanceTravelledTarget,
                location = battleLeaderboardUnitInfo.Location
            };
        }

        public static BattleLeaderboardUnitInfo ToBattleLeaderboardUnitInfo(this MemberBrief memberBrief)
        {
            return new BattleLeaderboardUnitInfo
            {
                Uid = memberBrief.uid,
                Name = memberBrief.name,
                DistanceTravelledTarget = memberBrief.traveledDistance,
                Location = memberBrief.location
            };
        }
    }
}
