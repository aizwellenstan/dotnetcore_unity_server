using Google.Protobuf.Collections;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace ETModel
{
	public static class OtherHelper
    {
        public static T Search<T>(List<T> list, Predicate<T> predicate)
        {
            if(list == null)
            {
                return default;
            }
            for(int i = 0; i < list.Count; i++)
            {
                T obj = list[i];
                if (predicate.Invoke(obj))
                {
                    return obj;
                }
            }
            return default;
        }

        public static List<T> SearchAll<T>(List<T> list, Predicate<T> predicate)
        {
            List<T> results = new List<T>();
            if (list == null)
            {
                return results;
            }
            for (int i = 0; i < list.Count; i++)
            {
                T obj = list[i];
                if (predicate.Invoke(obj))
                {
                    results.Add(obj);
                }
            }
            return results;
        }

        public static T Search<T>(T[] array, Predicate<T> predicate)
        {
            if (array == null)
            {
                return default;
            }
            for (int i = 0; i < array.Length; i++)
            {
                T obj = array[i];
                if (predicate.Invoke(obj))
                {
                    return obj;
                }
            }
            return default;
        }

        public static T Search<T>(RepeatedField<T> list, Predicate<T> predicate)
        {
            if (list == null)
            {
                return default;
            }
            for (int i = 0; i < list.Count; i++)
            {
                T obj = list[i];
                if (predicate.Invoke(obj))
                {
                    return obj;
                }
            }
            return default;
        }


        public static Dictionary<K, List<V>> Group<K, V>(List<V> list, Func<V, K> func)
        {
            if (list == null)
            {
                return default;
            }
            return list.Aggregate(new Dictionary<K, List<V>>(), (map, item) =>
            {
                var key = func.Invoke(item);
                if (!map.ContainsKey(key))
                {
                    map.Add(key, new List<V>());
                }
                map[key].Add(item);
                return map;
            });
        }

        public static string GetCallStackMessage()
        {
            StackTrace stackTrace = new StackTrace();           // get call stack
            StackFrame[] stackFrames = stackTrace.GetFrames();  // get method calls (frames)
            List<string> list = new List<string>(stackFrames.Length);

            // write call stack method names
            foreach (StackFrame stackFrame in stackFrames)
            {
                list.Add(new StackTrace(stackFrame).ToString());
            }
            return string.Join("\r\n", list);
        }

        public static void ShowCallStackMessage()
        {
            Console.WriteLine(GetCallStackMessage());   // write method name
        }

        public static void LogCallStackMessage(string tag)
        {
            Log.Trace($"-------------{tag}-------------\r\n");
            Log.Trace(GetCallStackMessage());   // write method name
        }

        public static T CopyDeep<T>(T targetObj)
        {
            return BsonSerializer.Deserialize<T>(targetObj.ToJson());
        }

        public static BsonDocument GetDifferenceDocument(this Entity entity, Entity target)
        {
            var doc = entity.ToBsonDocument();
            return (BsonDocument)doc.Except(target.ToBsonDocument());
        }
    }
}
