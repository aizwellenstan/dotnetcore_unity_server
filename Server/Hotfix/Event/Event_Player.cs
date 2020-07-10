using ETModel;
using System;
using System.Collections.Generic;
using MongoDB.Bson;
using System.Linq;

namespace ETHotfix
{
    [Event(EventIdType.ShowPlayer)]
    public class Event_ShowPlayer : AEvent<long>
    {
        public override void Run(long uid)
        {
            var cacheProxyComponent = Game.Scene.GetComponent<CacheProxyComponent>();
            if (cacheProxyComponent == null)
            {
                Console.WriteLine("cacheProxyComponent is null");
                return;
            }
            var proxy = cacheProxyComponent.GetMemorySyncSolver<Player>();
            if (proxy == null)
            {
                Console.WriteLine("cache proxy is null");
                return;
            }
            var player = proxy.Get<Player>(uid);
            if (player == null)
            {
                Console.WriteLine("player is null");
                return;
            }
            Console.WriteLine($"Player[{uid}]: {player.ToJson()}");
        }
    }
}
