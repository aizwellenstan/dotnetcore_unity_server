using System;
using System.Net;
using System.Collections.Generic;
using System.Threading.Tasks;
using ETModel;
using MongoDB.Bson;
using System.Linq;

namespace ETHotfix
{
    [ObjectSystem]
    public class RoamingBotModuleAwakeSystem : AwakeSystem<RoamingBotModule>
    {
        public override void Awake(RoamingBotModule self)
        {
            self.Awake();
        }
    }

    //[ObjectSystem]
    //public class RoamingBotModuleUpdateSystem : UpdateSystem<RoamingBotModule>
    //{
    //    public override void Update(RoamingBotModule self)
    //    {
    //        self.Update();
    //    }
    //}

    [ObjectSystem]
    public class RoamingBotModuleDestroySystem : DestroySystem<RoamingBotModule>
    {
        public override void Destroy(RoamingBotModule self)
        {
            self.Destroy();
        }
    }

    public class RoamingBotModule : Component, BenchmarkComponent.IUpdate
    {
        BenchmarkClientComponent parent;

        TimerComponent timerComponent;

        MapUnitBotModule mapUnitBotModule;

        Session session;

        private readonly Random random = new Random();

        #region My MapUnit data

        private readonly C2M_MapUnitMove _c2m_MapUnitMove = new C2M_MapUnitMove();

        public const float InputAsyncTimeInterval = 0.2f;

        private float _inputAsyncTimeAfter = 0;

        private float _inputAsyncTimePre = 0.5f;

        private float _nowSpeed = 0;

        private float _preSpeed = 0;

        private long mapUnitId = 0;

        private double DistanceTravelled;

        #endregion

        public async void Awake()
        {
            parent = GetParent<BenchmarkClientComponent>();
            timerComponent = Game.Scene.GetComponent<TimerComponent>();
            session = parent.session;
            mapUnitBotModule = parent.GetComponent<MapUnitBotModule>();

            L2C_RoamingGetList l2C_RoamingGetList = await RoamingUtility.GetMapList(session);
            if (l2C_RoamingGetList.Error != ErrorCode.ERR_Success)
            {
                Console.WriteLine($"To get roaming road list Failed");
                return;
            }
            var info = l2C_RoamingGetList.Infos.FirstOrDefault(e => e.RoadSettingId == parent.roadSettingId);
            long roomId = info != null ? info.RoomId : 0L;
            L2C_RoamingEnter l2C_RoamingEnter = await RoamingUtility.EnterRoamingRoom(session, roomId);
            if (l2C_RoamingEnter.Error != ErrorCode.ERR_Success)
            {
                Console.WriteLine($"To enter roaming room[{roomId}] Failed. Error:{l2C_RoamingEnter.Error}");
                return;
            }

            //記錄自身MapUnitId
            mapUnitId = l2C_RoamingEnter.SelfInfo.MapUnitId;

            //記錄自身MapUnitInfo
            mapUnitBotModule.mapUnitInfos.TryAdd(mapUnitId, new MapUnitBotModule.MapUnitData
            {
                mapUnitInfo = l2C_RoamingEnter.SelfInfo,
                m2C_MapUnitUpdate = null,
            });

            _nowSpeed = random.Next(5, 30);

            mapUnitBotModule.EnableGame(true);
        }

        public void Update()
        {
            if (!mapUnitBotModule.isEnableToStartGame)
                return;

            //向Server同步資料
            if (timerComponent.time > _inputAsyncTimeAfter)
            {
                if (_nowSpeed < 0.05f) _nowSpeed = 0;
                if (Math.Abs(_preSpeed - _nowSpeed) > 0.0000000001f || _nowSpeed > 0)
                {
                    if (mapUnitBotModule.mapUnitInfos.ContainsKey(mapUnitId))
                    {
                        DistanceTravelled = DistanceTravelled + _nowSpeed * (timerComponent.time - _inputAsyncTimePre);
                        _c2m_MapUnitMove.DistanceTravelledTarget = DistanceTravelled;
                        _c2m_MapUnitMove.SpeedMS = _nowSpeed;

                        try
                        {
                            session.Send(_c2m_MapUnitMove);
                        }
                        catch (Exception e)
                        {
                            Log.Error(e);
                        }
                    }
                }
                _preSpeed = _nowSpeed;
                _inputAsyncTimePre = timerComponent.time;
                _inputAsyncTimeAfter = InputAsyncTimeInterval + timerComponent.time;
            }
        }

        public void Destroy()
        {
            mapUnitBotModule.mapUnitInfos.Clear();
        }

        public string GetMessage()
        {
            return $"DistanceTravelled:{DistanceTravelled}";
        }
    }
}