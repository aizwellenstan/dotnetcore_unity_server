using ETModel;

namespace ETHotfix
{
    [ObjectSystem]
    public class RoomTeamComponentAwakeSystem : AwakeSystem<RoomTeamComponent, TeamRoomData>
    {
        public override void Awake(RoomTeamComponent self, TeamRoomData teamRoomData)
        {
            self.Data = teamRoomData;
            self.DataIsLoaded = false;
            self.SetReservationMember(null);
            self.MemberDatas = new TeamMemberData[RoomTeamComponent.MEMBER_MAX];
            self.MemberDataUidDict.Clear();
            self.BattleLeaderboardUnitInfos.Clear();
            self.RoomEntity = self.Entity as Room;
            self.RoomRunStartTimeMs = -1;
            self.RoadDistance_km = 0;
            self.RoadDistance_m = 0;
        }
    }

    [ObjectSystem]
    public class RoomTeamComponentUpdateSystem : UpdateSystem<RoomTeamComponent>
    {
        public override void Update(RoomTeamComponent self)
        {
            if (self.RoomEntity != null)
            {
                switch (self.RoomEntity.State)
                {
                    case RoomState.Run:
                        {
                            //判斷是否正在載入比賽
                            if (!self.DataIsLoaded)
                            {
                                if (TimeHelper.ClientNowMilliSeconds() > self.RoomRunLoadingOutTimeMs)
                                {
                                    //踢掉還沒載入的
                                    for (int i = 0; i < RoomTeamComponent.MEMBER_MAX; i++)
                                    {
                                        if (self.MemberDatas[i] == null)
                                            continue;

                                        if (!self.MemberDatas[i].LoadingDone)
                                        {
                                            MapUnit kickMember = self.RoomEntity.GetMapUnitByUid(self.MemberDatas[i].Uid);
                                            RoomTeamHelper.KickMember(self, kickMember, TeamLoseType.LoadingTimeOut);
                                        }
                                    }
                                    //讓隊伍進入比賽
                                    RoomTeamHelper.LoadingFinish(self);
                                }
                            }

                            //判斷是否結束比賽
                            if (self.IsEnd())
                            {
                                for (int i = 0; i < self.RoomEntity.MapUnitList.Count; i++)
                                {
                                    self.BattleLeaderboardUnitInfos.Add(new BattleLeaderboardUnitInfo()
                                    {
	                                    Uid = self.RoomEntity.MapUnitList[i].Uid,
	                                    Name = self.RoomEntity.MapUnitList[i].Info.Name,
                                        DistanceTravelledTarget = self.RoomEntity.MapUnitList[i].Info.DistanceTravelled,
                                        Location = self.RoomEntity.MapUnitList[i].Info.Location,
                                    });

                                    //設定結束騎乘時間
                                    self.RoomEntity.MapUnitList[i].TrySetEndTime();
                                }

                                //排序 依照DistanceTravelledTarget(大到小)
                                for (int i = 0; i < self.BattleLeaderboardUnitInfos.Count; i++)
                                {
                                    for (int m = i + 1; m < self.BattleLeaderboardUnitInfos.Count; m++)
                                    {
                                        if (self.BattleLeaderboardUnitInfos[m].DistanceTravelledTarget >
                                            self.BattleLeaderboardUnitInfos[i].DistanceTravelledTarget)
                                        {
                                            var temp = self.BattleLeaderboardUnitInfos[i];
                                            self.BattleLeaderboardUnitInfos[i] = self.BattleLeaderboardUnitInfos[m];
                                            self.BattleLeaderboardUnitInfos[m] = temp;
                                        }
                                    }
                                }

                                //寫入排名
                                for (int i = 0; i < self.BattleLeaderboardUnitInfos.Count; i++)
                                {
                                    for (int m = 0; m < self.RoomEntity.MapUnitList.Count; m++)
                                    {
                                        if (self.RoomEntity.MapUnitList[m].Uid == self.BattleLeaderboardUnitInfos[i].Uid)
                                        {
                                            self.RoomEntity.MapUnitList[m].SetRank(i + 1);
                                            break;
                                        }
                                    }
                                }

                                // 紀錄隊伍資訊
                                var teamId = RideInfoHelper.SaveRideTeamRecord(self.BattleLeaderboardUnitInfos);

                                // 紀錄騎乘資訊
                                for (int i = 0; i < self.RoomEntity.MapUnitList.Count; i++)
                                {
                                    RideInfoHelper.SaveRoadAllInfo(self.RoomEntity.MapUnitList[i], teamId);
                                }

                                //傳送排行榜
                                var m2c_BattleLeaderboard = new M2C_BattleLeaderboard();
                                m2c_BattleLeaderboard.BattleLeaderboardUnitInfos.AddRange(self.BattleLeaderboardUnitInfos);
                                MapMessageHelper.BroadcastRoom(self.RoomEntity.info.RoomId, m2c_BattleLeaderboard);

                                //結束
                                self.RoomEntity.SwitchState(RoomState.End);

                                Game.Scene.GetComponent<RoomComponent>().Update(self.RoomEntity).Coroutine();
                            }
                        }
                        break;
                }
            }
        }


    }

    public static class RoomTeamComponentSystem
    {
        public static void LeaveTeam(this RoomTeamComponent self, MapUnit leaveMapUnit)
        {
            // 把離開房間的MapUnit設為null
            for (int i = 0; i < RoomTeamComponent.MEMBER_MAX; i++)
            {
                if (self.MemberDatas[i] == null)
                    continue;

                if (!self.MemberDatas[i].LoadingDone)
                {
                    MapUnit kickMember = self.RoomEntity.GetMapUnitByUid(self.MemberDatas[i].Uid);
                    if(leaveMapUnit?.Uid == kickMember?.Uid)
                    {
                        self.MemberDatas[i] = null;
                    }
                }
            }

            // 判斷是否全部載入完畢
            // 讓隊伍進入比賽
            if (self.IsAllLoadingDone())
            {
                RoomTeamHelper.LoadingFinish(self);
            }
        }
    }
}