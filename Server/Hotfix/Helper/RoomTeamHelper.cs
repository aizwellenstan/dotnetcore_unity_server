using ETModel;
using System;
using System.Collections.Generic;

namespace ETHotfix
{
    public class RoomTeamHelper
    {
        public static async void KickMember(RoomTeamComponent roomTeamComponent, MapUnit kickMember, TeamLoseType loseType)
        {
            if (kickMember == null)
                return;

            //廣播給指定玩家失去房間
            M2C_TeamLose m2c_TeamLose = new M2C_TeamLose();
            m2c_TeamLose.LoseType = loseType;
            List<MapUnit> broadcastMapUnits = new List<MapUnit>();
            broadcastMapUnits.Add(kickMember);
            MapMessageHelper.BroadcastTarget(m2c_TeamLose, broadcastMapUnits);

            //Player移除mapUnitId
            var proxy = Game.Scene.GetComponent<CacheProxyComponent>();
            var playerSync = proxy.GetMemorySyncSolver<Player>();
            var player = playerSync.Get<Player>(kickMember.Uid);
            player?.LeaveRoom();
            await playerSync.Update(player);

            //中斷指定玩家與Map的連接
            var mapUnitGateComponent = kickMember?.GetComponent<MapUnitGateComponent>();
            if (mapUnitGateComponent != null)
            {
                mapUnitGateComponent.IsDisconnect = true;
            }
            Game.Scene.GetComponent<MapUnitComponent>().Remove(kickMember.Id);

            if (m2c_TeamLose.LoseType != TeamLoseType.LoadingTimeOut)
            {
                //廣播給離開玩家外的玩家 成員異動
                M2C_TeamModifyMember m2c_TeamModifyMember = new M2C_TeamModifyMember();
                m2c_TeamModifyMember.Uid = kickMember.Uid;
                MapMessageHelper.BroadcastRoom(roomTeamComponent.RoomEntity.Id, m2c_TeamModifyMember, kickMember.Uid);
            }
        }

        public static async void LoadingFinish(RoomTeamComponent roomTeamComponent)
        {
            if (roomTeamComponent.DataIsLoaded)
                return;

            roomTeamComponent.DataIsLoaded = true;

            //等3秒給Client Lerp
            await Game.Scene.GetComponent<TimerComponent>().WaitForSecondAsync(3.0f);

            //設定比賽開始結束時間
            var roadSetting = Game.Scene.GetComponent<ConfigComponent>().Get
                     (typeof(RoadSetting), roomTeamComponent.RoomEntity.info.RoadSettingId) as RoadSetting;

            if (roadSetting != null)
            {
                var timerComponent = Game.Scene.GetComponent<TimerComponent>();
                roomTeamComponent.RoomRunStartTimeMs = TimeHelper.ClientNowMilliSeconds();
                roomTeamComponent.RoadDistance_km = RoadHelper.GetDistance_km(roadSetting);
                roomTeamComponent.RoadDistance_m = RoadHelper.GetDistance_m(roadSetting);
            }
            else
            {
                Log.Error($"找不到該路徑資料, RoadSettingId:{roomTeamComponent.RoomEntity.info.RoadSettingId}");
            }

            //設定所有Player StartRoom
            List<MapUnit> mapUnits = roomTeamComponent.RoomEntity.GetAll();
            var proxy = Game.Scene.GetComponent<CacheProxyComponent>();
            var playerSync = proxy.GetMemorySyncSolver<Player>();
            for (int i = 0; i < mapUnits.Count; i++)
            {
                Player target = playerSync.Get<Player>(mapUnits[i].Uid);
                if (target != null)
                {
                    target.StartRoom();
                    await playerSync.Update(target);
                }
            }

            //廣播給所有玩家 所有人載入完成
            M2C_TeamGoBattleProgressAllDone m2c_TeamGoBattleProgressAllDone = new M2C_TeamGoBattleProgressAllDone()
            {
                StartUTCTick = DateTime.UtcNow.Ticks
            };
            MapMessageHelper.BroadcastRoom(roomTeamComponent.RoomEntity.Id, m2c_TeamGoBattleProgressAllDone);
        }
    }
}
