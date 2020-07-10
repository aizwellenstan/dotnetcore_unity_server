using ETModel;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ETHotfix
{
	[ActorMessageHandler(AppType.Map)]
	public class C2M_SaveRideRecordHandler : AMActorLocationRpcHandler<MapUnit, C2M_SaveRideRecord, M2C_SaveRideRecord>
    {
        private const int secondForRecord = 600;

        protected override async ETTask Run(MapUnit mapUnit, C2M_SaveRideRecord request, Action<M2C_SaveRideRecord> reply)
        {
            await RunAsync(mapUnit, request, reply);
        }

        protected async ETTask RunAsync(MapUnit mapUnit, C2M_SaveRideRecord request, Action<M2C_SaveRideRecord> reply)
        {
            await ETTask.CompletedTask;
            try
            {
                M2C_SaveRideRecord m2C_SaveRideRecord = new M2C_SaveRideRecord();

                // [同步]取得資料並離開房間
                if (mapUnit.Room.Type == RoomType.Team)
                {
                    m2C_SaveRideRecord.Error = ErrorCode.ERR_NotSupportedType;
                }
                else if(mapUnit.Room.Type == RoomType.Roaming)
                {
                    mapUnit.TrySetEndTime();
                    bool isSaveDB = mapUnit.GetCumulativeTime() >= secondForRecord;
                    if (isSaveDB)
                    {
                        RideInfoHelper.SaveRoadAllInfo(mapUnit, 0L, isSaveDB, isSaveDB);
                    }
                    else
                    {
                        m2C_SaveRideRecord.Error = ErrorCode.ERR_TimeNotUp;
                    }
                }
                reply(m2C_SaveRideRecord);
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }
    }
}
