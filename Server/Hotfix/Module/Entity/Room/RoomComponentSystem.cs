using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;
using ETModel;

namespace ETHotfix
{
    [ObjectSystem]
    public class RoomComponentStartSystem : StartSystem<RoomComponent>
    {
        public override void Start(RoomComponent self)
        {
            self.Start();
        }
    }

    public static class RoomComponentSystem
    {
        public static async ETTask<Room> CreateRoamingRoom(this RoomComponent self, RoomInfo roomInfo)
        {
            Room room = ComponentFactory.CreateWithId<Room, RoomType>(IdGenerater.GenerateId(), RoomType.Roaming);
            room.SetData(roomInfo);
            await self.MemorySync.Create(room);
            room.AddComponent<RoomRoamingComponent>();
            room.AddComponent<RoomBlockComponent, Room>(room);
            room.AddComponent<RoomNpcComponent>();
            var first = OtherHelper.Search(self.RoamingList, r => r.Id == room.Id);
            if (first == null)
            {
                self.RoamingList.Add(room);
                self.RoamingSettingDict.Add(roomInfo.RoadSettingId, room);
            }
            return room;
        }

        public static async ETTask<Room> CreateTeamRoom(this RoomComponent self, RoomInfo roomInfo, TeamRoomData teamRoomData)
        {
            Room room = ComponentFactory.CreateWithId<Room, RoomType>(IdGenerater.GenerateId(), RoomType.Team);
            room.SetData(roomInfo);
            room = await self.MemorySync.Create(room);
            room.AddComponent<RoomTeamComponent, TeamRoomData>(teamRoomData);
            room.AddComponent<RoomBlockComponent, Room>(room);
            var first = OtherHelper.Search(self.TeamList, r => r.Id == room.Id);
            if (first == null)
                self.TeamList.Add(room);
            return room;
        }

        public static async ETTask DestroyRoom(this RoomComponent self, long id)
        {
            Room room = self.Get(id);
            if (room == null)
                return;

            switch (room.Type)
            {
                case RoomType.Roaming:
                    {
                        self.RoamingSettingDict.Remove(room.info.RoadSettingId);
                        self.RoamingList.Remove(room);
                        await self.MemorySync.Delete<Room>(id);
                    }
                    break;
                case RoomType.Team:
                    {
                        self.TeamList.Remove(room);
                        await self.MemorySync.Delete<Room>(id);
                    }
                    break;
            }
        }

        public static async ETTask<Room> Update(this RoomComponent self, Room room)
        {
            return await self.MemorySync.Update(room);
        }

        //public static async ETTask<bool> EnterRoom(this RoomComponent self, long roomId)
        //{

        //}
    }
}