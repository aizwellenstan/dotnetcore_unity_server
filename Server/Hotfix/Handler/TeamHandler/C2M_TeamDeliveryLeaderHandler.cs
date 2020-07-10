using ETModel;
using System;

namespace ETHotfix
{
	[ActorMessageHandler(AppType.Map)]
	public class C2M_TeamDeliveryLeaderHandler : AMActorLocationRpcHandler<MapUnit, C2M_TeamDeliveryLeader, M2C_TeamDeliveryLeader>
    {
        protected override async ETTask Run(MapUnit mapUnit, C2M_TeamDeliveryLeader message, Action<M2C_TeamDeliveryLeader> reply)
        {
            await ETTask.CompletedTask;

            M2C_TeamDeliveryLeader response = new M2C_TeamDeliveryLeader();
            try
            {
                if (mapUnit.Room == null)
                {
                    response.Error = ErrorCode.ERR_RoomIdNotFound;
                    reply(response);
                    return;
                }

                if (mapUnit.Room.Type != RoomType.Team)
                {
                    response.Error = ErrorCode.ERR_RoonTypeError;
                    reply(response);
                    return;
                }

                var roomTeamComponent = mapUnit.Room.GetComponent<RoomTeamComponent>();
                if (roomTeamComponent == null)
                {
                    response.Error = ErrorCode.ERR_RoomTeamComponentNull;
                    reply(response);
                    return;
                }

                if (roomTeamComponent.Data?.LeaderUid != mapUnit.Uid)
                {
                    response.Error = ErrorCode.ERR_RoomTeamIsNotLeader;
                    reply(response);
                    return;
                }

                roomTeamComponent.Data.LeaderUid = message.Uid;
                response.Error = 0;
                reply(response);

                M2C_TeamModifyData m2c_TeamModifyData = new M2C_TeamModifyData();
                m2c_TeamModifyData.Data = roomTeamComponent.Data;
                MapMessageHelper.BroadcastRoom(mapUnit.RoomId, m2c_TeamModifyData);
            }
            catch (Exception e)
            {
                ReplyError(response, e, reply);
            }
        }
    }
}