using System;
using System.Collections.Generic;

namespace ETModel
{
	public class RoomNpcComponent : Component/*, ISerializeToEntity*/, IEvent
	{
        public List<NPCConfig> NPCConfigs = new List<NPCConfig>();
        public List<long> CharacterIds = new List<long>();
        public List<long> BicycleIds = new List<long>();
        public List<long> BodyIds = new List<long>();
        public List<long> DecorationIds = new List<long>();

        public List<NPC> _npcs = new List<NPC>();
        public Dictionary<long, NPC> _npcDicts = new Dictionary<long, NPC>();

        public Action<object> eventHandler_1;

        public Action<object, object> eventHandler_2;

        public void Start()
        {
            Game.EventSystem.RegisterEvent(EventIdType.RefreshNPC, this);
            //InitNPC();
            //StartNPC();
        }

        public void Update()
        {
            //UpdateNPC();
        }

        public void Destroy()
        {
            //DestroyNPC();
            Game.EventSystem.UnregisterEvent(EventIdType.RefreshNPC, this);
        }

        public void CreateNPC(NPCConfig config)
        {
            Room room = Entity as Room;
            if (config.RoadSettingId != room.info.RoadSettingId)
                return;

            NPC npc = new NPC(this, config);
            _npcDicts.Add(config.Id, npc);
            _npcs.Add(npc);
        }

        private void InitNPC()
        {
            if (!(Entity is Room room))
            {
                //不是Room就跳過
                return;
            }

            if (room.Type != RoomType.Roaming)
            {
                //不是Roaming就跳過
                return;
            }

            for (int i = 0; i < NPCConfigs.Count; i++)
            {
                CreateNPC(NPCConfigs[i]);
            }
        }

        private void DestroyNPC()
        {
            for (int i = 0; i < _npcs.Count; i++)
            {
                _npcs[i].Release();
            }
        }

        public long GetRandomCharacterId()
        {
            if (CharacterIds.Count <= 0)
                return 1;
            return CharacterIds[RandomHelper.RandomNumber(0, CharacterIds.Count)];
        }


        public long GetRandomBicycleId()
        {
            if (BicycleIds.Count <= 0)
                return 100;
            return BicycleIds[RandomHelper.RandomNumber(0, BicycleIds.Count)];
        }

        public long GetRandomDecorationId()
        {
            if (DecorationIds.Count <= 0)
                return 500;
            return DecorationIds[RandomHelper.RandomNumber(0, DecorationIds.Count)];
        }

        public long GetRandomBodyId()
        {
            if (BodyIds.Count <= 0)
                return 200;
            return BodyIds[RandomHelper.RandomNumber(0, BodyIds.Count)];
        }

        void IEvent.Handle()
        {

        }

        void IEvent.Handle(object a)
        {
            eventHandler_1?.Invoke(a);
        }

        void IEvent.Handle(object a, object b)
        {
            eventHandler_2?.Invoke(a, b);
        }

        void IEvent.Handle(object a, object b, object c)
        {

        }
    }
}