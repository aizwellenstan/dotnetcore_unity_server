using System.Collections.Generic;

namespace ETModel
{
	public class Block 
	{
        private RoomBlockComponent _roomBlockComponent = null;
        public double _minRoadDistance_m { private set; get; }
        public double _maxRoadDistance_m { private set; get; }
        private List<BlockChannel> _blockChannels = new List<BlockChannel>();

        public long roomId => _roomBlockComponent != null ? _roomBlockComponent.Entity.Id : -1;

        public Room room => _roomBlockComponent != null ? (Room)_roomBlockComponent.Entity : null;

        public long blockId { private set; get; }

        public Block(RoomBlockComponent roomBlockComponent, double minRoadDistance, double maxRoadDistance)
        {
            blockId = IdGenerater.GenerateId();
            _roomBlockComponent = roomBlockComponent;
            _minRoadDistance_m = minRoadDistance;
            _maxRoadDistance_m = maxRoadDistance;
        }

        public void Start()
        {
            for (int i = 0; i < _blockChannels.Count; i++)
            {
                if (_blockChannels[i] != null)
                {
                    _blockChannels[i].Start();
                }
            }
        }

        public void Release()
        {
            for (int i = 0; i < _blockChannels.Count; i++)
            {
                if (_blockChannels[i] != null)
                {
                    _blockChannels[i].Release();
                }
            }
            _blockChannels.Clear();
            _roomBlockComponent = null;
        }

        #region MapUnit

        public void Enter(MapUnit mapUnit, double position, bool isPreEnter)
        {
            if (mapUnit == null)
            {
                Log.Error($"Block.Enter() Failed, mapUnit == null");
                return;
            }

            //進入該Block
            if (!isPreEnter)
            {
                mapUnit.Block = this;
            }

            //判斷是否已有本Block的SendChannel
            if (mapUnit.SendChannel?.Block == this)
            {
                return;
            }

            //找尋還有空位的BlockChannel
            BlockChannel freeChannel = null;
            for (int i = 0; i < _blockChannels.Count; i++)
            {
                if (!_blockChannels[i].IsFullMember())
                {
                    freeChannel = _blockChannels[i];
                    break;
                }
            }

            //新開一個BlockChannel
            if (freeChannel == null)
            {
                freeChannel = new BlockChannel(this);
                freeChannel.Start();
                _blockChannels.Add(freeChannel);
            }

            //進入
            freeChannel.Enter(mapUnit, position);
        }

        public void Leave(MapUnit mapUnit, bool isLeaveRoom)
        {
            if (mapUnit == null)
            {
                Log.Error($"Block.Leave() Failed, mapUnit == null");
                return;
            }

            mapUnit.Block = null;
            if (mapUnit.SendChannel?.Block == this)
            {
                mapUnit.SendChannel.Leave(mapUnit, isLeaveRoom);
            }
        }

        public bool IsInside(double position)
        {
            switch (room.Type)
            {
                case RoomType.Roaming:
                    return position >= _minRoadDistance_m && position < _maxRoadDistance_m;
                case RoomType.Team:
                    return true;
            }
            return false;
        }

        #endregion
    }
}