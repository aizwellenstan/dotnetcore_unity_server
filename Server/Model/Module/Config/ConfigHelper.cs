using System;
using System.IO;

namespace ETModel
{
	public static class ConfigHelper
	{
        public static string ConfigPath = $"../Config/{0}.txt";
        public async static ETTask<string> GetTextAsync(string key)
		{
            string path = string.Format(ConfigPath, key);
            try
			{
				string configStr = await File.ReadAllTextAsync(path);
				return configStr;
			}
			catch (Exception e)
			{
				throw new Exception($"load config file fail, path: {path} {e}");
			}
		}

		public static string GetText(string key)
		{
			string path = string.Format(ConfigPath, key);
			try
			{
				string configStr = File.ReadAllText(path);
				return configStr;
			}
			catch (Exception e)
			{
				throw new Exception($"load config file fail, path: {path} {e}");
			}
		}

		public static T ToObject<T>(string str)
		{
			return MongoHelper.FromJson<T>(str);
		}
	}
}
