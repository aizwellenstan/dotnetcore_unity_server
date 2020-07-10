using ETHotfix;

namespace ETModel
{
    public class MapUnitFsmMoveComponent : Component
    {
        public MapUnit MapUnit { get; set; } = null;
        //public M2C_MapUnitUpdate M2C_UpdateMapUnit { get; set; } = new M2C_MapUnitUpdate();
        public bool Enable { get; set; } = false;


        //millisecond
        public const int SpeedRandomTimeIntervalMin = 800;
        public const int SpeedRandomTimeIntervalMax = 1100;
        public long SpeedRandomTimeAfter = 0;

        //millisecond
        public const int SpeedLerpTimeInterval = 100;
        public long SpeedLerpTimeAfter = 0;
        public long SpeedLerpTimePrevious = 0;

        //m/s
        public float SpeedMin = 25;
        public float SpeedMax = 32;
        public float SpeedNow = 0;
        public float SpeedTarget = 10;

        //millisecond
        public const long MoveTimeInterval = 500;
        public long MoveTimeAfter = 0;
        public long MoveTimePrevious = 0;

        //millisecond
        public const long AsyncTimeInterval = 500;
        public long AsyncTimeAfter = 0;

        //distanceTravelledEnd
        public const double DistanceTravelledEndMin = 30000;
        public const double DistanceTravelledEndMax = 99000;
        public double DistanceTravelledEnd = 0;

        public void CheckDistanceTravelledEnd()
        {
            if(MapUnit?.Info?.DistanceTravelled > DistanceTravelledEnd)
            {
                RefreshDistanceTravelledEnd();
                MapUnit.TrySetStartTime(true);
                MapUnit.Info.DistanceTravelled = 0;
            }
        }

        public void RefreshDistanceTravelledEnd()
        {
            DistanceTravelledEnd = RandomHelper.RandomDouble() * (DistanceTravelledEndMax - DistanceTravelledEndMin) + DistanceTravelledEndMin;
        }

        public void Reset()
        {
            MapUnit = null;
            //M2C_UpdateMapUnit = new M2C_MapUnitUpdate();
            Enable = false;

            //millisecond
            SpeedRandomTimeAfter = 0;

            //millisecond
            SpeedLerpTimePrevious = 0;

            //m/s
            SpeedNow = 0;
            SpeedTarget = 10;

            //millisecond
            MoveTimeAfter = 0;
            MoveTimePrevious = 0;

            //millisecond
            AsyncTimeAfter = 0;

            //distanceTravelledEnd
            RefreshDistanceTravelledEnd();
        }
    }
}