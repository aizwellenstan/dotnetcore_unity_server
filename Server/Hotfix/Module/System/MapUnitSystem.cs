using ETModel;
using System;

namespace ETHotfix
{
    [ObjectSystem]
    public class MapUnitAwakeSystem : AwakeSystem<MapUnit, MapUnitType>
    {
        public override void Awake(MapUnit self, MapUnitType a)
        {
            self.Awake(a);
        }
    }

    [ObjectSystem]
    public class MapUnitDestroySystem : DestroySystem<MapUnit>
    {
        public override void Destroy(MapUnit self)
        {
            self.LeaveRoom();
        }
    }

    //[ObjectSystem]
    //public class MapUnitUpdateSystem : UpdateSystem<MapUnit>
    //{
    //    //millisecond
    //    private const long _sendTimeInterval = 200;
    //    private long _sendTimeAfter = 0;

    //    public override void Update(MapUnit self)
    //    {
    //        long nowTime = TimeHelper.Now();
    //        if (self.ActorMessages.Count > 0 && nowTime > _sendTimeAfter)
    //        {
    //            IActorMessage message = self.ActorMessages.Dequeue();
    //            MapMessageHelper.Broadcast(self, message);
    //            _sendTimeAfter = nowTime + _sendTimeInterval;
    //        }
    //    }
    //}

    public static class MapUnitSystem
    {
        #region Room

        public static async ETTask<bool> EnterRoom(this MapUnit self, long roomId)
        {
            Room room = Game.Scene.GetComponent<RoomComponent>().Get(roomId);
            if (room == null)
                return false;
            self.Room = room;
            self.RoomId = room.Id;
            await room.AddMapUnit(self);
            self.RoomBlockComponent = room.GetComponent<RoomBlockComponent>();
            if (self.MapUnitType != MapUnitType.Npc)
                self.LookMapUnitBlockChannel(self);
            return true;
        }

        public static void LeaveRoom(this MapUnit self)
        {
            if (self.Room != null)
            {
                self.Room.RemoveMapUnitByUid(self.Uid).Coroutine();
            }
            self.Room = null;
            self.RoomId = -1;
            self.RoomBlockComponent = null;
            self.Block = null;
            self.SendChannel = null;
            self.UnlookMapUnitBlockChannel();
            self.UnlookAllSource();
        }

        #endregion
    }
}