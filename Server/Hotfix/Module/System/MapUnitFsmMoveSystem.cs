using ETModel;
using System;

namespace ETHotfix
{
    [ObjectSystem]
    public class MapUnitFsmMoveAwakeSystem : AwakeSystem<MapUnitFsmMoveComponent>
    {
        public override void Awake(MapUnitFsmMoveComponent self)
        {
            self.Reset();
            self.MapUnit = self.GetParent<MapUnit>();
            //self.M2C_UpdateMapUnit.MapUnitId = self.MapUnit.Id;
            self.Enable = true;
        }
    }

    [ObjectSystem]
    public class MapUnitFsmMoveUpdateSystem : UpdateSystem<MapUnitFsmMoveComponent>
    {
        public override void Update(MapUnitFsmMoveComponent self)
        {
            if (!self.Enable)
                return;
            if (self.MapUnit == null)
                return;
            long nowTime = TimeHelper.ClientNowMilliSeconds();
            UpdateSpeed(self, nowTime);
            UpdateMove(self, nowTime);
            UpdateSync(self, nowTime);
        }


        public void UpdateSpeed(MapUnitFsmMoveComponent self, long nowTime)
        {
            if (nowTime > self.SpeedRandomTimeAfter)
            {
                self.SpeedTarget = (float)RandomHelper.RandomDouble() * (self.SpeedMax - self.SpeedMin) + self.SpeedMin;
                self.SpeedRandomTimeAfter = nowTime + RandomHelper.RandomNumber(MapUnitFsmMoveComponent.SpeedRandomTimeIntervalMin, MapUnitFsmMoveComponent.SpeedRandomTimeIntervalMax);
            }

            if (nowTime > self.SpeedLerpTimeAfter)
            {
                if (self.SpeedLerpTimePrevious <= 0)
                    self.SpeedLerpTimePrevious = nowTime;

                long detlaTime = nowTime - self.SpeedLerpTimePrevious;
                if (Math.Abs(self.SpeedTarget - self.SpeedNow) > 0.01f)
                {
                    if (self.SpeedTarget > self.SpeedNow)
                    {
                        self.SpeedNow += 2f * (detlaTime * 0.001f);
                        if (self.SpeedTarget < self.SpeedNow)
                            self.SpeedNow = self.SpeedTarget;
                    }
                    else
                    {
                        self.SpeedNow -= 2f * (detlaTime * 0.001f);
                        if (self.SpeedTarget > self.SpeedNow)
                            self.SpeedNow = self.SpeedTarget;
                    }
                }
                self.SpeedLerpTimePrevious = nowTime;
                self.SpeedLerpTimeAfter = nowTime + MapUnitFsmMoveComponent.SpeedLerpTimeInterval;
            }
        }


        public void UpdateMove(MapUnitFsmMoveComponent self, long nowTime)
        {
            if (nowTime > self.MoveTimeAfter)
            {
                if (self.MoveTimePrevious <= 0)
                    self.MoveTimePrevious = nowTime;
                self.MapUnit.SetDistanceTravelled(self.MapUnit.Info.DistanceTravelled + self.SpeedNow * ((nowTime - self.MoveTimePrevious) * 0.001f));
                self.MapUnit.SetSpeedMS(self.SpeedNow);
                self.MoveTimeAfter = nowTime + MapUnitFsmMoveComponent.MoveTimeInterval;
                self.MoveTimePrevious = nowTime;
                self.CheckDistanceTravelledEnd();
            }
        }

        public void UpdateSync(MapUnitFsmMoveComponent self, long nowTime)
        {
            if (nowTime > self.AsyncTimeAfter)
            {
                self.MapUnit.SetDistanceTravelled(self.MapUnit.Info.DistanceTravelled);
                self.MapUnit.SetSpeedMS(self.MapUnit.Info.SpeedMS);
                //self.M2C_UpdateMapUnit.DistanceTravelledTarget = self.MapUnit.Info.DistanceTravelled;
                //self.M2C_UpdateMapUnit.SpeedMS = self.MapUnit.Info.SpeedMS;
                //self.M2C_UpdateMapUnit.DistanceTravelledUpdateUTCTick = System.DateTime.UtcNow.Ticks;
                //MapMessageHelper.BroadcastRoom(self.MapUnit.RoomId, self.M2C_UpdateMapUnit);
                self.AsyncTimeAfter = nowTime + MapUnitFsmMoveComponent.AsyncTimeInterval + self.MapUnit.Id % 100;
            }
        }
    }

    [ObjectSystem]
    public class MapUnitFsmMoveDestroySystem : DestroySystem<MapUnitFsmMoveComponent>
    {
        public override void Destroy(MapUnitFsmMoveComponent self)
        {
            self.Enable = false;
            self.MapUnit = null;
        }
    }
}