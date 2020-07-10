using System;
using System.Collections.Generic;
using System.Net;
using ETModel;
using Google.Protobuf.Collections;

namespace ETHotfix
{
    [MessageHandler(AppType.Map)]
    public class L2M_GetAllMapUnitInfoOnRoomHandler : AMRpcHandler<L2M_GetAllMapUnitInfoOnRoom, M2L_GetAllMapUnitInfoOnRoom>
    {
        protected override void Run(Session session, L2M_GetAllMapUnitInfoOnRoom message, Action<M2L_GetAllMapUnitInfoOnRoom> reply)
        {
            RunAsync(session, message, reply);
        }

        private void RunAsync(Session session, L2M_GetAllMapUnitInfoOnRoom message, Action<M2L_GetAllMapUnitInfoOnRoom> reply)
        {
            M2L_GetAllMapUnitInfoOnRoom response = new M2L_GetAllMapUnitInfoOnRoom();
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
                RepeatedField<MapUnitInfo> mapUnitInfos = new RepeatedField<MapUnitInfo>();
                List<MapUnit> mapUnits = room.GetAll();
                for (int i = 0; i < mapUnits.Count; i++)
                {
                    mapUnitInfos.Add(mapUnits[i].Info);
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
