using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ETModel
{
    public static class CacheHelper
    {
        public const string key = "_id";

        public const long INVALID_CALLER = -1L;

        public const long REFRESH_FLAG = -1L;

        private static readonly Dictionary<Type, AppType> syncServerMap =
            new Dictionary<Type, AppType>();

        private static readonly Dictionary<Type, Dictionary<string, PropertyInfo>> typeToCachePropertiesMap =
            new Dictionary<Type, Dictionary<string, PropertyInfo>>();

        private static readonly Dictionary<Type, string[]> typeToCachePropertyNamesMap =
            new Dictionary<Type, string[]>();

        private static readonly Dictionary<Type, Dictionary<string, PropertyInfo>> typeToSyncPropertiesMap =
            new Dictionary<Type, Dictionary<string, PropertyInfo>>();

        private static readonly Dictionary<Type, List<string>> typeToSyncPropertyNamesMap =
            new Dictionary<Type, List<string>>();

        public static void Initialize()
        {
            Type[] types = typeof(Game).Assembly.GetTypes();
            foreach (Type type in types)
            {
                if (!type.IsSubclassOf(typeof(Entity)))
                {
                    continue;
                }

                MemorySyncAttribute memorySyncAttr = type.GetCustomAttribute<MemorySyncAttribute>(false);
                if (memorySyncAttr != null)
                {
                    syncServerMap.Add(type, memorySyncAttr.appType);
                }

                // pick property
                var prots = type.GetProperties(BindingFlags.DeclaredOnly | BindingFlags.GetProperty | BindingFlags.Public |
                        BindingFlags.Instance);
                // 蒐集不做快取的欄位(不進Redis)
                var cachePropertyMap = new Dictionary<string, PropertyInfo>();
                // 蒐集不做同步的欄位
                var syncPropertyMap = new Dictionary<string, PropertyInfo>();
                foreach (var p in prots)
                {
                    // 蒐集不做快取的欄位
                    var cacheIgnoreAttr = p.GetCustomAttribute<CacheIgnoreAttribute>();
                    if (cacheIgnoreAttr != null)
                    {
                        continue;
                    }
                    cachePropertyMap.Add(p.Name, p);

                    // 蒐集不做同步的欄位
                    var syncIgnoreAttr = p.GetCustomAttribute<SyncIgnoreAttribute>();
                    if (syncIgnoreAttr != null)
                    {
                        continue;
                    }
                    var syncOnAttr = p.GetCustomAttribute<SyncOnlyOnAttribute>();
                    if(syncOnAttr != null)
                    {
                        var appType = StartConfigComponent.Instance.StartConfig.AppType;
                        if (syncOnAttr.appType == AppType.AllServer || appType == AppType.AllServer)
                        {
                            syncPropertyMap.Add(p.Name, p);
                        }
                        else
                        {
                            if(((int)syncOnAttr.appType & (int)appType) == (int)appType)
                            {
                                syncPropertyMap.Add(p.Name, p);
                            }
                        }
                    }
                    else
                    {
                        syncPropertyMap.Add(p.Name, p);
                    }
                }
                typeToCachePropertyNamesMap.Add(type, cachePropertyMap.Keys.ToArray());
                typeToCachePropertiesMap.Add(type, cachePropertyMap);
                typeToSyncPropertyNamesMap.Add(type, syncPropertyMap.Keys.ToList());
                typeToSyncPropertiesMap.Add(type, syncPropertyMap);
            }
        }

        /// <summary>
        /// 反序列化Redis為Bson格式
        /// </summary>
        /// <param name="type"></param>
        /// <param name="entries"></param>
        /// <returns></returns>
        public static BsonDocument Deserialize(Type type, HashEntry[] entries)
        {
            BsonDocument doc = new BsonDocument();
            for (int i = 0; i < entries.Length; i++)
            {
                var entry = entries[i];
                if (entry.Value.IsNull)
                {
                    continue;
                }
                if (entry.Name == key)
                {
                    doc[key] = (long)entry.Value;
                    continue;
                }
                PropertyInfo property = GetCachePropertyInfo(type, entry.Name);

                // 非基本型別解析Json
                if (!property.PropertyType.IsValueType && property.PropertyType != typeof(string))
                {
                    var str = (string)entry.Value;
                    if (string.Compare(str, "null", StringComparison.OrdinalIgnoreCase) == 0 || string.IsNullOrEmpty(str))
                    {
                        continue;
                    }
                    else
                    {
                        doc[entry.Name] = BsonSerializer.Deserialize<BsonDocument>(str);
                    }
                }
                else
                {
                    if (property.PropertyType == typeof(long))
                    {
                        doc[entry.Name] = (long)entry.Value;
                    }
                    else if (property.PropertyType == typeof(int))
                    {
                        doc[entry.Name] = (int)entry.Value;
                    }
                    else if (property.PropertyType == typeof(string))
                    {
                        doc[entry.Name] = (string)entry.Value;
                    }
                    else if (property.PropertyType == typeof(bool))
                    {
                        doc[entry.Name] = (bool)entry.Value;
                    }
                    else if (property.PropertyType == typeof(float))
                    {
                        doc[entry.Name] = (float)entry.Value;
                    }
                    else if (property.PropertyType == typeof(double))
                    {
                        doc[entry.Name] = (double)entry.Value;
                    }
                }
            }
            return doc;
        }

        /// <summary>
        /// 序列化Bson為Redis格式
        /// </summary>
        /// <param name="type"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static List<HashEntry> Serialize(Type type, BsonDocument entity)
        {
            List<HashEntry> updatedList = new List<HashEntry>();
            string[] propertyNames = GetCachePropertyNames(type);

            for (int j = 0; j < propertyNames.Length; j++)
            {
                string attName = propertyNames[j];
                PropertyInfo property = GetCachePropertyInfo(type, attName);

                // 必須允許更新Null的值
                if (!entity.Contains(attName))
                    continue;
                // 非基本型別轉Json處理
                if (!property.PropertyType.IsValueType && property.PropertyType != typeof(string))
                {
                    updatedList.Add(new HashEntry(attName, entity[attName].ToJson()));
                }
                else
                {
                    HashEntry entry = new HashEntry(attName, RedisValue.Null);
                    if (property.PropertyType == typeof(long))
                    {
                        entry = new HashEntry(attName, entity[attName].AsInt64);
                    }
                    else if (property.PropertyType == typeof(int))
                    {
                        entry = new HashEntry(attName, entity[attName].AsInt32);
                    }
                    else if (property.PropertyType == typeof(string))
                    {
                        entry = new HashEntry(attName, entity[attName].AsString);
                    }
                    else if (property.PropertyType == typeof(bool))
                    {
                        entry = new HashEntry(attName, entity[attName].AsBoolean);
                    }
                    else if (property.PropertyType == typeof(float))
                    {
                        entry = new HashEntry(attName, (float)entity[attName].AsDouble);
                    }
                    else if (property.PropertyType == typeof(double))
                    {
                        entry = new HashEntry(attName, entity[attName].AsDouble);
                    }
                    updatedList.Add(entry);
                }
            }

            return updatedList;
        }

        public static AppType GetMemorySyncAppType(Type type)
        {
            return syncServerMap[type];
        }

        /// <summary>
        /// 檢查是否是同步的目標
        /// </summary>
        /// <param name="type"></param>
        /// <param name="appType"></param>
        /// <returns></returns>
        public static bool IsSyncOn(Type type, AppType appType)
        {
            if (appType == AppType.AllServer)
                return true;
            if(!syncServerMap.TryGetValue(type, out AppType val))
            {
                return false;
            }
            return ((int)val & (int)appType) == (int)appType;
        }

        public static PropertyInfo GetCachePropertyInfo(Type type, string property)
        {
            typeToCachePropertiesMap[type].TryGetValue(property, out var info);
            return info;
        }

        public static string[] GetCachePropertyNames(Type type)
        {
            return typeToCachePropertyNamesMap[type];
        }

        public static PropertyInfo GetSyncPropertyInfo(Type type, string property)
        {
            typeToSyncPropertiesMap[type].TryGetValue(property, out var info);
            return info;
        }

        public static List<string> GetSyncPropertyNames(Type type)
        {
            return typeToSyncPropertyNamesMap[type];
        }

        public static T GetFromCache<T>(long id) where T : ComponentWithId
        {
            var proxy = Game.Scene.GetComponent<CacheProxyComponent>();
            var memSync = proxy.GetMemorySyncSolver<T>();
            return memSync.Get<T>(id);
        }

        public static bool IsMine<T>(long id) where T : ComponentWithId
        {
            var proxy = Game.Scene.GetComponent<CacheProxyComponent>();
            var memSync = proxy.GetMemorySyncSolver<T>();
            return memSync.IsMine(id);
        }

        public static byte[] ConvertRedisPublish2Message(long id, long appType)
        {
            byte[] front = BitConverter.GetBytes(id);
            byte[] back = BitConverter.GetBytes(appType);
            byte[] combined = combined = new byte[16]
            {
                front[0], front[1],front[2],front[3],front[4],front[5],front[6],front[7],
                back[0], back[1],back[2],back[3],back[4],back[5],back[6],back[7],
            };
            return combined;
        }

        public static void ConvertMessage2RedisSubscribe(byte[] data, out long id, out long appId)
        {
            id = BitConverter.ToInt64(data, 0);
            appId = BitConverter.ToInt64(data, 8);
        }
    }
}
