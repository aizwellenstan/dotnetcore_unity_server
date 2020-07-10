using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;
using ETModel;

namespace ETHotfix
{
    [ObjectSystem]
    public class PlayerComponentAwakeSystem : AwakeSystem<PlayerComponent>
    {
        public override void Awake(PlayerComponent self)
        {
            self.Awake();
        }
    }

    [ObjectSystem]
    public class PlayerComponentDestroySystem : DestroySystem<PlayerComponent>
    {
        public override void Destroy(PlayerComponent self)
        {
            self.Destroy();
        }
    }

    public static class PlayerComponentSystem
    {
        public static void Awake(this PlayerComponent self)
        {
            var proxy = Game.Scene.GetComponent<CacheProxyComponent>();
            self.MemorySync = proxy.GetMemorySyncSolver<Player>();
            self.MemorySync.onRefresh += self.OnRefresh;

            //self.Test();
        }

        //public static void Test(this PlayerComponent self)
        //{
        //    List<long> uids = new List<long>
        //    {
        //        385283104112654L,
        //        385284038459567L,
        //        385284160618576L,
        //        385284163240049L,
        //        385284180541703L,
        //        385284256104497L,
        //        385284340318336L
        //    };
        //    for(int i = 0; i < 1000; i++)
        //    {
        //        var rnd = RandomHelper.RandomNumber(0, uids.Count);
        //        var uid = uids[rnd];
        //        self.Run(uid);
        //    }
        //}

        //public static async void Run(this PlayerComponent self, long uid)
        //{
        //    System.Console.WriteLine($"uid:{uid}");
        //    //ComponentWithId entity = await Game.Scene.GetComponent<CacheProxyComponent>().QueryById<ComponentWithId>("player", uid);
        //    //System.Console.WriteLine($"uid:{uid}:QueryById");
        //    var player = ComponentFactory.CreateWithId<Player>(uid);
        //    await self.Add(player);
        //    System.Console.WriteLine($"uid:{uid}:Add");
        //    player.gateSessionActorId = 0;
        //    player.SetOnline(true);
        //    // 隨機配給MapServerID
        //    var startComponent = Game.Scene.GetComponent<StartConfigComponent>();
        //    player.mapServerId = RandomHelper.RandomNumber(0, startComponent.MapConfigs.Count);
        //    await self.Update(player);
        //    System.Console.WriteLine($"uid:{uid}:Update");
        //    await self.Remove(uid);
        //    System.Console.WriteLine($"uid:{uid}:Remove");
        //}

        public static void OnRefresh(this PlayerComponent self, long? id)
        {

        }

        public static async ETTask Add(this PlayerComponent self, Player player)
        {
            await self.MemorySync.Create(player);
        }

        public static async ETTask Update(this PlayerComponent self, Player player)
        {
            await self.MemorySync.Update(player);
        }

        public static async ETTask Remove(this PlayerComponent self, long uid)
        {
            await self.MemorySync.Delete<Player>(uid);
        }

        public static void Destroy(this PlayerComponent self)
        {
            // 預防遞歸卡死
            if (self.IsDisposed)
            {
                return;
            }

            // 非擁有者請勿操作Dispose
            self.MemorySync.Dispose();
            self.Dispose();
        }
    }
}