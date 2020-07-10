using System.Collections.Generic;
using ETHotfix;
using Google.Protobuf.Collections;

namespace ETModel
{
    public class ReservationDataList
    {
        public long Uid = -1;
        public RepeatedField<ReservationData> Datas = new RepeatedField<ReservationData>();
        public int OwnCount = 0;

        public void Add(ReservationData reservationData)
        {
            Datas.Add(reservationData);
            if (reservationData.SenderUid == Uid)
                OwnCount++;
        }

        public void Remove(long reservationId)
        {
            for (int i = 0; i < Datas.Count; i++)
            {
                if (Datas[i].ReservationId == reservationId)
                {
                    if (Datas[i].SenderUid == Uid)
                        OwnCount--;
                    Datas.RemoveAt(i);
                    break;
                }
            }
        }
    }

    public class ReservationComponent : Component
	{
        /// <summary>
        /// 預約同步控制器(擁有者)
        /// </summary>
        public RedisEventSolverComponent MemorySync { get; set; }

        public readonly Dictionary<long, Reservation> _idReservationDict = new Dictionary<long, Reservation>();

        public readonly Dictionary<long, ReservationDataList> _uIdReservationDict = new Dictionary<long, ReservationDataList>();

		public Reservation GetByReservationId(long id)
		{
            _idReservationDict.TryGetValue(id, out var reservation);
			return reservation;
		}

        public ReservationDataList GetByUid(long uid)
        {
            _uIdReservationDict.TryGetValue(uid, out var reservationList);
            return reservationList;
        }

		public int Count
		{
			get
			{
				return _idReservationDict.Count;
			}
		}

        public void _AddReservation(Reservation reservation)
        {
            if (reservation == null)
            {
                Log.Error($"AddReservation Failed, reservation is null!");
                return;
            }

            if (_idReservationDict.ContainsKey(reservation.Id))
            {
                Log.Error($"AddReservation Failed, Id({reservation.Id}) is Exist!");
                return;
            }
   
            _idReservationDict.Add(reservation.Id, reservation);

            for (int i = 0; i < reservation.allData.MemberUid.count; i++)
            {
                long uid = reservation.allData.MemberUid[i];
                if (!_uIdReservationDict.TryGetValue(uid, out var reservationList))
                {
                    reservationList = new ReservationDataList() { Uid = uid };
                    _uIdReservationDict.Add(uid, reservationList);
                }
                reservationList.Add(reservation.data);
            }
        }

        public bool _RemoveReservation(long reservationId)
        {
            if (_idReservationDict.Remove(reservationId, out var reservation))
            {
                for (int m = 0; m < reservation.allData.MemberUid.Count; m++)
                {
                    long uid = reservation.allData.MemberUid[m];
                    if (_uIdReservationDict.TryGetValue(uid, out var reservationList))
                    {
                        reservationList.Remove(reservationId);
                    }
                }
                reservation.Dispose();
                return true;
            }
            return false;
        }
    }
}