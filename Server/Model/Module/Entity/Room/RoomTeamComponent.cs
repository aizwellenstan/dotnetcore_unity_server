using ETHotfix;
using Google.Protobuf.Collections;
using System.Collections.Generic;

namespace ETModel
{
    public class RoomTeamComponent : Component/*, ISerializeToEntity*/
	{
        public const int MEMBER_MAX = 8;

        public TeamRoomData Data = null;
        public TeamMemberData[] MemberDatas = null;
        public Dictionary<long, TeamMemberData> MemberDataUidDict  = new Dictionary<long, TeamMemberData>();
        public RepeatedField<BattleLeaderboardUnitInfo> BattleLeaderboardUnitInfos = new RepeatedField<BattleLeaderboardUnitInfo>();

        public const long RoomRunLoadingMaxTimeMs = 30000;

        public bool DataIsLoaded = false;
        public Room RoomEntity = null;
        public long RoomRunLoadingOutTimeMs = -1;
        public long RoomRunStartTimeMs = -1;
        public double RoadDistance_km = 0;
        public double RoadDistance_m = 0;

        public void TeamLeaderRun()
        {
            if (RoomEntity != null && RoomEntity.State == RoomState.Start)
            {
                RoomRunLoadingOutTimeMs = TimeHelper.ClientNowMilliSeconds() + RoomRunLoadingMaxTimeMs;
                RoomEntity.SwitchState(RoomState.Run);
            }
        }

        public bool IsRun()
        {
            return RoomEntity.State == RoomState.Run && DataIsLoaded;
        }

        public bool IsEnd()
        {
            if (!IsRun())
                return false;
            for (int i = 0; i < RoomEntity.MapUnitList.Count; i++)
            {
                if (RoomEntity.MapUnitList[i].Info.DistanceTravelled > RoadDistance_m)
                {
                    return true;
                }
            }
            return false;
        }

        public void SetReady(long uid, bool isReady)
        {
            TeamMemberData teamMemberData = GetMember(uid);
            if (teamMemberData != null)
            {
                teamMemberData.IsReady = isReady;
            }
        }
        
        #region LoadProgress

        public void SetProgress(long uid, float progress)
        {
            TeamMemberData teamMemberData = GetMember(uid);
            if (teamMemberData != null)
            {
                teamMemberData.Progress = progress;
                if (teamMemberData.Progress > 0.999f)
                    teamMemberData.LoadingDone = true;
            }
        }

        public bool IsAllLoadingDone()
        {
            bool isAllDone = true;
            for (int i = 0; i < MEMBER_MAX; i++)
            {
                if (MemberDatas[i] == null)
                    continue;

                if (!MemberDatas[i].LoadingDone)
                {
                    isAllDone = false;
                    break;
                }
            }
            return isAllDone;
        }

        #endregion

        #region Member

        public TeamMemberData GetMember(long uid)
        {
            MemberDataUidDict.TryGetValue(uid, out var teamMemberData);
            return teamMemberData;
        }

        public bool AddMember(MapUnit mapUnit)
        {
            if (!MemberDataUidDict.ContainsKey(mapUnit.Uid))
            {
                int newMemberIndex = -1;

                if (Data.IsReservation)
                {
                    //預約有屬於每個玩家的固定位置
                    for (int i = 0; i < ReservationMembers.Count; i++)
                    {
                        if (ReservationMembers[i].Uid == mapUnit.Uid)
                        {
                            newMemberIndex = i;
                            break;
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < MEMBER_MAX; i++)
                    {
                        if (MemberDatas[i] == null)
                        {
                            newMemberIndex = i;
                            break;
                        }
                    }
                }
   
                if(newMemberIndex == -1)
                {
                    Log.Error($"AddMember Failed, 超出最大人數!, Uid:{mapUnit.Uid}, RoomId:{RoomEntity.Id}");
                    return false;
                }

                TeamMemberData memberData = new TeamMemberData()
                {
                    MemberIndex = newMemberIndex,
                    Uid = mapUnit.Uid,
                    Name = mapUnit.Info.Name,
                    Location = mapUnit.Info.Location,
                    CharSetting = mapUnit.Info.CharSetting,
                };

                MemberDataUidDict.Add(mapUnit.Uid, memberData);
                MemberDatas[newMemberIndex] = memberData;
                return true;
            }
            return false;
        }

        public bool RemoveMember(long uid)
        {
            if (MemberDataUidDict.TryGetValue(uid, out var memberData))
            {
                MemberDataUidDict.Remove(uid);
                MemberDatas[memberData.MemberIndex] = null;

                //(非預約房間)自動遞補隊長
                if (!Data.IsReservation)
                {
                    if (Data.LeaderUid == uid)
                    {
                        for (int i = 0; i < MEMBER_MAX; i++)
                        {
                            if (MemberDatas[i] != null)
                            {
                                Data.LeaderUid = MemberDatas[i].Uid;
                                Data.LeaderName = MemberDatas[i].Name;
                            }
                        }
                    }
                }
                return true;
            }
            return false;
        }

        #endregion

        #region Reservation

        public RepeatedField<ReservationMemberData> ReservationMembers { get; private set; } = null;

        public void SetReservationMember(RepeatedField<ReservationMemberData> members)
        {
            ReservationMembers = members;
        }

        public bool IsReservationMember(long uid)
        {
            for (int i = 0; i < ReservationMembers.Count; i++)
            {
                if (ReservationMembers[i].Uid == uid)
                    return true;
            }
            return false;
        }

        #endregion
    }
}