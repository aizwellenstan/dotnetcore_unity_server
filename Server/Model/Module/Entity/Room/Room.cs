using ETHotfix;
using Google.Protobuf.Collections;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;
using System;
using System.Collections.Generic;

namespace ETModel
{
    public enum RoomState
    {
        Ready,
        Start,
        Run,
        End,
    }

    public enum RoomType
    {
        Roaming,
        Team
    }

    public sealed partial class Room : Entity
	{
        [BsonIgnore]
        [CacheIgnore]
        public RoomState State 
        {
            get
            {
                return (RoomState)state;
            }
            set
            {
                state = (int)value;
            }
        }

        [BsonIgnore]
        [CacheIgnore]
        public RoomType Type 
        {
            get 
            {
                return (RoomType)type;
            }
            set
            {
                type = (int)value;
            }
        }

        [BsonIgnore]
        [CacheIgnore]
        public int MemberCount
        {
            get
            {
                return this.MapUnitList.Count;
            }
        }

        [BsonIgnore]
        public readonly Dictionary<long, MapUnit> UidDict = new Dictionary<long, MapUnit>();

        [BsonIgnore]
        public readonly Dictionary<long, MapUnit> IdDict = new Dictionary<long, MapUnit>();

        [BsonIgnore]
        public readonly List<MapUnit> MapUnitList = new List<MapUnit>();

        [BsonIgnore]
        public M2C_MapUnitGlobalCreate _m2C_MapUnitGlobalCreate = new M2C_MapUnitGlobalCreate();

        [BsonIgnore]
        public M2C_MapUnitGlobalDestroy _m2C_MapUnitGlobalDestroy = new M2C_MapUnitGlobalDestroy();

        public void Awake(RoomType roomType)
		{
            Type = roomType;
            State = RoomState.Start;
            info = null;
        }

        public override void Dispose()
        {
            if (this.IsDisposed)
            {
                return;
            }

            base.Dispose();
        }

        public void SwitchState(RoomState state)
        {
            State = state;
        }

        /// <summary>
        /// MapUintId
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public MapUnit GetMapUnitById(long id)
        {
            this.IdDict.TryGetValue(id, out MapUnit mapUnit);
            return mapUnit;
        }

        /// <summary>
        /// PlayerUid
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        public MapUnit GetMapUnitByUid(long uid)
        {
            this.UidDict.TryGetValue(uid, out MapUnit mapUnit);
            return mapUnit;
        }

        public List<MapUnit> GetAll()
        {
            return this.MapUnitList;
        }

        public void SetData(RoomInfo roomInfo)
        {
            info = roomInfo;
            info.RoomId = Id;
        }
    }
}