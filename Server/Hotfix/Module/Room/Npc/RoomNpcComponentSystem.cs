using ETModel;
using System;
using System.Collections.Generic;

namespace ETHotfix
{
    [ObjectSystem]
    public class RoomNpcComponentAwakeSystem : AwakeSystem<RoomNpcComponent>
    {
        public override void Awake(RoomNpcComponent self)
        {
            ConfigComponent configComponent = Game.Scene.GetComponent<ConfigComponent>();

            IConfig[] npcConfig = configComponent.GetAll(typeof(NPCConfig));
            for (int i = 0; i < npcConfig.Length; i++)
            {
                self.NPCConfigs.Add(npcConfig[i] as NPCConfig);
            }
            
            IConfig[] characterConfig = configComponent.GetAll(typeof(CharacterConfig));
            for (int i = 0; i < characterConfig.Length; i++)
            {
                if (characterConfig[i].Id >= 1 && characterConfig[i].Id <= 99)
                {
                    self.CharacterIds.Add(characterConfig[i].Id);
                }

                if (characterConfig[i].Id >= 100 && characterConfig[i].Id <= 199)
                {
                    self.BicycleIds.Add(characterConfig[i].Id);
                }

                if (characterConfig[i].Id >= 200 && characterConfig[i].Id <= 499)
                {
                    self.BodyIds.Add(characterConfig[i].Id);
                }

                if (characterConfig[i].Id >= 500 && characterConfig[i].Id <= 505)
                {
                    self.DecorationIds.Add(characterConfig[i].Id);
                }
            }

            self.eventHandler_1 += self.EventHandler_1;
            self.eventHandler_2 += self.EventHandler_2;
        }
    }

    [ObjectSystem]
    public class RoomNpcComponentStartSystem : StartSystem<RoomNpcComponent>
    {
        public override void Start(RoomNpcComponent self)
        {
            self.Start();
        }
    }

    [ObjectSystem]
    public class RoomNpcComponentUpdateSystem : UpdateSystem<RoomNpcComponent>
    {
        public override void Update(RoomNpcComponent self)
        {
            self.Update();
        }
    }

    [ObjectSystem]
    public class RoomNpcComponentDestroySystem : DestroySystem<RoomNpcComponent>
    {
        public override void Destroy(RoomNpcComponent self)
        {
            self.eventHandler_1 -= self.EventHandler_1;
            self.eventHandler_2 -= self.EventHandler_2;

            self.Destroy();
        }
    }

    public static class RoomNpcComponentEx
    {
        private static void StartNPC(this RoomNpcComponent self)
        {
            for (int i = 0; i < self._npcs.Count; i++)
            {
                self._npcs[i].Start();
            }
        }

        private static void UpdateNPC(this RoomNpcComponent self)
        {
            for (int i = 0; i < self._npcs.Count; i++)
            {
                self._npcs[i].Update();
            }
        }

        public static void RefreshConfig(this RoomNpcComponent self, params long[] idList)
        {
            ConfigComponent configComponent = Game.Scene.GetComponent<ConfigComponent>();
            for (int i = 0; i < idList?.Length; i++)
            {
                long id = idList[i];
                if (!configComponent.AllConfig.TryGetValue(typeof(NPCConfig), out ACategory configCategory))
                {
                    Log.Error("ConfigComponent not found key: NPCConfig");
                    continue;
                }

                var newConfig = configCategory.TryGet(id) as NPCConfig;
                if (newConfig == null)
                {
                    Log.Error($"NPCConfig is null, id:{id}");
                    continue;
                }

                if (self._npcDicts.TryGetValue(id, out var npc))
                {
                    npc.RefreshConfig(newConfig);
                }
                else
                {
                    self.CreateNPC(newConfig);
                }
            }
        }

        public static void EventHandler_1(this RoomNpcComponent self, object a)
        {
            self.RefreshConfig((long)a);
        }

        public static void EventHandler_2(this RoomNpcComponent self, object a, object b)
        {
            long maxId = Math.Max((long)a, (long)b);
            long minId = Math.Min((long)a, (long)b);

            List<long> ids = new List<long>();
            for (long i = minId; i <= maxId; i++)
            {
                ids.Add(i);
            }

            self.RefreshConfig(ids.ToArray());
        }
    }
}