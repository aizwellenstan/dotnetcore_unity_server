using ETHotfix;
using System.Collections.Generic;

namespace ETModel
{
    public sealed partial class Reservation : Entity
	{
        [CacheIgnore]
        public ReservationState State
        {
            set
            {
                state = (int)value;
            }
            get
            {
                return (ReservationState)state;
            }
        }

        /// <summary>
        /// 是否被初始化完成
        /// </summary>
        [CacheIgnore]
        public bool IsInitialized { get; set; } = false;

        public void Awake()
		{
            allData = null;
            data = null;
            State = ReservationState.Sleep;
            room = null;
            IsInitialized = false;
        }

        public void SetData(ReservationAllData allData)
        {
            this.allData = allData;
            data = new ReservationData()
            {
                ReservationId = allData.ReservationId,
                SenderName = allData.SenderName,
                SenderUid = allData.SenderUid,
                StartUTCTimeTick = allData.StartUTCTimeTick,
            };
        }

        public override void Dispose()
		{
			if (this.IsDisposed)
			{
				return;
			}

            // 擁有者才能廣播
            if (CacheHelper.IsMine<Reservation>(Id))
            {
                // 告知成員刪除該邀請(只傳在線上的)
                if (allData?.MemberUid.count > 0)
                {
                    List<long> targetUids = new List<long>();
                    for (int i = 0; i < allData.MemberUid.Count; i++)
                    {
                        long targetUid = allData.MemberUid[i];
                        Player reservationTarget = CacheHelper.GetFromCache<Player>(targetUid);
                        if (reservationTarget == null)
                            continue;
                        targetUids.Add(targetUid);
                    }

                    G2C_TeamReservationRemove g2c_TeamReservationRemove = new G2C_TeamReservationRemove();
                    g2c_TeamReservationRemove.ReservationId = allData.ReservationId;
                    GateMessageHelper.BroadcastTarget(g2c_TeamReservationRemove, targetUids);
                }
            }

            base.Dispose();
        }
    }
}