
using ETModel;
using Google.Protobuf;
using System;

namespace ETHotfix
{
    [ObjectSystem]
    public class ReservationComponentStartSystem : StartSystem<ReservationComponent>
    {
        public override void Start(ReservationComponent self)
        {
            self.Start();
        }
    }

    [ObjectSystem]
    public class ReservationComponentDestroySystem : DestroySystem<ReservationComponent>
    {
        public override void Destroy(ReservationComponent self)
        {
            self.Destroy();
        }
    }

    public static class ReservationComponentEx
    {
        public static async void Start(this ReservationComponent self)
        {
            var proxy = Game.Scene.GetComponent<CacheProxyComponent>();
            self.MemorySync = proxy.GetMemorySyncSolver<Reservation>();
            self.MemorySync.onCreate += self.OnCreated;
            self.MemorySync.onWillDelete += self.OnWillDeleted;

            var reservations = await ReservationDataHelper.GetAll();
            for (int i = 0; i < reservations.Count; i++)
            {
                if (reservations[i].allData == null)
                {
                    await ReservationDataHelper.Remove(reservations[i].uid);
                    continue;
                }

                // 解析預約
                ReservationAllData allData = new ReservationAllData();
                CodedInputStream codedInputStream = new CodedInputStream(reservations[i].allData.Bytes);
                allData.MergeFrom(codedInputStream);

                // 判斷是否過期
                if (DateTime.UtcNow.Ticks > allData.AwakeUTCTimeTick)
                {
                    await ReservationDataHelper.Remove(reservations[i].uid);
                    continue;
                }

                // 實體化預約
                await self.CreateReservation(allData);
            }
        }

        public static void Destroy(this ReservationComponent self)
        {
            if (self.IsDisposed)
            {
                return;
            }
            self.Dispose();

            foreach (Reservation reservation in self._idReservationDict.Values)
            {
                reservation.Dispose();
            }
            self._idReservationDict.Clear();
            self._uIdReservationDict.Clear();

            self.MemorySync.onCreate -= self.OnCreated;
            self.MemorySync.onWillDelete -= self.OnWillDeleted;
            // 非擁有者請勿操作Dispose
            self.MemorySync.Dispose();
        }

        private static void OnCreated(this ReservationComponent self, long id)
        {
            //if (self.MemorySync.IsMine(id))
            //    return;
            Reservation reservation = self.MemorySync.Get<Reservation>(id);
            if(reservation == null)
            {
                // GG跑到這邊表示有問題
                Log.Error($"Reservation[{id}] is missing!");
            }
            else
            {
                self._AddReservation(reservation);
            }
        }

        private static void OnWillDeleted(this ReservationComponent self, long id)
        {
            //if (self.MemorySync.IsMine(id))
            //    return;
            Reservation reservation = self.MemorySync.Get<Reservation>(id);
            if (reservation == null)
            {
                // GG跑到這邊表示有問題
                Log.Error($"Reservation[{id}] is missing!");
            }
            else
            {
                self._RemoveReservation(id);
            }
        }

        public static async ETTask<Reservation> CreateReservation(this ReservationComponent self, ReservationAllData reservationData)
        {
            Reservation reservation = ComponentFactory.CreateWithId<Reservation>(reservationData.ReservationId);
            reservation.SetData(reservationData);
            await self.MemorySync.Create(reservation);
            reservation.IsInitialized = true;
            return reservation;
        }

        public static async ETTask<bool> UpdateReservation(this ReservationComponent self, Reservation reservation)
        {
            var obj = await self.MemorySync.Update(reservation);
            return obj != null;
        }

        public static async ETTask<bool> DestroyReservation(this ReservationComponent self, long reservationId)
        {
            if (self._RemoveReservation(reservationId))
            {
                await self.MemorySync.Delete<Reservation>(reservationId);
                return true;
            }
            return false;
        }
    }
}