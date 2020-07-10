using ETHotfix;
using MongoDB.Bson;
using System;
using System.Collections.Generic;

namespace ETModel
{
    public enum MapUnitType
    {
        Hero,
        Npc
    }

    public sealed class MapUnit : Entity
    {
        public long Uid { get; set; } = 0;
        public MapUnitType MapUnitType { get; private set; }

        public void Awake(MapUnitType mapUnitType)
        {
            MapUnitType = mapUnitType;
            Room = null;
            Uid = 0;
            RoomId = 0;
            Info = null;
            StartRideTimeUtcTick = -1;
            EndRideTimeUtcTick = -1;
            _topSpeedMS = float.MinValue;
            Rank = -1;
            Calories = 0;
            Power = 0;
        }

        public override void Dispose()
        {
            if (this.IsDisposed)
            {
                return;
            }
            base.Dispose();

            LookAtSources.Clear();
        }

        #region Info

        private MapUnitInfo _info;

        public MapUnitInfo Info { get; set; }

        public MapUnitInfo_Global GlobalInfo { get; set; }

        public void SetInfo(MapUnitInfo mapUnitInfo)
        {
            mapUnitInfo.MapUnitId = Id;
            Info = mapUnitInfo;
            Info.Uid = Uid;
            GlobalInfo = new MapUnitInfo_Global()
            {
                MapUnitId = mapUnitInfo.MapUnitId,
                Name = mapUnitInfo.Name,
                Location = mapUnitInfo.Location
            };
        }

        #endregion


        #region RoadInfo

        public RideRecord CreateRideRecord(long teamId = 0L)
        {
            var rideRecordInfo = ComponentFactory.CreateWithId<RideRecord>(IdGenerater.GenerateId());
            rideRecordInfo.uid = Uid;
            rideRecordInfo.teamId = teamId;
            rideRecordInfo.roadConfigId = (Room?.info?.RoadSettingId).GetValueOrDefault();
            rideRecordInfo.rank = Rank;
            rideRecordInfo.mileage = (int)Math.Floor(Info.DistanceTravelled);
            rideRecordInfo.cumulativeSecond = GetCumulativeTime();
            rideRecordInfo.calories = Calories;
            rideRecordInfo.topSpeed = GetTopSpeedKMH();
            rideRecordInfo.averageSpeed = rideRecordInfo.mileage * 3.6f / rideRecordInfo.cumulativeSecond;
            rideRecordInfo.power = Power;
            rideRecordInfo.createAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var roomTeamComponent = Room.GetComponent<RoomTeamComponent>();
            if (roomTeamComponent != null)
            {
                rideRecordInfo.rideType = (int)RideRecord.RideType.Party;
            }
            else
            {
                rideRecordInfo.rideType = (int)RideRecord.RideType.Roam;
            }
            return rideRecordInfo;
        }

        public PlayerRideTotalInfo CreateRideTotalInfo()
        {
            var rideTotalInfo = new PlayerRideTotalInfo();
            rideTotalInfo.Mileage = (int)Math.Floor(Info.DistanceTravelled);
            rideTotalInfo.CumulativeTime = GetCumulativeTime();
            rideTotalInfo.TopSpeed = GetTopSpeedKMH();
            rideTotalInfo.Calories = Calories;
            rideTotalInfo.AverageSpeed = rideTotalInfo.Mileage * 3.6f / rideTotalInfo.CumulativeTime;
            return rideTotalInfo;
        }

        #endregion

        #region Rank

        public int Rank { get; private set; } = -1;

        public void SetRank(int rank)
        {
            Rank = rank;
        }

        #endregion

        #region Room

        public Room Room { get; set; }

        public long RoomId { get; set; } = 0;

        public RoomBlockComponent RoomBlockComponent { private get; set; }

        #endregion

        #region Block

        public Block Block = null;
        public BlockChannel SendChannel = null;
        public MapUnit LookAtTarget = null;
        public List<MapUnit> LookAtSources = new List<MapUnit>();
        private M2C_MapUnitCreateAndDestroy _m2C_MapUnitCreateAndDestroy = new M2C_MapUnitCreateAndDestroy();

        public void LookMapUnitBlockChannel(MapUnit target)
        {
            if (target == null)
            {
                Log.Error($"觀看對象不存在, SelfUid:{Uid}");
                return;
            }
            if (target.SendChannel == null)
                return;
            if (target == LookAtTarget)
                return;

            if (RoomId == 0 || RoomId != target.RoomId)
                return;

            BlockChannel oldBlockChannel = null;
            BlockChannel newBlockChannel = null;

            _m2C_MapUnitCreateAndDestroy.CreateMapUnitInfos.Clear();
            _m2C_MapUnitCreateAndDestroy.DestroyMapUnitIds.Clear();

            //取消觀看舊的BlockChannel
            if (LookAtTarget != null && LookAtTarget.SendChannel != null)
            {
                LookAtTarget.SendChannel.Unlook(this);
                if (LookAtTarget.LookAtSources.Contains(this))
                    LookAtTarget.LookAtSources.Remove(this);
                oldBlockChannel = LookAtTarget.SendChannel;
            }

            //觀看新的BlockChannel
            target.SendChannel.Look(this);
            LookAtTarget = target;
            if (!LookAtTarget.LookAtSources.Contains(this))
                LookAtTarget.LookAtSources.Add(this);
            newBlockChannel = target.SendChannel;

            //清空舊的資料
            _m2C_MapUnitCreateAndDestroy.CreateMapUnitInfos.Clear();
            _m2C_MapUnitCreateAndDestroy.DestroyMapUnitIds.Clear();

            //取得觀看的BlockChannel資訊
            for (int i = 0; i < newBlockChannel.PositionMambers.Count; i++)
            {
                var rev = newBlockChannel.PositionMambers[i];
                var info = rev.Info;
                //info.DebugId = IdGenerater.GenerateId();
                //BsonDocument doc = new BsonDocument
                //{
                //    { "Block.room.Id" , Block?.room?.Id },
                //    { "Block.roomId" , Block?.roomId },
                //    { "Block.blockId" , Block?.blockId },
                //    { "rev.Id" , rev?.Id },
                //    { "rev.MapAppId" , IdGenerater.GetAppId(rev == null ? 0 : rev.Id) },
                //    { "rev.Uid" , rev?.Uid },
                //    { "rev.RoomId" , rev?.RoomId },
                //    { "rev.Room.Id" , rev?.Room?.Id },
                //    { "rev.Block.room.Id" , rev?.Block?.room?.Id },
                //    { "rev.Block.roomId" , rev?.Block?.roomId },
                //    { "rev.Block.blockId" , rev?.Block?.blockId },
                //    { "sendToList", $"MapUnitId:{this.Id}, Uid:{this.Uid}, BlockId:{this.Block?.blockId}" }
                //};
                //Log.Trace($"DebugId[{info.DebugId}]> {doc.ToJson()}");

                _m2C_MapUnitCreateAndDestroy.CreateMapUnitInfos.Add(info);
            }

            // 移除取消觀看的BlockChannel資訊
            for (int i = 0; i < oldBlockChannel?.PositionMambers?.Count; i++)
            {
                if (_m2C_MapUnitCreateAndDestroy.CreateMapUnitInfos.Contains(oldBlockChannel.PositionMambers[i].Info))
                    continue;
                _m2C_MapUnitCreateAndDestroy.DestroyMapUnitIds.Add(oldBlockChannel.PositionMambers[i].Id);
            }

            MapMessageHelper.BroadcastTarget(_m2C_MapUnitCreateAndDestroy, this);
        }

        public void UnlookMapUnitBlockChannel()
        {
            if (LookAtTarget != null && LookAtTarget.SendChannel != null)
            {
                LookAtTarget.SendChannel.Unlook(this);
            }
            LookAtTarget = null;
        }

        public void UnlookAllSource()
        {
            //自身離開房間，讓觀看者看自己
            for (int i = 0; i < LookAtSources.Count; i++)
            {
                if (LookAtSources[i] != this)
                    LookAtSources[i].LookMapUnitBlockChannel(LookAtSources[i]);
            }
            LookAtSources.Clear();
        }

        #endregion

        #region Start/End Time

        public long StartRideTimeUtcTick { get; private set; } = -1;
        public long EndRideTimeUtcTick { get; private set; } = -1;

        public void TrySetStartTime(bool isForce = false)
        {
            if (StartRideTimeUtcTick < 0 || isForce)
            {
                StartRideTimeUtcTick = System.DateTime.UtcNow.Ticks;
            }
        }

        public void TrySetEndTime()
        {
            if (EndRideTimeUtcTick < 0)
            {
                EndRideTimeUtcTick = System.DateTime.UtcNow.Ticks;
            }
        }

        #endregion

        #region Speed

        private float _topSpeedMS = 0;

        public void SetSpeedMS(float speedMS)
        {
            _topSpeedMS = Math.Max(_topSpeedMS, speedMS);
            Info.SpeedMS = speedMS;
        }

        public float GetTopSpeedKMH()
        {
            //m/s * 3.6 = km/h
            return _topSpeedMS * 3.6f;
        }

        #endregion

        #region CumulativeTime

        /// <summary>
        /// 單位(秒)
        /// </summary>
        /// <returns></returns>
        public long GetCumulativeTime()
        {
            if (StartRideTimeUtcTick <= 0)
            {
                return 0;
            }
            return Convert.ToInt64(Math.Floor((EndRideTimeUtcTick - StartRideTimeUtcTick) * 0.0000001));
        }

        #endregion

        #region DistanceTravelled

        public void SetDistanceTravelled(double distanceTravelled)
        {
            Info.DistanceTravelled = distanceTravelled;

            //告訴Block更新位置
            if (RoomBlockComponent != null)
            {
                RoomBlockComponent.ModifyPosition(this, Info.DistanceTravelled);
            }
            else
            {
                Log.Error($"SetDistanceTravelled Failed, 找不到RoomBlockComponent, RoomId:{Room.Id},  Uid:{Uid}");
            }
        }

        #endregion

        #region Calories

        public float Calories { get; private set; } = 0;

        public void SetCalories(float calories)
        {
            Calories = calories;
        }

        #endregion

        #region Power

        public float Power { get; private set; } = 0;

        public void SetPower(float power)
        {
            Power = power;
        }

        #endregion

        public List<MapUnit> GetLookAtSources()
        {
            for (int i = 0; i < LookAtSources.Count; i++)
            {
                var tar = LookAtSources[i];
                if(tar.IsDisposed)
                {
                    LookAtSources.Remove(tar);
                }
                var player = CacheHelper.GetFromCache<Player>(tar.Uid);
                Log.Error($"Uid:{player.uid}> MapUnitId:{player.mapUnitId},IsDisposed:{tar.IsDisposed},RoomId:{player.roomID}");
            }
            return LookAtSources;
        }
    }
}
