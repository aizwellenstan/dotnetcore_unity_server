using ETHotfix;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ETModel
{
	public class RoomBlockComponent : Component/*, ISerializeToEntity*/
	{
        private double _roadDistance_m = 0;
        private const double _blockMaxDistance = 1000.0;
        private List<Block> _blocks = new List<Block>();

        public void InitAllBlock(double RoadDistance_m)
        {
            _blocks.Clear();
            _roadDistance_m = RoadDistance_m;

            var room = GetParent<Room>();
            switch (room.Type)
            {
                case RoomType.Roaming:
                    int blockCount = (int)Math.Ceiling(_roadDistance_m / _blockMaxDistance);
                    for (int i = 0; i < blockCount; i++)
                    {
                        var nowMinRoadDistance_m = i * _blockMaxDistance;
                        var nowMaxRoadDistance_m = Math.Min((i + 1) * _blockMaxDistance, _roadDistance_m);
                        var newBlock = new Block(this, nowMinRoadDistance_m, nowMaxRoadDistance_m);
                        _blocks.Add(newBlock);
                    }
                    break;
                case RoomType.Team:
                    _blocks.Add(new Block(this, 0, int.MaxValue));
                    break;
            }
        }

        public void StartAllBlock()
        {
            for (int i = 0; i < _blocks.Count; i++)
            {
                _blocks[i].Start();
            }
        }

        public void ReleaseAllBlock()
        {
            for (int i = 0; i < _blocks.Count; i++)
            {
                _blocks[i].Release();
            }
            _blocks.Clear();
        }


        #region MapUnit

        private M2C_MapUnitUpdate _m2C_MapUnitUpdate = new M2C_MapUnitUpdate();
        private M2C_MapUnitCreateAndDestroy _m2C_MapUnitCreateAndDestroy = new M2C_MapUnitCreateAndDestroy();

        public void Enter(MapUnit mapUnit, double position)
        {
            ModifyPosition(mapUnit, 0);
        }

        public void Leave(MapUnit mapUnit)
        {
            if (mapUnit != null && mapUnit.Block != null)
            {
                mapUnit.Block.Leave(mapUnit, true);
            }
        }

        public void ModifyPosition(MapUnit mapUnit, double position)
        {
            position %= _roadDistance_m;
            bool IsInside = mapUnit.Block != null && 
                            mapUnit.Block.IsInside(position);

            if (!IsInside)
            {
                BlockChannel oldBlockChannel = null;
                BlockChannel newBlockChannel = null;

                //離開舊的Block
                if (mapUnit.Block != null)
                {
                    oldBlockChannel = mapUnit.SendChannel;
                    mapUnit.Block.Leave(mapUnit, false);
                }

                //加入新的Block
                for (int i = 0; i < _blocks.Count; i++)
                {
                    if (_blocks[i].IsInside(position))
                    {
                        _blocks[i].Enter(mapUnit, position, false);
                        break;
                    }
                }
                newBlockChannel = mapUnit.SendChannel;

                //清空舊的資料
                _m2C_MapUnitCreateAndDestroy.CreateMapUnitInfos.Clear();
                _m2C_MapUnitCreateAndDestroy.DestroyMapUnitIds.Clear();

                //取得觀看的BlockChannel資訊
                for (int i = 0; i < newBlockChannel.PositionMambers.Count; i++)
                {
                    var rev = newBlockChannel.PositionMambers[i];
                    var info = rev.Info;
                    //info.DebugId = IdGenerater.GenerateId();
                    //var list = mapUnit.GetLookAtSources();
                    //BsonDocument doc = new BsonDocument
                    //{
                    //    { "rev.Id" , rev?.Id },
                    //    { "rev.MapAppId" , IdGenerater.GetAppId(rev == null ? 0 : rev.Id) },
                    //    { "rev.Uid" , rev?.Uid },
                    //    { "rev.RoomId" , rev?.RoomId },
                    //    { "rev.Room.Id" , rev?.Room?.Id },
                    //    { "rev.Block.room.Id" , rev?.Block?.room?.Id },
                    //    { "rev.Block.roomId" , rev?.Block?.roomId },
                    //    { "rev.Block.blockId" , rev?.Block?.blockId },
                    //    { "sendToList", list.Select(e => $"MapUnitId:{e.Id}, Uid:{e.Uid}, BlockId:{e.Block?.blockId}").ToJson() }
                    //};
                    //Log.Trace($"DebugId[{info.DebugId}]> {doc.ToJson()}");

                    _m2C_MapUnitCreateAndDestroy.CreateMapUnitInfos.Add(info);
                }

                //移除取消觀看的BlockChannel資訊
                for (int i = 0; i < oldBlockChannel?.PositionMambers?.Count; i++)
                {
                    if (_m2C_MapUnitCreateAndDestroy.CreateMapUnitInfos.Contains(oldBlockChannel.PositionMambers[i].Info))
                        continue;
                    _m2C_MapUnitCreateAndDestroy.DestroyMapUnitIds.Add(oldBlockChannel.PositionMambers[i].Id);
                }

                MapMessageHelper.BroadcastTarget(_m2C_MapUnitCreateAndDestroy, mapUnit.GetLookAtSources());
            }

            //更新MapUnit
            _m2C_MapUnitUpdate.MapUnitId = mapUnit.Id;
            _m2C_MapUnitUpdate.DistanceTravelledTarget = mapUnit.Info.DistanceTravelled;
            _m2C_MapUnitUpdate.SpeedMS = mapUnit.Info.SpeedMS;
            _m2C_MapUnitUpdate.DistanceTravelledUpdateUTCTick = System.DateTime.UtcNow.Ticks;
            SendMessage(mapUnit, _m2C_MapUnitUpdate);
        }

        public void SendMessage(MapUnit sender, IActorMessage message)
        {
            if (sender == null)
            {
                Log.Error("SendMessage 失敗, sender為空");
                return;
            }

            if (sender.SendChannel != null)
            {
                sender.SendChannel.SendMessage(message);
            }
        }

        #endregion
    }
}