using ETHotfix;

namespace ETModel
{
    [ObjectSystem]
	public class RoomRoamingComponentAwakeSystem : AwakeSystem<RoomRoamingComponent>
	{
		public override void Awake(RoomRoamingComponent self)
		{
			self.Awake();
		}
	}

	public class RoomRoamingComponent : Component/*, ISerializeToEntity*/
	{
        public RoamingRoomData Data { get; private set; }

        public void Awake()
        {
            Data = new RoamingRoomData();

            if (Entity is Room room)
                room.SwitchState(RoomState.Run);
        }
    }
}