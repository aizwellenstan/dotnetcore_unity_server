using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ETModel
{
    public static class DBHelper
    {
        private static readonly Dictionary<Type, Dictionary<string, PropertyInfo>> typeToPropertiesMap =
            new Dictionary<Type, Dictionary<string, PropertyInfo>>();

        private static readonly Dictionary<Type, string[]> typeToPropertyNamesMap =
            new Dictionary<Type, string[]>();

        private static readonly Dictionary<Type, Dictionary<string, DBIndexAttribute>> typeToIndicesMap =
            new Dictionary<Type, Dictionary<string, DBIndexAttribute>>();

        private static readonly Dictionary<Type, string[]> typeToIndexNamesMap =
            new Dictionary<Type, string[]>();

        private static readonly Dictionary<Type, CacheUseMode> typeToCacheUseModeMap =
            new Dictionary<Type, CacheUseMode>();

        public static void Initialize()
        {
            Type[] types = typeof(Game).Assembly.GetTypes();
            foreach (Type type in types)
            {
                if (!type.IsSubclassOf(typeof(Entity)))
                {
                    continue;
                }

                object[] objects = type.GetCustomAttributes(typeof(Attribute), false);
                if (objects.Length == 0)
                {
                    continue;
                }
                byte flag = 0;
                for (int i = 0; i < objects.Length; i++)
                {
                    object attr = objects[i];
                    switch (attr)
                    {
                        case DBSchemaAttribute dbAtrr:
                            flag |= 1;
                            break;
                        case RedisCacheAttribute rdsAtrr:
                            flag |= 2;
                            break;
                        case MemorySyncAttribute memAtrr:
                            flag |= 4;
                            break;
                    }
                }
                if(!Enum.IsDefined(typeof(CacheUseMode), flag))
                {
                    continue;
                }
                else
                {
                    if(((CacheUseMode)flag) == CacheUseMode.Unknown)
                    {
                        continue;
                    }
                }
                typeToCacheUseModeMap.Add(type, (CacheUseMode)flag);

                // pick property
                var prots = type.GetProperties(BindingFlags.DeclaredOnly | BindingFlags.GetProperty | BindingFlags.Public |
                        BindingFlags.Instance);
                var propertyMap = new Dictionary<string, PropertyInfo>();
                foreach (var p in prots)
                {
                    propertyMap.Add(p.Name, p);
                }
                typeToPropertyNamesMap.Add(type, propertyMap.Keys.ToArray());
                typeToPropertiesMap.Add(type, propertyMap);
                // pick index
                var indexMap = new Dictionary<string, DBIndexAttribute>();
                object[] indices = type.GetCustomAttributes(typeof(DBIndexAttribute), false);
                List<string> fields = new List<string>();
                foreach (var idx in indices.OfType<DBIndexAttribute>())
                {
                    var isValid = idx.columnNames.Count(name => typeToPropertyNamesMap[type].Contains(name)) == idx.columnNames.Length;
                    if (!isValid)
                    {
                        throw new Exception("given column name is not property name");
                    }
                    indexMap.Add(idx.indexName, idx);
                    for (int i = 0; i < idx.columnNames.Length; i++)
                    {
                        var col = idx.columnNames[i];
                        if (!fields.Contains(col))
                        {
                            fields.Add(col);
                        }
                    }
                }
                typeToIndexNamesMap.Add(type, indexMap.Keys.ToArray());
                typeToIndicesMap.Add(type, indexMap);
            }
        }

        public static string[] GetPropertyNames(Type type)
        {
            return typeToPropertyNamesMap[type];
        }

        public static string[] GetIndexNames(Type type)
        {
            return typeToIndexNamesMap[type];
        }

        public static PropertyInfo GetPropertyInfo(Type type, string property)
        {
            typeToPropertiesMap[type].TryGetValue(property, out var info);
            return info;
        }

        public static DBIndexAttribute GetCacheIndexAttribute(Type type, string indexName)
        {
            typeToIndicesMap[type].TryGetValue(indexName, out var attr);
            return attr;
        }

        public static List<Tuple<PropertyInfo, object>> GetParameterList(Type type, string indexName, object entity)
        {
            List<Tuple<PropertyInfo, object>> parameters = new List<Tuple<PropertyInfo, object>>();
            var attr = GetCacheIndexAttribute(type, indexName);
            for (int i = 0; i < attr.columnNames.Length; i++)
            {
                var name = attr.columnNames[i];
                var info = GetPropertyInfo(type, name);
                parameters.Add(new Tuple<PropertyInfo, object>(info, info.GetValue(entity)));
            }
            return parameters;
        }

        public static IEnumerable<KeyValuePair<Type, Dictionary<string, DBIndexAttribute>>> GetDBSchemaIndicesIter()
        {
            return typeToIndicesMap;
        }

        public static Dictionary<string, DBIndexAttribute> GetDBSchemaIndicesTypeMap(Type type)
        {
            return typeToIndicesMap[type];
        }

        public static CacheUseMode GetCacheUseMode(Type type)
        {
            typeToCacheUseModeMap.TryGetValue(type, out var cacheUseMode);
            return cacheUseMode;
        }

        public static IEnumerable<KeyValuePair<Type, CacheUseMode>> GetCacheUseModesIter()
        {
            return typeToCacheUseModeMap;
        }
    }

    public class DBIndexOrder
    {
        public enum Order
        {
            Ascend = 1,
            Descend = -1,
        }

        public string columnName { private set; get; }
        public Order order { private set; get; }

        public DBIndexOrder(string columnName, Order order = Order.Ascend)
        {
            this.columnName = columnName;
            this.order = order;
        }
    }

    public enum CacheUseMode : byte
    {
        Unknown = 0,
        DBOnly = 1,
        RedisCacheOnly = 2,
        DBCache = 3,
        MemorySync = 4,
        MemorySyncByDB = 5,
        MemorySyncByRedis = 6,
    }
}
