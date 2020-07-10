using System.Collections.Generic;
using System.Linq;
using ETHotfix;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;

namespace ETModel
{
    public class RoomComponent : Component
	{
        /// <summary>
        /// 房間同步控制器(擁有者)
        /// </summary>
        public RedisEventSolverComponent MemorySync { get; set; }

  //      [BsonElement]
		//[BsonDictionaryOptions(DictionaryRepresentation.ArrayOfArrays)]
		//private readonly Dictionary<long, Room> idRooms = new Dictionary<long, Room>();

        // Roaming
        [BsonElement]
        [BsonDictionaryOptions(DictionaryRepresentation.ArrayOfArrays)]
        public readonly Dictionary<long, Room> RoamingSettingDict = new Dictionary<long, Room>();

        [BsonElement]
        public readonly List<Room> RoamingList = new List<Room>();

        // Team
        [BsonElement]
        public readonly List<Room> TeamList = new List<Room>();

        public void Start()
        {
            var proxy = Game.Scene.GetComponent<CacheProxyComponent>();
            MemorySync = proxy.GetMemorySyncSolver<Room>();
            MemorySync.onCreate += OnCreate;
            MemorySync.onWillDelete += OnWillDelete;

            // 因為內部會取RoomComponent但Awake時還沒產生好，所以寫在Start
            Game.EventSystem.Run(EventIdType.SyncAllRoamingRoom);
        }

        public override void Dispose()
		{
			if (this.IsDisposed)
			{
				return;
			}
			base.Dispose();

			//foreach (Room unit in this.idRooms.Values)
			//{
			//	unit.Dispose();
			//}

            MemorySync.onCreate -= OnCreate;
            MemorySync.onWillDelete -= OnWillDelete;
            // 非擁有者請勿操作Dispose
            MemorySync.Dispose();

            //this.idRooms.Clear();
		}

        public void OnCreate(long id)
        {
            //if (MemorySync.IsMine(id))
            //    return;
            Room room = MemorySync.Get<Room>(id);
            Room first = null;
            // 房間創建同步完成
            if (room != null)
            {
                // 初始化同步用控制器
                switch (room.Type)
                {
                    case RoomType.Roaming:
                        first = OtherHelper.Search(RoamingList, r => r.Id == id);
                        if(first == null)
                        {
                            RoamingList.Add(room);
                            RoamingSettingDict.Add(room.info.RoadSettingId, room);
                        }
                        break;
                    case RoomType.Team:
                        first = OtherHelper.Search(TeamList, r => r.Id == id);
                        if (first == null)
                        {
                            TeamList.Add(room);
                        }
                        break;
                }
            }
            else
            {
                // GG跑到這邊表示有問題
                Log.Error($"Room[{id}] is missing!");
            }
        }

        public void OnWillDelete(long id)
        {
            //if (MemorySync.IsMine(id))
            //    return;
            Room room = MemorySync.Get<Room>(id);
            // 房間刪除同步完成
            if (room == null)
            {
                // GG跑到這邊表示有問題
                Log.Error($"Room[{id}] has removed!");
            }
            else
            {
                switch (room.Type)
                {
                    case RoomType.Roaming:
                        RoamingList.Remove(room);
                        RoamingSettingDict.Remove(room.info.RoadSettingId);
                        break;
                    case RoomType.Team:
                        TeamList.Remove(room);
                        break;
                }
            }
        }

        //      public Room Get(long id)
        //{
        //	this.idRooms.TryGetValue(id, out Room room);
        //	return room;
        //}

        public Room Get(long id)
        {
            return MemorySync.Get<Room>(id);
        }

  //      public void RemoveNoDispose(long id)
		//{
		//	this.idRooms.Remove(id);
		//}

		//public int Count
		//{
		//	get
		//	{
		//		return this.idRooms.Count;
		//	}
		//}

        //public int CountByType(RoomType roomType)
        //{
        //    switch (roomType)
        //    {
        //        case RoomType.Roaming:
        //            return RoamingList.Count;
        //        case RoomType.Team:
        //            return TeamList.Count;
        //    }
        //    return 0;
        //}

  //      public Room[] GetAll()
		//{
		//	return this.idRooms.Values.ToArray();
		//}

        public List<Room> GetAllByType(RoomType roomType)
        {
            switch (roomType)
            {
                case RoomType.Roaming:
                    return RoamingList;
                case RoomType.Team:
                    return TeamList;
            }
            return null;
        }

        public Room GetRoamingBySettingId(long settingId)
        {
            RoamingSettingDict.TryGetValue(settingId, out var room);
            return room;
        }

        //public Room CreateRoamingRoom(RoomInfo roomInfo)
        //{
        //    Room room = ComponentFactory.CreateWithId<Room, RoomType>(IdGenerater.GenerateId(), RoomType.Roaming);
        //    room.SetData(roomInfo);
        //    room.AddComponent<RoomRoamingComponent>();
        //    room.AddComponent<RoomBlockComponent, Room>(room);
        //    room.AddComponent<RoomNpcComponent>();
        //    idRooms.Add(room.Id, room);
        //    if (!RoamingList.Contains(room))
        //    {
        //        RoamingList.Add(room);
        //        RoamingSettingDict.Add(roomInfo.RoadSettingId, room);
        //    }
        //    return room;
        //}

        //public Room CreateTeamRoom(RoomInfo roomInfo, TeamRoomData teamRoomData)
        //{
        //    Room room = ComponentFactory.CreateWithId<Room, RoomType>(IdGenerater.GenerateId(), RoomType.Team);
        //    room.SetData(roomInfo);
        //    room.AddComponent<RoomTeamComponent, TeamRoomData>(teamRoomData);
        //    room.AddComponent<RoomBlockComponent, Room>(room);
        //    idRooms.Add(room.Id, room);
        //    if (!TeamList.Contains(room))
        //        TeamList.Add(room);
        //    return room;
        //}

        //public void DestroyRoom(long id)
        //{
        //    Room room = Get(id);
        //    if (room == null)
        //        return;

        //    switch (room.Type)
        //    {
        //        case RoomType.Roaming:
        //            {
        //                RoamingSettingDict.Remove(room.Info.RoadSettingId);
        //                RoamingList.Remove(room);
        //                idRooms.Remove(id);
        //                room.Dispose();
        //            }
        //            break;
        //        case RoomType.Team:
        //            {
        //                TeamList.Remove(room);
        //                idRooms.Remove(id);
        //                room.Dispose();
        //            }
        //            break;
        //    }
        //}
    }
}