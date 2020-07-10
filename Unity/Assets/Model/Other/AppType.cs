using System;
using System.Collections.Generic;

namespace ETModel
{
	[Flags]
	public enum AppType
	{
		None = 0,
		Manager = 1,
		Realm = 1 << 1,
		Gate = 1 << 2,
		Http = 1 << 3,
		DB = 1 << 4,
		Location = 1 << 5,
		Map = 1 << 6,
        Master = 1 << 7,
        Lobby = 1 << 8,

		BenchmarkWebsocketServer = 1 << 26,
		BenchmarkWebsocketClient = 1 << 27,
		Robot = 1 << 28,
		Benchmark = 1 << 29,
		// 客户端Hotfix层
		ClientH = 1 << 30,
		// 客户端Model层
		ClientM = 1 << 31,

		// 7
		AllServer = Manager | Realm | Gate | Http | DB | Location | Map | BenchmarkWebsocketServer | Master | Lobby
    }

	public static class AppTypeHelper
	{
		public static List<AppType> GetServerTypes()
		{
			List<AppType> appTypes = new List<AppType> { AppType.Manager, AppType.Realm, AppType.Gate };
			return appTypes;
		}

		public static bool Is(this AppType a, AppType b)
		{
			if ((a & b) != 0)
			{
				return true;
			}
			return false;
		}
	}
}