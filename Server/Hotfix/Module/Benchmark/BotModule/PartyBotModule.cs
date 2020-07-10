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
    public class PartyBotModuleAwakeSystem : AwakeSystem<PartyBotModule>
    {
        public override void Awake(PartyBotModule self)
        {
            self.Awake();
        }
    }

    //[ObjectSystem]
    //public class PartyBotModuleUpdateSystem : UpdateSystem<PartyBotModule>
    //{
    //    public override void Update(PartyBotModule self)
    //    {
    //        self.Update();
    //    }
    //}

    [ObjectSystem]
    public class PartyBotModuleDestroySystem : DestroySystem<PartyBotModule>
    {
        public override void Destroy(PartyBotModule self)
        {
            self.Destroy();
        }
    }

    public class PartyBotModule : Component, BenchmarkComponent.IUpdate
    {
        BenchmarkClientComponent parent;

        TimerComponent timerComponent;

        MapUnitBotModule mapUnitBotModule;

        Session session;

        BenchmarkComponent benchmarkComponent;

        Dictionary<long, Team> teams = new Dictionary<long, Team>();

        private readonly Random random = new Random();

        #region My MapUnit data

        private readonly C2M_MapUnitMove _c2m_MapUnitMove = new C2M_MapUnitMove();

        public const float InputAsyncTimeInterval = 0.2f;

        private float _inputAsyncTimeAfter = 0;

        private float _inputAsyncTimePre = 0.5f;

        private float _nowSpeed = 0;

        private float _preSpeed = 0;

        private double DistanceTravelled;

        #endregion

        private long roomId;

        private bool isLeader = false;

        State state { set; get; }

        public class MapUnitData
        {
            public MapUnitInfo mapUnitInfo;

            public M2C_MapUnitUpdate m2C_MapUnitUpdate;
        }

        public class Team
        {
            public RoomInfo Info;

            public TeamRoomData Data;

            public List<TeamMemberData> teamMemberDataList;
        }

        enum State
        {
            CreateOrEnterRoom,
            OnTeam,
            GoBattle,
            Battle,
            Waiting,
        }

        public void Awake()
        {
            parent = GetParent<BenchmarkClientComponent>();
            timerComponent = Game.Scene.GetComponent<TimerComponent>();
            session = parent.session;
            benchmarkComponent = Game.Scene.GetComponent<BenchmarkComponent>();
            mapUnitBotModule = parent.GetComponent<MapUnitBotModule>();

            state = State.CreateOrEnterRoom;

            //_nowSpeed = random.Next(5, 30);
            _nowSpeed = 1;

            //while (!IsDisposed)
            //{
            //    await timerComponent.WaitForSecondAsync(5);
            //    Console.WriteLine($"Name:{parent.userName}, State:{state}");
            //}
        }

        public async void Update()
        {
            switch (state)
            {
                case State.CreateOrEnterRoom:

                    state = State.Waiting;

                    if (benchmarkComponent.groupToTaskControllerMap.TryGetValue(parent.index / 8, out var src))
                    {
                        if (src.Count != 0)
                        {
                            await src[parent.index % 8].Task;
                            await timerComponent.WaitForSecondAsync(5);
                        }
                        else
                        {
                            var list = benchmarkComponent.groupToTaskControllerMap[parent.index / 8];
                            for (int i = 0; i < 8; i++)
                            {
                                list.Add(new ETTaskCompletionSource());
                            }
                        }
                    }
                    else
                    {
                        var list = new List<ETTaskCompletionSource>();
                        for (int i = 0; i < 8; i++)
                        {
                            list.Add(new ETTaskCompletionSource());
                        }
                        benchmarkComponent.groupToTaskControllerMap.Add(parent.index / 8, list);
                    }

                    //Get team list
                    await GetTeamList();

                    //Create or join team
                    if (teams.Count == 0)
                    {
                        await CreateTeam(1000L);
                        foreach(var task in benchmarkComponent.groupToTaskControllerMap[parent.index / 8])
                        {
                            task.TrySetResult();
                        }
                    }
                    else
                    {
                        if (benchmarkComponent.groupToRoomMap.TryGetValue(parent.index / 8, out var roomId))
                        {
                            var team = teams[roomId];
                            await TeamEnter(team.Info.RoomId);
                            await timerComponent.WaitForSecondAsync(5);
                            await ReadyOnTeam(true);
                        }
                        else
                        {
                            await CreateTeam(1000L);
                            foreach (var task in benchmarkComponent.groupToTaskControllerMap[parent.index / 8])
                            {
                                task.TrySetResult();
                            }
                        }
                    }

                    state = State.OnTeam;

                    break;
                case State.OnTeam:

                    if (isLeader)
                    {
                        if (teams.TryGetValue(roomId, out var team))
                        {
                            if (team.Info.NowMemberCount == team.Info.MaxMemberCount)
                            {
                                //parent.UserLog($"Ready:{team.teamMemberDataList.Select(e => e.IsReady).ToList().ToJson()}");
                                if (team.teamMemberDataList.All(e => e.IsReady))
                                {
                                    state = State.Waiting;

                                    benchmarkComponent.groupToTaskControllerMap[parent.index / 8].Clear();

                                    await TeamRun();
                                }
                            }
                            else
                            {
                                //parent.UserLog($"**RoomId:{roomId}, NowMemberCount:{team.Info.NowMemberCount}, MaxMemberCount:{team.Info.MaxMemberCount}**");
                            }
                        }
                        else
                        {
                            parent.UserLog($"team[roomId:{roomId}] is nonexistent");
                        }
                    }
                    else
                    {
                        state = State.Waiting;
                    }

                    break;
                case State.GoBattle:

                    state = State.Waiting;

                    await SendTeamGoBattleProgress(0.3f);

                    await SendTeamGoBattleProgress(0.6f);

                    await SendTeamGoBattleProgress(0.9f);

                    await SendTeamGoBattleProgress(1.0f);

                    mapUnitBotModule.EnableGame(true);

                    break;
                case State.Battle:

                    if (!mapUnitBotModule.isEnableToStartGame)
                        return;

                    //向Server同步資料
                    if (timerComponent.time > _inputAsyncTimeAfter)
                    {
                        if (_nowSpeed < 0.05f) _nowSpeed = 0;
                        if (Math.Abs(_preSpeed - _nowSpeed) > 0.0000000001f || _nowSpeed > 0)
                        {
                            if (mapUnitBotModule.mapUnitInfos.ContainsKey(mapUnitBotModule.mapUnitId))
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
                                    //parent.UserLog($"Msg:{e.Message}, Stack:{e.StackTrace}");
                                    Log.Error(e);
                                }
                            }
                        }
                        _preSpeed = _nowSpeed;
                        _inputAsyncTimePre = timerComponent.time;
                        _inputAsyncTimeAfter = InputAsyncTimeInterval + timerComponent.time;
                    }
                    //parent.UserLog($"State.Battle:{DistanceTravelled}");

                    break;
                case State.Waiting:
                    //
                    break;
            }
        }

        public void Destroy()
        {
            mapUnitBotModule.mapUnitInfos.Clear();
            
        }

        public string GetMessage()
        {
            return $"Current state:{state}, DistanceTravelled:{DistanceTravelled}";
        }

        private async ETTask GetTeamList()
        {
            L2C_TeamGetList l2C_TeamGetList = await session.Call(new C2L_TeamGetList()
            {
                IsReservation = false,
            }) as L2C_TeamGetList;
            if (l2C_TeamGetList.Error != ErrorCode.ERR_Success)
            {
                Console.WriteLine($"To get team list Failed");
                return;
            }
            for (int i = 0; i < l2C_TeamGetList.Infos.Count; i++)
            {
                if(teams.TryGetValue(l2C_TeamGetList.Infos[i].RoomId, out var team))
                {
                    team.Info = l2C_TeamGetList.Infos[i];
                    team.Data = l2C_TeamGetList.Datas[i];
                }
                else
                {
                    teams.Add(l2C_TeamGetList.Infos[i].RoomId, new Team
                    {
                        Info = l2C_TeamGetList.Infos[i],
                        Data = l2C_TeamGetList.Datas[i],
                    });
                }
            }
            //Console.WriteLine($"Name:{parent.userName}:GetTeamList");
        }

        private async ETTask CreateTeam(long RoadSettingId)
        {
            L2C_TeamCreate l2C_TeamCreate = await session.Call(new C2L_TeamCreate()
            {
                RoadSettingId = RoadSettingId,
            }) as L2C_TeamCreate;
            if (l2C_TeamCreate.Error != ErrorCode.ERR_Success)
            {
                Console.WriteLine($"To create team[RoadSettingId:{RoadSettingId}] Failed");
                return;
            }
            this.roomId = l2C_TeamCreate.Info.RoomId;
            this.isLeader = true;
            teams.Add(l2C_TeamCreate.Info.RoomId, new Team
            {
                Info = l2C_TeamCreate.Info,
                Data = l2C_TeamCreate.Data,
                teamMemberDataList = new List<TeamMemberData>
                {
                    new TeamMemberData
                    {
                        IsReady = true,
                        Uid = l2C_TeamCreate.Data.LeaderUid,
                        MemberIndex = 0,
                    }
                },
            });
            benchmarkComponent.groupToRoomMap.Add(parent.index / 8, l2C_TeamCreate.Info.RoomId);
            //Console.WriteLine($"Name:{parent.userName}:CreateTeam");
        }

        private async ETTask TeamEnter(long roomId)
        {
            L2C_TeamEnter l2C_TeamEnter = await session.Call(new C2L_TeamEnter()
            {
                TeamRoomId = roomId
            }) as L2C_TeamEnter;
            if (l2C_TeamEnter.Error != ErrorCode.ERR_Success)
            {
                Console.WriteLine($"To enter team[ID:{roomId}] Failed! Reason: Error code[{l2C_TeamEnter.Error}]");
                return;
            }
            this.roomId = roomId;
            if (teams.TryGetValue(roomId, out var team))
            {
                team.Info = l2C_TeamEnter.Info;
                team.Data = l2C_TeamEnter.Data;
                team.teamMemberDataList = l2C_TeamEnter.MemberDatas.ToList();
            }
            else
            {
                Console.WriteLine($"To enter team[ID:{roomId}] Failed! Reason: room is nonexistent");
            }
            //Console.WriteLine($"Name:{parent.userName}:TeamEnter, RoomId:{roomId}, NowCount:{l2C_TeamEnter.Info.NowMemberCount}");
        }

        private async ETTask ReadyOnTeam(bool isReady)
        {
            M2C_TeamReady m2C_TeamReady = await session.Call(new C2M_TeamReady()
            {
                IsReady = isReady
            }) as M2C_TeamReady;
            if (m2C_TeamReady.Error != ErrorCode.ERR_Success)
            {
                Console.WriteLine($"Name:{parent.userName}:To ready on team[roomId:{roomId}] is Failed! Reason: Error code[{m2C_TeamReady.Error}]");
                return;
            }
        }

        private async ETTask TeamRun()
        {
            M2C_TeamRun m2C_TeamRun = await session.Call(new C2M_TeamRun()) as M2C_TeamRun;
            if (m2C_TeamRun.Error != ErrorCode.ERR_Success)
            {
                Console.WriteLine($"To go to battlement on team[roomId:{roomId}] is Failed! Reason: Error code[{m2C_TeamRun.Error}]");
                return;
            }
            //Console.WriteLine($"Name:{parent.userName}:TeamRun");
        }

        private async ETTask SendTeamGoBattleProgress(float progress)
        {
            await timerComponent.WaitForSecondAsync(0.5f);

            session.Send(new C2M_TeamGoBattleProgress
            {
                Progress = progress,
            });
        }

        #region Actor Message

        [MessageHandler(AppType.Benchmark)]
        public class M2C_TeamModifyMemberHandler : AMHandler<M2C_TeamModifyMember>
        {
            protected override void Run(Session session, M2C_TeamModifyMember message)
            {
                try
                {
                    _Run(session, message);
                }
                catch(Exception e)
                {
                    TestPlayerDataComponent testPlayerDataComponent = session.GetComponent<TestPlayerDataComponent>();
                    BenchmarkComponent benchmarkComponent = Game.Scene.GetComponent<BenchmarkComponent>();
                    if (benchmarkComponent.clients.TryGetValue(testPlayerDataComponent.testPlayerSetting.DeviceUniqueIdentifier, out var client))
                    {
                        client.UserLog($"Msg:{e.Message}, Stack:{e.StackTrace}");
                    }
                }
            }

            protected void _Run(Session session, M2C_TeamModifyMember message)
            {
                TestPlayerDataComponent testPlayerDataComponent = session.GetComponent<TestPlayerDataComponent>();
                BenchmarkComponent benchmarkComponent = Game.Scene.GetComponent<BenchmarkComponent>();
                if (benchmarkComponent.clients.TryGetValue(testPlayerDataComponent.testPlayerSetting.DeviceUniqueIdentifier, out var client))
                {
                    PartyBotModule partyBotModule = client.GetComponent<PartyBotModule>();
                    if (partyBotModule.teams.TryGetValue(partyBotModule.roomId, out var team))
                    {
                        bool isAdd = true;

                        for (int i = 0; i < team.teamMemberDataList.Count; i++)
                        {
                            if (team.teamMemberDataList[i].Uid == message.Uid)
                            {
                                if (message.MemberData == null)
                                {
                                    //刪除
                                    isAdd = false;
                                    team.teamMemberDataList.RemoveAt(i);
                                    team.Info.NowMemberCount--;
                                    //client.UserLog($"RoomId:{partyBotModule.roomId}, NowMemberCount:{team.Info.NowMemberCount}");
                                }
                                else
                                {
                                    //更新
                                    isAdd = false;
                                    team.teamMemberDataList[i] = message.MemberData;
                                }
                                break;
                            }
                        }

                        if (isAdd)
                        {
                            team.teamMemberDataList.Add(message.MemberData);
                            team.Info.NowMemberCount++;
                            //client.UserLog($"RoomId:{partyBotModule.roomId}, NowMemberCount:{team.Info.NowMemberCount}");
                        }
                    }
                    //client.UserLog($"M2C_TeamModifyMemberHandler");
                }
            }
        }

        [MessageHandler(AppType.Benchmark)]
        public class M2C_TeamReadyModifyHandler : AMHandler<M2C_TeamReadyModify>
        {
            protected override void Run(Session session, M2C_TeamReadyModify message)
            {
                TestPlayerDataComponent testPlayerDataComponent = session.GetComponent<TestPlayerDataComponent>();
                BenchmarkComponent benchmarkComponent = Game.Scene.GetComponent<BenchmarkComponent>();
                if (benchmarkComponent.clients.TryGetValue(testPlayerDataComponent.testPlayerSetting.DeviceUniqueIdentifier, out var client))
                {
                    PartyBotModule partyBotModule = client.GetComponent<PartyBotModule>();
                    if (partyBotModule.teams.TryGetValue(partyBotModule.roomId, out var team))
                    {
                        for (int i = 0; i < team.teamMemberDataList.Count; i++)
                        {
                            if (team.teamMemberDataList[i].Uid == message.Uid)
                            {
                                team.teamMemberDataList[i].IsReady = message.IsReady;
                                break;
                            }
                        }
                    }
                    //client.UserLog($"M2C_TeamReadyModifyHandler");
                }
            }
        }

        [MessageHandler(AppType.Benchmark)]
        public class M2C_TeamGoBattleHandler : AMHandler<M2C_TeamGoBattle>
        {
            protected override void Run(Session session, M2C_TeamGoBattle message)
            {
                TestPlayerDataComponent testPlayerDataComponent = session.GetComponent<TestPlayerDataComponent>();
                BenchmarkComponent benchmarkComponent = Game.Scene.GetComponent<BenchmarkComponent>();
                if (benchmarkComponent.clients.TryGetValue(testPlayerDataComponent.testPlayerSetting.DeviceUniqueIdentifier, out var client))
                {
                    PartyBotModule partyBotModule = client.GetComponent<PartyBotModule>();
                    MapUnitBotModule mapUnitBotModule = client.GetComponent<MapUnitBotModule>();
                    mapUnitBotModule.mapUnitId = message.MapUnitId;
                    for (int i = 0; i < message.MapUnitInfos.Count; i++)
                    {
                        var info = message.MapUnitInfos[i];
                        mapUnitBotModule.mapUnitInfos.TryAdd(info.MapUnitId, new MapUnitBotModule.MapUnitData
                        {
                            mapUnitInfo = info,
                        });
                    }
                    partyBotModule.state = State.GoBattle;
                    partyBotModule._inputAsyncTimePre = partyBotModule.timerComponent.time;
                    //client.UserLog($"M2C_TeamGoBattleHandler");
                }
            }
        }

        [MessageHandler(AppType.Benchmark)]
        public class M2C_TeamGoBattleProgressReceiverHandler : AMHandler<M2C_TeamGoBattleProgressReceiver>
        {
            protected override void Run(Session session, M2C_TeamGoBattleProgressReceiver message)
            {
                TestPlayerDataComponent testPlayerDataComponent = session.GetComponent<TestPlayerDataComponent>();
                BenchmarkComponent benchmarkComponent = Game.Scene.GetComponent<BenchmarkComponent>();
                if (benchmarkComponent.clients.TryGetValue(testPlayerDataComponent.testPlayerSetting.DeviceUniqueIdentifier, out var client))
                {
                    //client.UserLog($"M2C_TeamGoBattleProgressReceiverHandler");
                }
            }
        }

        [MessageHandler(AppType.Benchmark)]
        public class M2C_TeamGoBattleProgressAllDoneHandler : AMHandler<M2C_TeamGoBattleProgressAllDone>
        {
            protected override void Run(Session session, M2C_TeamGoBattleProgressAllDone message)
            {
                TestPlayerDataComponent testPlayerDataComponent = session.GetComponent<TestPlayerDataComponent>();
                BenchmarkComponent benchmarkComponent = Game.Scene.GetComponent<BenchmarkComponent>();
                if (benchmarkComponent.clients.TryGetValue(testPlayerDataComponent.testPlayerSetting.DeviceUniqueIdentifier, out var client))
                {
                    PartyBotModule partyBotModule = client.GetComponent<PartyBotModule>();
                    partyBotModule.state = State.Battle;
                    if (partyBotModule.isLeader)
                    {
                        benchmarkComponent.groupToRoomMap.Remove(client.index / 8);
                    }
                    //client.UserLog($"M2C_TeamGoBattleProgressAllDoneHandler");
                }
            }
        }

        #endregion
    }
}