using ETModel;

namespace ETHotfix
{
    public class RoadHelper
    {
        public const int RoamingIdStart = 1;
        public const int RoamingIdEnd = 999;

        public const int TeamIdStart = 1000;
        public const int TeamIdEnd = 9999;

        public static double GetDistance_km(RoadSetting roadSetting)
        {
            return roadSetting.Distance * 0.5f;
        }

        public static double GetDistance_m(RoadSetting roadSetting)
        {
            return GetDistance_km(roadSetting) * 1000;
        }
    }
}
