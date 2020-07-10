using ETHotfix;
using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ETModel
{
    public static class ReservationDataHelper
    {
        public const int ReservationMaxCount = 5;
        public const long AwakeBeforeTimeTick = TimeSpan.TicksPerHour * 1;

        private static DBProxyComponent dbProxy
        {
            get
            {
                return Game.Scene.GetComponent<DBProxyComponent>();
            }
        }

        public static async ETTask<bool> IsExist(long uid)
        {
            var list = await dbProxy.Query<ReservationDB>(entity => entity.uid == uid);
            return list.Count != 0;
        }

        public static async ETTask Add(long uid, ReservationAllData allData)
        {
            if (await IsExist(uid))
            {
                Log.Error($"AddReservationDB 失敗! uid : {uid} 已經存在!");
                return;
            }
    
            ReservationDB reservationDB = ComponentFactory.CreateWithId<ReservationDB>(IdGenerater.GenerateId());
            reservationDB.uid = uid;
            reservationDB.allData = new MongoDB.Bson.BsonBinaryData(allData.ToByteArray());

            await dbProxy.Save(reservationDB);
        }

        public static async ETTask Remove(long uid)
        {
            await dbProxy.DeleteJson<ReservationDB>(entity => (entity.uid == uid));
        }

        public static async ETTask<List<ReservationDB>> GetAll()
        {
            var list = await dbProxy.Query<ReservationDB>(entity => true);
            return list.OfType<ReservationDB>().ToList();
        }
    }
}
