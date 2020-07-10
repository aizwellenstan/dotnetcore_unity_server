using ETModel;

namespace ETHotfix
{
    [ObjectSystem]
    public class RoomBlockComponentAwakeSystem : AwakeSystem<RoomBlockComponent, Room>
    {
        public override void Awake(RoomBlockComponent self, Room room)
        {
            double roadDistance_m = 0;
            var roadSetting = Game.Scene.GetComponent<ConfigComponent>().Get(typeof(RoadSetting), room.info.RoadSettingId) as RoadSetting;
            if (roadSetting != null)
            {
                var timerComponent = Game.Scene.GetComponent<TimerComponent>();
                roadDistance_m = RoadHelper.GetDistance_m(roadSetting);
            }
            else
            {
                Log.Error($"RoomBlockComponent Awake Failed, roadSetting = null, Room{room.Id}");
            }
            self.InitAllBlock(roadDistance_m);
        }
    }

    [ObjectSystem]
	public class RoomBlockComponentStartSystem : StartSystem<RoomBlockComponent>
	{
        public override void Start(RoomBlockComponent self)
		{
            self.StartAllBlock();
        }
	}

    [ObjectSystem]
    public class RoomBlockComponentDestroySystem : DestroySystem<RoomBlockComponent>
    {
        public override void Destroy(RoomBlockComponent self)
        {
            self.ReleaseAllBlock();
        }
    }
}