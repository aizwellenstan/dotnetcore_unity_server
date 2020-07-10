using ETModel;
using System;

namespace ETHotfix
{
    [Event(EventIdType.SyncAllRoamingRoom)]
    public class Event_SyncAllRoamingRoom : AEvent
    {
        public async override void Run()
        {
            RoomComponent roomComponent = Game.Scene.GetComponent<RoomComponent>();
            if(roomComponent == null)
            {
                //TODO:用command時防呆，這邊先return，應該有更好的做法
                return;
            }

            ConfigComponent configComponent = Game.Scene.GetComponent<ConfigComponent>();
            IConfig[] roadSettings = configComponent.GetAll(typeof(RoadSetting));

            StartConfigComponent startConfigComponent = Game.Scene.GetComponent<StartConfigComponent>();
            StartConfig startConfig = startConfigComponent.StartConfig;

            for (int i = 0; i < roadSettings.Length; i++)
            {
                RoadSetting roadSetting = roadSettings[i] as RoadSetting;
                if (roadSetting == null)
                    continue;
                if (roadSetting.Id < RoadHelper.RoamingIdStart || roadSetting.Id > RoadHelper.RoamingIdEnd)
                    continue;
                int mapIndex = startConfigComponent.MapConfigs.IndexOf(startConfig);
                if(startConfig.AppType != AppType.AllServer)
                {
                    if (roadSetting.MapServerIndex != mapIndex)
                        continue;
                }
                var mapLimitSetting = (MapLimitSetting)configComponent.Get(typeof(MapLimitSetting), roadSetting.MapServerIndex);
                RoomInfo roamingRoomInfo = new RoomInfo()
                {
                    Title = string.Empty,
                    RoadSettingId = roadSetting.Id,
                    MaxMemberCount = mapLimitSetting == null ? 1000 : mapLimitSetting.MaxUserCount,
                };

                var room = roomComponent.GetRoamingBySettingId(roadSetting.Id);

                if (room == null) 
                {
                    roamingRoomInfo.NowMemberCount = 0;
                    room = await roomComponent.CreateRoamingRoom(roamingRoomInfo);
                }
                else
                {
                    room.SetData(roamingRoomInfo);
                }
                  
                roomComponent.Update(room).Coroutine();

                //RoomInfo teamRoomInfo = new RoomInfo()
                //{
                //    Title = $"測試組隊_{RandomHelper.RandomNumber(1515, 54861655)}",
                //    RoadSettingId = roadSetting.Id,
                //    MaxMemberCount = 8,
                //    NowMemberCount = 0,
                //};

                //TeamRoomData teamRoomData = new TeamRoomData()
                //{
                //    LeaderName = $"測試玩家_{i}",
                //    LeaderUid = 989898989,
                //    StartUTCTimeTick = DateTime.UtcNow.Ticks,
                //};

                //roomComponent.CreateTeamRoom(teamRoomInfo, teamRoomData);
            }
        }
    }
}
