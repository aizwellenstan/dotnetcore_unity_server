using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using MongoDB.Bson.Serialization.Attributes;

namespace ETModel
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public abstract class SchemaAttribute : Attribute
    {
        public const string key = "_id";

        public SchemaAttribute()
        {

        }
    }

    /// <summary>
    /// (TODO)用Redis快取資料；如果再加上DBSchema屬性，可以做讀寫優化
    /// </summary>
    public sealed class RedisCacheAttribute : SchemaAttribute
    {
        public int secondOfStartDelay { private set; get; }

        public int secondOfDelaySaving { private set; get; }

        public int countOfBatchSaving { private set; get; }

        public TimeSpan timeToLive { private set; get; }

        public RedisCacheAttribute()
        {

        }

        public RedisCacheAttribute(int secondOfDelaySaving) : this(secondOfDelaySaving, 0)
        {

        }

        public RedisCacheAttribute(int secondOfDelaySaving, int countOfBatchSaving) : this(secondOfDelaySaving, countOfBatchSaving, 0)
        {

        }

        public RedisCacheAttribute(int secondOfDelaySaving, int countOfBatchSaving, int timeToLiveOnSecond) : base()
        {
            var rnd = new Random();
            this.secondOfStartDelay = rnd.Next(0, 301);
            this.secondOfDelaySaving = secondOfDelaySaving;
            this.countOfBatchSaving = countOfBatchSaving;
            this.timeToLive = new TimeSpan(0, 0, timeToLiveOnSecond);
        }
    }

    /// <summary>
    /// 進程記憶體同步用
    /// </summary>
    public sealed class MemorySyncAttribute : SchemaAttribute
    {
        public AppType appType { private set; get; } = AppType.AllServer;

        public MemorySyncAttribute() : base()
        {

        }

        public MemorySyncAttribute(AppType appType) : base()
        {
            this.appType = appType;
        }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public sealed class DBIndexAttribute : Attribute
    {
        public string indexName { get; private set; }
        public string[] columnNames { get; private set; }
        public bool isUnique { get; private set; }
        public DBIndexOrder[] dBIndexOrders { get; private set; }

        public DBIndexAttribute(string indexName, string[] orderedColumnNames, bool isUnique = false)
        {
            this.indexName = indexName;
            this.isUnique = isUnique;
            List<string> columns = new List<string>();
            List<DBIndexOrder> orders = new List<DBIndexOrder>();
            for (int i = 0; i < orderedColumnNames.Length; i+=2)
            {
                string columnName = string.Empty;
                int columnOrder = 1;
                if (i + 1 >= orderedColumnNames.Length)
                {
                    break;
                }
                columnName = orderedColumnNames[i];
                columnOrder = int.Parse(orderedColumnNames[i + 1]);
                orders.Add(new DBIndexOrder(columnName, 
                    (DBIndexOrder.Order)Enum.ToObject(typeof(DBIndexOrder.Order), columnOrder)));
                columns.Add(columnName);
            }
            this.columnNames = columns.ToArray();
            this.dBIndexOrders = orders.ToArray();
        }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class DBSchemaAttribute : Attribute
    {
        public bool isNeedToAlter { private set; get; }

        public DBSchemaAttribute(bool alter = false)
        {
            isNeedToAlter = alter;
        }
    }

    /// <summary>
    /// 要忽略快取的欄位
    /// 優先權高於SyncIgnoreAttribute
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public sealed class CacheIgnoreAttribute : Attribute
    {
        
    }

    /// <summary>
    /// 要忽略同步的欄位
    /// 優先權高於SyncOnlyOnAttribute
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public sealed class SyncIgnoreAttribute : Attribute
    {

    }

    /// <summary>
    /// 要同步快取在指定的Server上
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public sealed class SyncOnlyOnAttribute : Attribute
    {
        public AppType appType { private set; get; } = AppType.AllServer;

        public SyncOnlyOnAttribute(AppType appType)
        {
            this.appType = appType;
        }
    }
}
