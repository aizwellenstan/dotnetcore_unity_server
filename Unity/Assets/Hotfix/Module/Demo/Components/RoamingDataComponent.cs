using System.Collections.Generic;
using System.Linq;
using ETModel;

namespace ETHotfix
{
    public sealed class RoamingSettingData : Object
    {
        public long RoomId { get; set; }
        public long SourceId;
        public string Title;
        public string Level;
        public double Distance;
        public string PreviewName;
        public int MemberCount;
        public int MaxUserCount;
        public int NowUserCount;
    }

    [ObjectSystem]
    public class RoamingDataComponentAwakeSystem : AwakeSystem<RoamingDataComponent>
    {
        public override void Awake(RoamingDataComponent self)
        {
            self.Awake();
        }
    }

    public class RoamingDataComponent : Component
    {
        public List<RoamingSettingData> RoamingSettingDatas = new List<RoamingSettingData>();

        public void Awake()
        {
        
        }
        
        public override void Dispose()
        {
            if (this.IsDisposed)
            {
                return;
            }
            base.Dispose();
            RoamingSettingDatas.Clear();
        }

        public bool IsUserFull(long roomId)
        {
            var result = RoamingSettingDatas.Find(e => e.RoomId == roomId);
            if (result == null)
                return false;
            return result.NowUserCount >= result.MaxUserCount;
        }
    }
}