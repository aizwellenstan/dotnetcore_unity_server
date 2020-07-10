using ETModel;
using System;
using System.Collections.Generic;
using MongoDB.Bson;
using System.Linq;

namespace ETHotfix
{
    [Event(EventIdType.PrintFullStatistic)]
    public class Event_Statistic : AEvent
    {
        public override void Run()
        {
            var lobbyComponent = Game.Scene.GetComponent<LobbyComponent>();
            if (lobbyComponent == null)
            {
                Console.WriteLine("LobbyComponent is null");
                return;
            }

            var cacheProxyComponent = Game.Scene.GetComponent<CacheProxyComponent>();
            var memorySync = cacheProxyComponent.GetMemorySyncSolver<Player>();

            BsonDocument log = new BsonDocument();

            log["nowUserOnlineCount"] = memorySync.Data.Count(e => ((Player)e.Value).isOnline);
            log["nowRoomCount"] = lobbyComponent.GetAllRoomCount();

            Console.WriteLine($"{log.ToJson()}");
        }
    }
}
