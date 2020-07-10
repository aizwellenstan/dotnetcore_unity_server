using ETModel;
using System;
using System.Collections.Generic;
using MongoDB.Bson;
using System.Linq;

namespace ETHotfix
{
    [Event(EventIdType.ShowMapUnit)]
    public class Event_ShowMapUnit : AEvent<long>
    {
        public override void Run(long mapUnitId)
        {
            var mapUnitComponent = Game.Scene.GetComponent<MapUnitComponent>();
            if (mapUnitComponent == null)
            {
                Console.WriteLine("MapUnitComponent is null");
                return;
            }
            var mapUnit = mapUnitComponent.Get(mapUnitId);
            if(mapUnit == null)
            {
                Console.WriteLine("mapUnit is null");
                return;
            }
            if (mapUnit.Block == null)
            {
                Console.WriteLine("block is null");
                return;
            }
            Console.WriteLine($"MapUnit[{mapUnitId}]-> Info:{mapUnit.Info.ToJson()}, GlobalInfo:{mapUnit.GlobalInfo.ToJson()};  Block-> minRoadDistance: {mapUnit.Block._minRoadDistance_m}m, maxRoadDistance: {mapUnit.Block._maxRoadDistance_m}m");
        }
    }

    [Event(EventIdType.RemoveMapunit)]
    public class Event_RemoveMapunit : AEvent
    {
        public override void Run()
        {
            var lobbyComponent = Game.Scene.GetComponent<LobbyComponent>();
            if(lobbyComponent == null)
            {
                Console.WriteLine("LobbyComponent is null");
                return;
            }

            var cacheProxyComponent = Game.Scene.GetComponent<CacheProxyComponent>();
            var memorySync = cacheProxyComponent.GetMemorySyncSolver<Player>();

            foreach(var data in memorySync.Data)
            {
                Player player = data.Value as Player;
                if (!player.isOnline)
                {
                    NetworkHelper.DisconnectPlayer(player);
                }
            }
            Console.WriteLine($"ok");
        }
    }
}
