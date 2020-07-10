using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;
using ETModel;
using System;

namespace ETHotfix
{
    //[ObjectSystem]
    //public class RoomStartSystem : StartSystem<Room>
    //{
    //    public override void Start(Room self)
    //    {
    //        self.Start();
    //    }
    //}

    public static class NPCSystemSystem
    {
        public static void Start(this NPC self)
        {
            self.CreateNPC();
        }

        public static void RefreshConfig(this NPC self, NPCConfig config)
        {
            if (self._mapUnit != null)
                self.DestroyNPC();

            self._data.SetData(config);

            Room room = self._roomNPCComponent.Entity as Room;
            if (config.RoadSettingId != room.info.RoadSettingId)
                return;

            self.CreateNPC();
        }

        private static void UpdateNPC(this NPC self)
        {
            if (self._mapUnit != null)
            {
                if (TimeHelper.ClientNowMilliSeconds() > self._rideAfterTime_ms)
                {
                    self.DestroyNPC();
                }
            }
            else
            {
                if (TimeHelper.ClientNowMilliSeconds() > self._restAfterTime_ms)
                {
                    self.CreateNPC();
                }
            }
        }

        public static void Update(this NPC self)
        {
            self.UpdateNPC();
        }

        public static async void CreateNPC(this NPC self)
        {
            if (self._data.Enable == 0)
                return;

            Room room = self._roomNPCComponent.Entity as Room;
            self._mapUnit = ComponentFactory.CreateWithId<MapUnit, MapUnitType>(IdGenerater.GenerateId(), MapUnitType.Npc);
            ETHotfix.MapUnitInfo mapUnitInfo = new ETHotfix.MapUnitInfo()
            {
                Name = self._data.Name,
                RoomId = room.Id,
                DistanceTravelled = 0,
                PathId = (int)(self._data.Id % 4),
                StartUTCTick = DateTime.UtcNow.Ticks,
                CharSetting = new ETHotfix.PlayerCharSetting()
            };

            //Location
            if (self._data.Location <= -1)
            {
                mapUnitInfo.Location = (int)(self._data.Id % 264);
            }
            else
            {
                mapUnitInfo.Location = self._data.Location;
            }

            //CharacterId
            if (self._data.CharacterId <= -1)
            {
                mapUnitInfo.CharSetting.CharacterId = self._roomNPCComponent.GetRandomCharacterId();
            }
            else
            {
                mapUnitInfo.CharSetting.CharacterId = self._data.CharacterId;
            }

            //BodyId
            if (self._data.BodyId <= -1)
            {
                mapUnitInfo.CharSetting.BodyId = self._roomNPCComponent.GetRandomBodyId();
            }
            else
            {
                mapUnitInfo.CharSetting.BodyId = self._data.BodyId;
            }

            //BicycleId
            if (self._data.BicycleId <= -1)
            {
                mapUnitInfo.CharSetting.BicycleId = self._roomNPCComponent.GetRandomBicycleId();
            }
            else
            {
                mapUnitInfo.CharSetting.BicycleId = self._data.BicycleId;
            }

            // DecorationId
            if (self._data.DecorationId <= -1)
            {
                mapUnitInfo.CharSetting.DecorationId = self._roomNPCComponent.GetRandomDecorationId();
            }
            else
            {
                mapUnitInfo.CharSetting.DecorationId = self._data.DecorationId;
            }

            self._mapUnit.Uid = self._data.Id;
            self._mapUnit.SetInfo(mapUnitInfo);
            await self._mapUnit.EnterRoom(room.Id);
            await Game.Scene.GetComponent<RoomComponent>().Update(self._mapUnit.Room);

            var mapUnitFsmMoveComponent = self._mapUnit.AddComponent<MapUnitFsmMoveComponent>();
            mapUnitFsmMoveComponent.SpeedMin = (float)self._data.MinSpeed;
            mapUnitFsmMoveComponent.SpeedMax = (float)self._data.MaxSpeed;

            Game.Scene.GetComponent<MapUnitComponent>().Add(self._mapUnit);

            self._rideAfterTime_ms = TimeHelper.NowAfterTimeSeconds(self._data.RideTime);
        }
    }
}