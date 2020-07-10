using ETHotfix;
using MongoDB.Bson;
using System.Collections.Generic;
using System.Linq;

namespace ETModel
{

	public class BlockChannel
    {
        public Block Block { get; private set; }
        public List<MapUnit> PositionMambers { get; private set; } = new List<MapUnit>();
        public List<MapUnit> ReceviceMambers { get; private set; } = new List<MapUnit>();
        private const int _blockChannelMaxMember = 8;

        public BlockChannel(Block block)
        {
            Block = block;
            PositionMambers.Clear();
            ReceviceMambers.Clear();
        }

        public void Start()
        {

        }

        public void Release()
        {
            Block = null;
            PositionMambers.Clear();
            ReceviceMambers.Clear();
        }

        #region MapUnit

        private M2C_MapUnitCreate m2C_MapUnitCreate = new M2C_MapUnitCreate();
        private M2C_MapUnitDestroy m2C_MapUnitDestroy = new M2C_MapUnitDestroy();

        /// <summary>
        /// 判斷是否在同一個房間內?
        /// </summary>
        /// <param name="mapUnit"></param>
        /// <returns></returns>
        private bool IsOnSameRoom(MapUnit mapUnit)
        {
            return Block.roomId != 0 && Block.roomId == mapUnit.RoomId;
        }

        public void Enter(MapUnit mapUnit, double position)
        {
            if (!IsOnSameRoom(mapUnit))
                return;

            mapUnit.SendChannel = this;

            var obj = OtherHelper.Search(PositionMambers, e => e.Uid == mapUnit.Uid);
            if (obj == null)
            {
                PositionMambers.Add(mapUnit);
            }
            m2C_MapUnitCreate.MapUnitInfo = mapUnit.Info;
            SendMessage(m2C_MapUnitCreate);
            var lookAtSources = mapUnit.GetLookAtSources();
            for (int i = 0; i < lookAtSources.Count; i++)
            {
                Look(lookAtSources[i]);
            }
        }

        public void Leave(MapUnit mapUnit, bool isLeaveRoom)
        {
            if (!IsOnSameRoom(mapUnit))
                return;

            mapUnit.SendChannel= null;

            var obj = OtherHelper.Search(PositionMambers, e => e.Uid == mapUnit.Uid);
            if (obj != null)
            {
                PositionMambers.Remove(mapUnit);
            }

            if (isLeaveRoom)
            {
                //如果MapUnit是離開Room，要告知MapUnitDestroy
                m2C_MapUnitDestroy.MapUnitId = mapUnit.Id;
                SendMessage(m2C_MapUnitDestroy);
            }
            else
            {
                //如果MapUnit是離開Block，不就要告知觀看者MapUnitDestroy
                for (int i = 0; i < mapUnit.LookAtSources.Count; i++)
                {
                    Unlook(mapUnit.LookAtSources[i]);
                }
                m2C_MapUnitDestroy.MapUnitId = mapUnit.Id;
                SendMessage(m2C_MapUnitDestroy);
            }
        }

        public void SendMessage(IActorMessage message)
        {
            // EthanDebug
            //if(message is M2C_MapUnitCreate m2C_MapUnitCreate)
            //{
            //    for(int i = 0; i < ReceviceMambers.Count; i++)
            //    {
            //        m2C_MapUnitCreate.MapUnitInfo.DebugId = IdGenerater.GenerateId();
            //        var rev = ReceviceMambers[i];

            //        BsonDocument doc = new BsonDocument
            //        {
            //            { "Block.room.Id" , Block?.room?.Id },
            //            { "Block.roomId" , Block?.roomId },
            //            { "Block.blockId" , Block?.blockId },
            //            { "rev.Id" , rev?.Id },
            //            { "rev.MapAppId" , IdGenerater.GetAppId(rev == null ? 0 : rev.Id) },
            //            { "rev.Uid" , rev?.Uid },
            //            { "rev.RoomId" , rev?.RoomId },
            //            { "rev.Room.Id" , rev?.Room?.Id },
            //            { "rev.Block.room.Id" , rev?.Block?.room?.Id },
            //            { "rev.Block.roomId" , rev?.Block?.roomId },
            //            { "rev.Block.blockId" , rev?.Block?.blockId },
            //            { "sendToList", ReceviceMambers.Select(e => $"MapUnitId:{e.Id}, Uid:{e.Uid}, BlockId:{e.Block?.blockId}").ToJson() }
            //        };
            //        Log.Trace($"DebugId[{m2C_MapUnitCreate.MapUnitInfo.DebugId}]> {doc.ToJson()}");
            //    }
            //}
            MapMessageHelper.BroadcastTarget(message, ReceviceMambers);
        }

        public void Look(MapUnit mapUnit)
        {
            if (!IsOnSameRoom(mapUnit))
                return;

            var obj = OtherHelper.Search(ReceviceMambers, e => e.Uid == mapUnit.Uid);
            if (obj == null)
            {
                ReceviceMambers.Add(mapUnit);
            }
        }

        public void Unlook(MapUnit mapUnit)
        {
            var obj = OtherHelper.Search(ReceviceMambers, e => e.Uid == mapUnit.Uid);
            if (obj != null)
            {
                ReceviceMambers.Remove(mapUnit);
            }
        }

        public bool IsFullMember()
        {
            return PositionMambers.Count >= _blockChannelMaxMember;
        }

        public bool IsInside(double position)
        {
            return Block.IsInside(position);
        }

        #endregion
    }
}