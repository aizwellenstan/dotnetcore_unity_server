using System;
using System.Net;
using System.Collections.Generic;
using System.Threading.Tasks;
using ETModel;
using MongoDB.Bson;

namespace ETHotfix
{
    public class MapUnitBotModule : Component
    {
        /// <summary>
        /// 還再等候Loading的時候，有封包先過來的話就先緩存
        /// </summary>
        public Queue<IActorMessage> _m2cCache = new Queue<IActorMessage>();

        /// <summary>
        /// 小地圖的單元Id
        /// </summary>
        public long mapUnitId = 0;

        /// <summary>
        /// 是否Loading好了進到比賽了?
        /// </summary>
        public bool isEnableToStartGame = false;

        public Dictionary<long, MapUnitData> mapUnitInfos = new Dictionary<long, MapUnitData>();

        public class MapUnitData
        {
            public MapUnitInfo mapUnitInfo;

            public M2C_MapUnitUpdate m2C_MapUnitUpdate;
        }

        public void EnableGame(bool isEnable)
        {
            isEnableToStartGame = isEnable;
            if (isEnableToStartGame)
            {
                MapUnitSyncAll();
            }
        }

        private void MapUnitSyncAll()
        {
            while (_m2cCache.Count > 0)
            {
                var message = _m2cCache.Dequeue();
                if (message == null)
                    continue;
                if (message is M2C_MapUnitCreateAndDestroy m2c_MapUnitCreateAndDestroy)
                {
                    SyncM2C_MapUnitCreateAndDestroy(m2c_MapUnitCreateAndDestroy);
                }
                else if (message is M2C_MapUnitCreate m2c_MapUnitCreate)
                {
                    SyncM2C_MapUnitCreate(m2c_MapUnitCreate.MapUnitInfo);
                }
                else if (message is M2C_MapUnitUpdate m2c_MapUnitUpdate)
                {
                    SyncM2C_MapUnitUpdate(m2c_MapUnitUpdate);
                }
                else if (message is M2C_MapUnitDestroy m2c_MapUnitDestroy)
                {
                    SyncM2C_MapUnitDestroy(m2c_MapUnitDestroy.MapUnitId);
                }
            }
        }

        private void SyncM2C_MapUnitCreateAndDestroy(M2C_MapUnitCreateAndDestroy message)
        {
            for (int i = 0; i < message.DestroyMapUnitIds.Count; i++)
            {
                SyncM2C_MapUnitDestroy(message.DestroyMapUnitIds[i]);
            }

            for (int i = 0; i < message.CreateMapUnitInfos.Count; i++)
            {
                SyncM2C_MapUnitCreate(message.CreateMapUnitInfos[i]);
            }
        }

        private void SyncM2C_MapUnitCreate(MapUnitInfo mapUnitInfo)
        {
            mapUnitInfos.TryAdd(mapUnitInfo.MapUnitId, new MapUnitData
            {
                mapUnitInfo = mapUnitInfo,
                m2C_MapUnitUpdate = null,
            });
        }

        private void SyncM2C_MapUnitUpdate(M2C_MapUnitUpdate message)
        {
            if (mapUnitInfos.TryGetValue(message.MapUnitId, out var mapUnitData))
            {
                if (message.MapUnitId == mapUnitId)
                {
                    return;
                }
                mapUnitData.m2C_MapUnitUpdate = message;
            }
        }

        private void SyncM2C_MapUnitDestroy(long mapUnitId)
        {
            if (mapUnitInfos.TryGetValue(mapUnitId, out var mapUnitData))
            {
                mapUnitInfos.Remove(mapUnitId);
            }
        }

        #region Actor Message

        [MessageHandler(AppType.Benchmark)]
        public class M2C_MapUnitGlobalCreateHandler : AMHandler<M2C_MapUnitGlobalCreate>
        {
            protected override void Run(Session session, M2C_MapUnitGlobalCreate message)
            {
                //TODO: M2C_MapUnitGlobalCreate
            }
        }

        [MessageHandler(AppType.Benchmark)]
        public class M2C_MapUnitGlobalDestroyHandler : AMHandler<M2C_MapUnitGlobalDestroy>
        {
            protected override void Run(Session session, M2C_MapUnitGlobalDestroy message)
            {
                //TODO: M2C_MapUnitGlobalDestroy
            }
        }

        [MessageHandler(AppType.Benchmark)]
        public class M2C_BattleLeaderboardHandler : AMHandler<M2C_BattleLeaderboard>
        {
            protected override void Run(Session session, M2C_BattleLeaderboard message)
            {
                TestPlayerDataComponent testPlayerDataComponent = session.GetComponent<TestPlayerDataComponent>();
                BenchmarkComponent benchmarkComponent = Game.Scene.GetComponent<BenchmarkComponent>();
                if (benchmarkComponent.clients.TryGetValue(testPlayerDataComponent.testPlayerSetting.DeviceUniqueIdentifier, out var client))
                {
                    client.Init();
                    //client.UserLog($"M2C_TeamGoBattleProgressReceiverHandler");
                }
            }
        }

        [MessageHandler(AppType.Benchmark)]
        public class G2C_UpdatePlayerRideInfoHandler : AMHandler<G2C_UpdatePlayerRideTotalInfo>
        {
            protected override void Run(Session session, G2C_UpdatePlayerRideTotalInfo message)
            {
                
            }
        }

        [MessageHandler(AppType.Benchmark)]
        public class M2C_MapUnitCreateHandler : AMHandler<M2C_MapUnitCreate>
        {
            protected override void Run(Session session, M2C_MapUnitCreate message)
            {
                TestPlayerDataComponent testPlayerDataComponent = session.GetComponent<TestPlayerDataComponent>();
                BenchmarkComponent benchmarkComponent = Game.Scene.GetComponent<BenchmarkComponent>();
                if (benchmarkComponent.clients.TryGetValue(testPlayerDataComponent.testPlayerSetting.DeviceUniqueIdentifier, out var client))
                {
                    MapUnitBotModule roamingBotModule = client.GetComponent<MapUnitBotModule>();
                    if (!roamingBotModule.isEnableToStartGame)
                    {
                        roamingBotModule._m2cCache.Enqueue(message);
                    }
                    else
                    {
                        roamingBotModule.SyncM2C_MapUnitCreate(message.MapUnitInfo);
                    }
                    //client.UserLog($"M2C_MapUnitCreateHandler");
                }
            }
        }

        [MessageHandler(AppType.Benchmark)]
        public class M2C_MapUnitUpdateHandler : AMHandler<M2C_MapUnitUpdate>
        {
            protected override void Run(Session session, M2C_MapUnitUpdate message)
            {
                TestPlayerDataComponent testPlayerDataComponent = session.GetComponent<TestPlayerDataComponent>();
                BenchmarkComponent benchmarkComponent = Game.Scene.GetComponent<BenchmarkComponent>();
                if (benchmarkComponent.clients.TryGetValue(testPlayerDataComponent.testPlayerSetting.DeviceUniqueIdentifier, out var client))
                {
                    MapUnitBotModule roamingBotModule = client.GetComponent<MapUnitBotModule>();
                    if (!roamingBotModule.isEnableToStartGame)
                    {
                        roamingBotModule._m2cCache.Enqueue(message);
                    }
                    else
                    {
                        roamingBotModule.SyncM2C_MapUnitUpdate(message);
                    }
                    //client.UserLog($"M2C_MapUnitUpdateHandler, DistanceTravelled: {roamingBotModule.DistanceTravelled}");
                }
            }
        }

        [MessageHandler(AppType.Benchmark)]
        public class M2C_MapUnitDestroyHandler : AMHandler<M2C_MapUnitDestroy>
        {
            protected override void Run(Session session, M2C_MapUnitDestroy message)
            {
                TestPlayerDataComponent testPlayerDataComponent = session.GetComponent<TestPlayerDataComponent>();
                BenchmarkComponent benchmarkComponent = Game.Scene.GetComponent<BenchmarkComponent>();
                if (benchmarkComponent.clients.TryGetValue(testPlayerDataComponent.testPlayerSetting.DeviceUniqueIdentifier, out var client))
                {
                    MapUnitBotModule roamingBotModule = client.GetComponent<MapUnitBotModule>();
                    if (!roamingBotModule.isEnableToStartGame)
                    {
                        roamingBotModule._m2cCache.Enqueue(message);
                    }
                    else
                    {
                        roamingBotModule.SyncM2C_MapUnitDestroy(message.MapUnitId);
                    }
                    //client.UserLog($"M2C_MapUnitDestroyHandler");
                }
            }
        }

        [MessageHandler(AppType.Benchmark)]
        public class M2C_MapUnitCreateAndDestroyHandler : AMHandler<M2C_MapUnitCreateAndDestroy>
        {
            protected override void Run(Session session, M2C_MapUnitCreateAndDestroy message)
            {
                TestPlayerDataComponent testPlayerDataComponent = session.GetComponent<TestPlayerDataComponent>();
                BenchmarkComponent benchmarkComponent = Game.Scene.GetComponent<BenchmarkComponent>();
                if (benchmarkComponent.clients.TryGetValue(testPlayerDataComponent.testPlayerSetting.DeviceUniqueIdentifier, out var client))
                {
                    MapUnitBotModule roamingBotModule = client.GetComponent<MapUnitBotModule>();
                    if (!roamingBotModule.isEnableToStartGame)
                    {
                        roamingBotModule._m2cCache.Enqueue(message);
                    }
                    else
                    {
                        for (int i = 0; i < message.DestroyMapUnitIds.Count; i++)
                        {
                            roamingBotModule.SyncM2C_MapUnitDestroy(message.DestroyMapUnitIds[i]);
                        }

                        for (int i = 0; i < message.CreateMapUnitInfos.Count; i++)
                        {
                            roamingBotModule.SyncM2C_MapUnitCreate(message.CreateMapUnitInfos[i]);
                        }
                    }
                    //client.UserLog($"M2C_MapUnitCreateAndDestroyHandler");
                }
            }
        }

        #endregion
    }
}