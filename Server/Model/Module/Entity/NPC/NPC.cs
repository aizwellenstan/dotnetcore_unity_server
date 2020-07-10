using System;

namespace ETModel
{
    public class NPCData
    {
        public long Id;
        public int Enable;
        public int RoadSettingId;
        public string Name;
        public int Location;
        public int CharacterId;
        public int BicycleId;
        public int BodyId;
        public int DecorationId;
        public double MinSpeed;
        public double MaxSpeed;
        public double RideTime;
        public double RestTime;

        public void SetData(NPCConfig config)
        {
            Id = config.Id;
            Enable = config.Enable;
            RoadSettingId = config.RoadSettingId;
            Name = config.Name;
            Location = config.Location;
            CharacterId = config.CharacterId;
            BicycleId = config.BicycleId;
            BodyId = config.BodyId;
            DecorationId = config.DecorationId;
            MinSpeed = config.MinSpeed;
            MaxSpeed = config.MaxSpeed;
            RideTime = config.RideTime;
            RestTime = config.RestTime;
        }
    }


    public class NPC
    {
        public RoomNpcComponent _roomNPCComponent = null;
        public MapUnit _mapUnit = null;
        public NPCData _data = new NPCData();

        public long _rideAfterTime_ms = 0;
        public long _restAfterTime_ms = 0;

        public NPC(RoomNpcComponent roomNPCComponent, NPCConfig config)
        {
            _roomNPCComponent = roomNPCComponent;
            _data.SetData(config);
        }

        public void Release()
        {
            _roomNPCComponent = null;
            _data = null;
            _mapUnit = null;
        }

        public void DestroyNPC()
        {
            if (_mapUnit != null)
            {
                Game.Scene.GetComponent<MapUnitComponent>().Remove(_mapUnit.Id);
                _mapUnit = null;
                _restAfterTime_ms = TimeHelper.NowAfterTimeSeconds(_data.RestTime);
            }
        }
    }
}