using System;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using ETHotfix;
using Google.Protobuf.Collections;

namespace ETModel
{
    #region 用法說明

    //TODO:禁用Collection的別名，統一用小寫類別名稱當作Collection名稱，底層會自動做
    //[RedisCache(typeof(User), "user", 0)]

    //DBSchema用法
    //[DBSchema(true)]
    //這裡宣告的物件都要附加屬性DBSchema，表示MongoDB的表
    //屬性alter表示是否在Server重開時，同步欄位跟索引
    //DBIndex用法
    //new string[] { "account", "1", "password": "-1" }
    //1:升序,-1:降序

    //例子:
    //[DBSchema(true)]
    //[DBIndex("account", new [] { "account", "1" }, true)]
    //public class User : Entity
    //{
    //    [BsonElement("account")]
    //    public string account { get; set; }
    //}

    /***************快取模式***************/

    // 快取模式分為5種
    // 1.DBOnly
    // 必要屬性{DBSchema}
    // 可選屬性{DBSchema, DBIndex}
    // 必用組件{DBProxyComponent}
    // 僅對DB做CRUD

    // 2.RedisCacheOnly
    // 必要屬性{RedisCache}
    // 可選屬性{DBIndex}
    // 必用組件{CacheProxyComponent}
    // 僅對Redis做CRUD

    // 3.MemorySyncByRedis
    // 必要屬性{RedisCache, MemorySync}
    // 可選屬性{DBIndex, SyncIgnore(僅用來修飾在(Property { set; get; })上)}
    // 必用組件{CacheProxyComponent}
    // MemorySync可以附加需要同步的Server的類型
    // 使用Redis對記憶體容器做CRUD的資料同步(Redis會有該份資料)

    // 4.MemorySyncByDB(尚未實作)
    // 必要屬性{DBSchema, MemorySync}
    // 可選屬性{DBIndex, SyncIgnore(僅用來修飾在(Property { set; get; })上)}
    // 必用組件{CacheProxyComponent}
    // MemorySync可以附加需要同步的Server的類型
    // 使用DB對記憶體容器做CRUD的資料同步(DB會有該份資料)

    // 5.DBCache(尚未實作)
    // 必要屬性{DBSchema, RedisCache}
    // 可選屬性{DBIndex, SyncIgnore(僅用來修飾在(Property { set; get; })上)}
    // 必用組件{CacheProxyComponent}
    // MemorySync可以附加需要同步的Server的類型
    // 使用Redis當DB的快取，以加速查找(Redis跟DB會有該份資料)

    #endregion

    [DBSchema(true)]
    [DBIndex("name", new[] { "name", "1" })]
    public class User : Entity
    {
        [BsonElement("salt")]
        public string salt { get; set; }

        [BsonElement("hashPassword")]
        public string hashPassword { get; set; }

        [BsonElement("email"), BsonDefaultValue("")]
        public string email { get; set; }

        //預設值 = "玩家" + id.ToString()
        [BsonElement("name"), BsonDefaultValue("")]
        public string name { get; set; }

        [BsonElement("sex"), BsonDefaultValue(0)]
        public int gender { get; set; }

        [BsonElement("height"), BsonDefaultValue(0)]
        public int height { get; set; }

        [BsonElement("weight"), BsonDefaultValue(0)]
        public int weight { get; set; }

        /// <summary>
        /// 國籍
        /// 預設在美國
        /// </summary>
        [BsonElement("location"), BsonDefaultValue((int)Location.Usa)]
        public int location { get; set; }

        [BsonElement("birthday"), BsonDefaultValue(19000101)]
        public int birthday { get; set; }

        [BsonElement("createAt"), BsonDefaultValue(0)]
        public long createAt { get; set; }

        [BsonElement("lastOnlineAt"), BsonDefaultValue(0L)]
        public long lastOnlineAt { get; set; }

        [BsonElement("coin"), BsonDefaultValue(0L)]
        public long coin { get; set; }

        [BsonElement("playerCharSetting"), BsonDefaultValue(null)]
        public PlayerCharSetting playerCharSetting { get; set; }

        [BsonElement("playerRideTotalInfo"), BsonDefaultValue(null)]
        public RideTotalInfo playerRideTotalInfo { get; set; }

        [BsonElement("language"), BsonDefaultValue(10)]//en = 10
        public int language { get; set; }

        [BsonElement("firebaseDeviceToken"), BsonDefaultValue("")]
        public string firebaseDeviceToken { get; set; }

        [BsonElement("lastCreateTokenAt"), BsonDefaultValue(0L)]
        public long lastCreateTokenAt { get; set; }

        [BsonElement("identity"), BsonDefaultValue(Identity.Player)]
        public int identity { get; set; }

        [BsonElement("userBagCapacity"), BsonDefaultValue(null)]
        public BsonDocument userBagCapacity { get; set; }

        public enum Gender
        {
            Unknown,
            Male,
            Female,
        }

        public enum Location
        {
            Usa = 0,
        }

        [Flags]
        public enum Identity
        {
            Player = 1 << 0,

            TestPlayer = 1 << 1,

            Super = 1 << 2,
        }
    }

    [DBSchema(true)]
    [DBIndex("uid", new[] { "uid", "1" })]
    [DBIndex("partyId", new[] { "party", "1", "userId", "1" }, true)]
    [DBIndex("idParty", new[] { "userId", "1", "party", "1" }, true)]
    public class ThirdPartyUser : Entity
    {
        [BsonElement("uid")]
        public long uid { get; set; }

        [BsonElement("party"), BsonDefaultValue("")]
        public string party { get; set; }

        [BsonElement("userId"), BsonDefaultValue("")]
        public string userId { get; set; }

        [BsonElement("name"), BsonDefaultValue("")]
        public string name { get; set; }

        [BsonElement("gender"), BsonDefaultValue("")]
        public string gender { get; set; }

        [BsonElement("location"), BsonDefaultValue("")]
        public string location { get; set; }

        [BsonElement("email"), BsonDefaultValue("")]
        public string email { get; set; }

        [BsonElement("birthday"), BsonDefaultValue("")]
        public string birthday { get; set; }

        [BsonElement("createAt"), BsonDefaultValue(0L)]
        public long createAt { get; set; }
    }

    [DBSchema(true)]
    public class ContestHistory : Entity
    {
        [BsonElement("email")]
        public string email { get; set; }
        [BsonElement("password")]
        public string password { get; set; }
        [BsonElement("level")]
        public int level { get; set; }
    }

    [DBSchema(true)]
    [DBIndex("uid", new[] { "uid", "1" })]
    [DBIndex("relationId", new[] { "uid", "1", "targetUid", "1" }, true)]
    public class Relationship : Entity
    {
        [Flags]
        public enum RelationType
        {
            Stranger = 1 << 0,

            Friend = 1 << 1,

            /// <summary>
            /// 黑名單
            /// </summary>
            Blacklist = 1 << 2,

            /// <summary>
            /// 申請列表
            /// </summary>
            Apply = 1 << 3,
        }

        [BsonElement("uid")]
        public long uid { get; set; }

        [BsonElement("targetUid")]
        public long targetUid { get; set; }

        [BsonElement("relationshipType"), BsonDefaultValue((int)RelationType.Friend)]
        public int relationshipType { get; set; }

        [BsonElement("confirmedAt"), BsonDefaultValue(0L)]
        public long confirmedAt { get; set; }

        [BsonElement("createAt"), BsonDefaultValue(0L)]
        public long createAt { get; set; }
    }

    [DBSchema(true)]
    [DBIndex("applyId", new[] { "applyId", "1" })]
    [DBIndex("receiverUid", new[] { "receiverUid", "1", "senderUid", "1" }, true)]
    public class RelationshipApply : Entity
    {
        [BsonElement("applyId")]
        public long applyId { get; set; }

        [BsonElement("senderName")]
        public string senderName { get; set; }

        [BsonElement("senderUid")]
        public long senderUid { get; set; }

        [BsonElement("senderLocation")]
        public int senderLocation { get; set; }

        [BsonElement("senderMileage")]
        public long senderMileage { get; set; }

        [BsonElement("receiverName")]
        public string receiverName { get; set; }

        [BsonElement("receiverUid")]
        public long receiverUid { get; set; }

        [BsonElement("receiverLocation")]
        public int receiverLocation { get; set; }

        [BsonElement("receiverMileage")]
        public long receiverMileage { get; set; }

        public static RelationshipApplyInfo ConvertToRelationshipApplyInfo(RelationshipApply relationshipApply)
        {
            var info = new RelationshipApplyInfo();
            if (relationshipApply != null)
            {
                info.ApplyId = relationshipApply.applyId;

                info.SenderName = relationshipApply.senderName;
                info.SenderUid = relationshipApply.senderUid;
                info.SenderLocation = relationshipApply.senderLocation;
                info.SenderMileage = relationshipApply.senderMileage;

                info.ReceiverName = relationshipApply.receiverName;
                info.ReceiverUid = relationshipApply.receiverUid;
                info.ReceiverLocation = relationshipApply.receiverLocation;
                info.ReceiverMileage = relationshipApply.receiverMileage;
            }
            return info;
        }
    }

    [DBSchema(true)]
    [DBIndex("uid", new[] { "uid", "1" })]
    public class Mail : Entity
    {
        [Flags]
        public enum MailType
        {
            Relationship = 1 << 0,
        }

        public enum From
        {
            System = -1,
        }

        [BsonElement("uid")]
        public long uid { get; set; }

        [BsonElement("from"), BsonDefaultValue((long)From.System)]
        public long from { get; set; }

        [BsonElement("mailType"), BsonDefaultValue((int)MailType.Relationship)]
        public int mailType { get; set; }

        [BsonElement("message"), BsonDefaultValue("")]
        public string message { get; set; }

        [BsonElement("attachment"), BsonDefaultValue(null)]
        public BsonDocument attachment { get; set; }

        [BsonElement("createAt"), BsonDefaultValue(0L)]
        public long createAt { get; set; }
    }

    [DBSchema(true)]
    [DBIndex("uid", new[] { "uid", "1" })]
    public class ReservationDB : Entity
    {
        [BsonElement("uid")]
        public long uid { get; set; }

        [BsonElement("allData"), BsonDefaultValue(null)]
        public BsonBinaryData allData { get; set; }
    }

    /// <summary>
    /// 資料庫升級腳本
    /// </summary>
    [DBSchema(true)]
    [DBIndex("step", new[] { "step", "1" }, true)]
    [DBIndex("scriptName", new[] { "scriptName", "1" }, true)]
    public class DBUpgradeScript : Entity
    {
        [BsonElement("step"), BsonDefaultValue(1)]
        public long step { get; set; }

        [BsonElement("scriptName"), BsonDefaultValue("InvalidScript")]
        public string scriptName { get; set; }

        [BsonElement("isChecked"), BsonDefaultValue(false)]
        public bool isChecked { get; set; }

        [BsonElement("checkAt"), BsonDefaultValue(0L)]
        public long checkAt { get; set; }

        [BsonElement("createAt"), BsonDefaultValue(0L)]
        public long createAt { get; set; }
    }

    /// <summary>
    /// 使用者道具&裝備
    /// </summary>
    [DBSchema(true)]
    [DBIndex("uid", new[] { "uid", "1" })]
    [DBIndex("uid_configType", new[] { "uid", "1", "configType", "1" })]
    [DBIndex("uid_configType_configId", new[] { "uid", "1", "configType", "1", "configId", "1" })]
    public class Equipment : Entity
    {
        [BsonElement("uid")]
        public long uid { get; set; }

        [BsonElement("configType"), BsonDefaultValue((int)EquipmentType.Decoration)]
        public int configType { get; set; }

        [BsonElement("configId"), BsonDefaultValue(0L)]
        public long configId { get; set; }

        [BsonElement("count"), BsonDefaultValue(0)]
        public int count { get; set; }

        [BsonElement("information"), BsonDefaultValue(null)]
        public BsonDocument information { get; set; } = new BsonDocument();

        public enum EquipmentType
        {
            None = 0,
            Decoration = 4,
        }
    }

    /// <summary>
    /// 漫騎&活動&揪團的歷史紀錄
    /// </summary>
    [DBSchema(true)]
    [DBIndex("uid", new[] { "uid", "1" }, false)]
    public class RideRecord : Entity
    {
        public enum RideType
        {
            /// <summary>
            /// 漫騎
            /// </summary>
            Roam = 0,

            /// <summary>
            /// 組隊
            /// </summary>
            Party = 1,
        }

        /// <summary>
        /// 紀錄的對象
        /// </summary>
        [BsonElement("uid"), BsonDefaultValue(0L)]
        public long uid { get; set; }

        /// <summary>
        /// 組隊的Id
        /// </summary>
        [BsonElement("teamId"), BsonDefaultValue(0L)]
        public long teamId { get; set; }

        /// <summary>
        /// 騎乘類型
        /// </summary>
        [BsonElement("rideType"), BsonDefaultValue((int)RideType.Roam)]
        public int rideType { get; set; } = (int)RideType.Roam;

        /// <summary>
        /// 路徑的id(對應企劃表的id)
        /// </summary>
        [BsonElement("roadConfigId"), BsonDefaultValue(0L)]
        public long roadConfigId { get; set; }

        /// <summary>
        /// 排名(如果是漫騎那排名是預設值0)
        /// </summary>
        [BsonElement("rank"), BsonDefaultValue(0)]
        public int rank { get; set; }

        /// <summary>
        /// 里程數
        /// </summary>
        [BsonElement("mileage"), BsonDefaultValue(0L)]
        public long mileage { get; set; }

        /// <summary>
        /// 騎了多久(單位:秒)
        /// </summary>
        [BsonElement("cumulativeSecond"), BsonDefaultValue(0L)]
        public long cumulativeSecond { get; set; }

        /// <summary>
        /// 平均速度(單位:km/h)
        /// </summary>
        [BsonElement("averageSpeed"), BsonDefaultValue(0f)]
        public double averageSpeed { get; set; }

        /// <summary>
        /// 最高速度(單位:km/h)
        /// </summary>
        [BsonElement("topSpeed"), BsonDefaultValue(0f)]
        public double topSpeed { get; set; }

        /// <summary>
        /// 卡路里
        /// </summary>
        [BsonElement("calories"), BsonDefaultValue(0f)]
        public double calories { get; set; }

        /// <summary>
        /// 消耗功率
        /// </summary>
        [BsonElement("power"), BsonDefaultValue(0f)]
        public double power { get; set; }

        /// <summary>
        /// 紀錄於...(單位:毫秒)
        /// </summary>
        [BsonElement("createAt"), BsonDefaultValue(0L)]
        public long createAt { get; set; }
    }

    public class MemberBrief
    {
        /// <summary>
        /// 使用者Id
        /// </summary>
        [BsonElement("uid"), BsonDefaultValue(0L)]
        public long uid { get; set; }

        /// <summary>
        /// 使用者姓名
        /// </summary>
        [BsonElement("name"), BsonDefaultValue("")]
        public string name { get; set; }


        /// <summary>
        /// 這次比賽走了多少距離?(單位:公尺)
        /// </summary>
        [BsonElement("traveledDistance"), BsonDefaultValue(0)]
        public double traveledDistance { get; set; }

        /// <summary>
        /// 國籍
        /// </summary>
        [BsonElement("location"), BsonDefaultValue((int)User.Location.Usa)]
        public int location { get; set; }
    }

    /// <summary>
    /// 漫騎&活動的組隊成員資訊概要
    /// </summary>
    [DBSchema(true)]
    public class RideTeamRecord : Entity
    {
        /// <summary>
        /// 所有成員
        /// </summary>
        [BsonElement("members"), BsonDefaultValue(null)]
        public RepeatedField<MemberBrief> members { get; set; } = new RepeatedField<MemberBrief>();

        /// <summary>
        /// 紀錄於...
        /// </summary>
        [BsonElement("createAt"), BsonDefaultValue(0L)]
        public long createAt { get; set; }
    }

    /// <summary>
    /// Log紀錄表
    /// </summary>
    [DBSchema(true)]
    [DBIndex("uid", new[] { "uid", "1" }, false)]
    [DBIndex("logType", new[] { "logType", "1" }, false)]
    [DBIndex("recordId", new[] { "uid", "1", "logType", "1" }, false)]
    public class DBLog : Entity
    {
        public enum LogType
        {
            Unknown = 0,

            // 使用者Log 1~9999
            SignUserUp = 1, // 使用者註冊
            SignUserIn = 2, // 使用者登入
            BindThirdPartyUser = 3, // 綁定第三方使用者
            UpdateUserRideTotalRecord = 100, // 更新使用者騎乘記錄
            UpdateUserCharacterSetting = 200, // 更新使用者腳色設定
            UpdateUserBagSlotCount = 201, // 更新使用者背包數
            UpdateUserProfiler = 300, // 更新使用者的簡介
            UpdateUserLanguage = 301, // 更新使用者語系

            // 好友關係Log 10000~19999
            Relationship = 10000,
            RelationshipApply = 10001,

            // 道具Log 20000~29999
            AddEquipment = 20000, // 新增道具
            SubtractEquipment = 20001, // 移除道具
            MergeEquipment = 20002, // 可疊道具如果出現2筆以上，把她合並成一筆
        }

        /// <summary>
        /// Log紀錄的對象
        /// </summary>
        [BsonElement("uid"), BsonDefaultValue(0L)]
        public long uid { get; set; }

        /// <summary>
        /// Log型態
        /// </summary>
        [BsonElement("logType"), BsonDefaultValue((int)LogType.Unknown)]
        public int logType { get; set; }

        /// <summary>
        /// Log紀錄
        /// </summary>
        [BsonElement("document"), BsonDefaultValue(null)]
        public BsonDocument document { get; set; }

        /// <summary>
        /// 紀錄於...
        /// </summary>
        [BsonElement("createAt"), BsonDefaultValue(0L)]
        public long createAt { get; set; }
    }

    [RedisCache]
    [MemorySync(AppType.Gate | AppType.Realm | AppType.Map | AppType.Lobby)]
    public partial class Player : Entity
    {
        [SyncOnlyOn(AppType.Gate | AppType.Lobby | AppType.Map)]
        public long gateSessionActorId { get; set; }

        public long mapUnitId { get; set; }

        public int gateAppId { get; set; }

        public int lobbyAppId { get; set; }

        public int mapAppId { get; set; }

        public long roomID { get; set; }

        #region Online/Offline

        public bool isOnline { get; set; } = true;

        public long disconnectTime { get; set; } = 0;

        #endregion

        #region PlayerStateData

        public PlayerStateData playerStateData { get; set; } = new PlayerStateData();

        #endregion
    }

    [RedisCache]
    [MemorySync(AppType.Gate | AppType.Realm | AppType.Map | AppType.Lobby)]
    public partial class Room : Entity
    {
        // TODO:複雜型別會被強轉Json
        // 可以考慮把欄位全部拆出來或轉Bson或Byte[]，不確定Redis可不可以直接吃二進制
        public RoomInfo info { get; set; }

        public int state { get; set; } = (int)RoomState.Ready;

        public int type { get; set; }
    }

    [RedisCache]
    [MemorySync(AppType.Lobby | AppType.Map)]
    public partial class Invite : Entity
    {
        public InviteData data { get; set; }
    }

    [RedisCache]
    [MemorySync(AppType.Lobby)]
    public partial class Reservation : Entity
    {
        public enum ReservationState
        {
            Sleep,
            Show,
            Run,
            Destroy
        }

        public ReservationAllData allData { get; set; }

        public ReservationData data { get; set; }

        public int state { get; set; }

        public Room room { get; set; }
    }
}
