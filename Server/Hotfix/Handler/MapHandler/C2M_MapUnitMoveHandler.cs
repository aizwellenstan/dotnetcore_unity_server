using ETModel;
using UnityEngine;

namespace ETHotfix
{
	[ActorMessageHandler(AppType.Map)]
	public class C2M_MapUnitMoveHandler : AMActorLocationHandler<MapUnit, C2M_MapUnitMove>
    {
        protected override void Run(MapUnit mapUnit, C2M_MapUnitMove message)
        {
            if (mapUnit.Room.State != RoomState.Run)
            {
                return;
            }

            mapUnit.TrySetStartTime();
            mapUnit.SetDistanceTravelled(message.DistanceTravelledTarget);
            mapUnit.SetSpeedMS(message.SpeedMS);
            mapUnit.SetCalories(message.Calories);
            mapUnit.SetPower(message.Power);

            //M2C_MapUnitUpdate m2C_MapUnitUpdate = new M2C_MapUnitUpdate();
            //m2C_MapUnitUpdate.MapUnitId = mapUnit.Id;
            //m2C_MapUnitUpdate.DistanceTravelledTarget = mapUnit.Info.DistanceTravelled;
            //m2C_MapUnitUpdate.SpeedMS = mapUnit.Info.SpeedMS;
            //m2C_MapUnitUpdate.DistanceTravelledUpdateUTCTick = System.DateTime.UtcNow.Ticks;

            //MapMessageHelper.BroadcastRoom(mapUnit.RoomId, m2C_MapUnitUpdate);
        }
    }
}