using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;
using ETModel;

namespace ETHotfix
{
    [ObjectSystem]
    public class RoomEntityAwakeSystem : AwakeSystem<Room, RoomType>
    {
        public override void Awake(Room self, RoomType a)
        {
            self.Awake(a);
        }
    }

    public static class RoomSystem
    {
        public static async ETTask AddMapUnit(this Room self, MapUnit mapUnit)
        {
            if (self.UidDict.ContainsKey(mapUnit.Uid))
                return;
            self.UidDict.Add(mapUnit.Uid, mapUnit);
            self.IdDict.Add(mapUnit.Id, mapUnit);
            self.MapUnitList.Add(mapUnit);
            self.info.NowMemberCount = self.MapUnitList.Count;

            // block
            var blockComponent = self.GetComponent<RoomBlockComponent>();
            blockComponent.Enter(mapUnit, 
                mapUnit.Info.DistanceTravelled);

            switch (self.Type)
            {
                case RoomType.Roaming:
                    // TODO AddMapUnit Roaming
                    break;
                case RoomType.Team:
                    {
                        var teamComponent = self.GetComponent<RoomTeamComponent>();
                        teamComponent.AddMember(mapUnit);
                    }
                    break;
            }

            // 同步房間資訊
            RoomComponent roomComponent = Game.Scene.GetComponent<RoomComponent>();
            await roomComponent.Update(self);

            // Create GlobalInfo
            self._m2C_MapUnitGlobalCreate.GlobalInfo = mapUnit.GlobalInfo;
            MapMessageHelper.BroadcastTarget(self._m2C_MapUnitGlobalCreate, self.MapUnitList);
        }

        public static async ETTask RemoveMapUnitByUid(this Room self, long uid)
        {
            RoomComponent roomComponent = Game.Scene.GetComponent<RoomComponent>();

            if (!self.UidDict.TryGetValue(uid, out MapUnit mapUnit))
            {
                return;
            }
            self.MapUnitList.Remove(mapUnit);
            self.IdDict.Remove(mapUnit.Id);
            self.UidDict.Remove(uid);

            self.info.NowMemberCount = self.MapUnitList.Count;

            // block
            var blockComponent = self.GetComponent<RoomBlockComponent>();
            blockComponent.Leave(mapUnit);

            switch (self.Type)
            {
                case RoomType.Roaming:
                    // TODO RemoveMapUnit Roaming
                    break;
                case RoomType.Team:
                    {
                        var teamComponent = self.GetComponent<RoomTeamComponent>();
                        if (teamComponent != null)
                        {
                            teamComponent.RemoveMember(uid);

                            // (非預約房間)沒有人就自動解散
                            if (!teamComponent.Data.IsReservation && self.MemberCount <= 0)
                            {
                                await roomComponent.DestroyRoom(self.Id);
                            }
                        }
                    }
                    break;
            }

            // 同步房間資訊
            await roomComponent.Update(self);

            // Destroy GlobalInfo
            self._m2C_MapUnitGlobalDestroy.MapUnitId = mapUnit.Id;
            MapMessageHelper.BroadcastTarget(self._m2C_MapUnitGlobalDestroy, self.MapUnitList);
        }
    }
}