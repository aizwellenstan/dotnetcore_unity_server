using System;
using System.Collections.Generic;
using System.Net;
using ETModel;
using Google.Protobuf.Collections;

namespace ETHotfix
{
    [MessageHandler(AppType.Map)]
    public class L2M_GetAllMapUnitGlobalInfoOnRoomHandler : AMRpcHandler<L2M_GetAllMapUnitGlobalInfoOnRoom, M2L_GetAllMapUnitGlobalInfoOnRoom>
    {
        protected override void Run(Session session, L2M_GetAllMapUnitGlobalInfoOnRoom message, Action<M2L_GetAllMapUnitGlobalInfoOnRoom> reply)
        {
            RunAsync(session, message, reply);
        }

        private void RunAsync(Session session, L2M_GetAllMapUnitGlobalInfoOnRoom message, Action<M2L_GetAllMapUnitGlobalInfoOnRoom> reply)
        {
            M2L_GetAllMapUnitGlobalInfoOnRoom response = new M2L_GetAllMapUnitGlobalInfoOnRoom();
            try
            {
                var roomComponent = Game.Scene.GetComponent<RoomComponent>();
                var room = roomComponent.Get(message.RoomId);
                if(room == null)
                {
                    response.Error = ErrorCode.ERR_RoomIdNotFound;
                    reply(response);
                    return;
                }
                RepeatedField<MapUnitInfo_Global> mapUnitInfos = new RepeatedField<MapUnitInfo_Global>();
                List<MapUnit> mapUnits = room.GetAll();
                for (int i = 0; i < mapUnits.Count; i++)
                {
                    mapUnitInfos.Add(mapUnits[i].GlobalInfo);
                }
                response.Data = mapUnitInfos;
                reply(response);
            }
            catch (Exception e)
            {
                ReplyError(response, e, reply);
            }
        }
    }
}
