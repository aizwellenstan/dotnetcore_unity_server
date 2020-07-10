using ETModel;
using System;
using System.Collections.Generic;

namespace ETHotfix
{
	[ActorMessageHandler(AppType.Map)]
	public class C2M_TeamKickHandler : AMActorLocationRpcHandler<MapUnit, C2M_TeamKick, M2C_TeamKick>
    {
        protected override async ETTask Run(MapUnit mapUnit, C2M_TeamKick message, Action<M2C_TeamKick> reply)
        {
            await ETTask.CompletedTask;

            M2C_TeamKick response = new M2C_TeamKick();
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

                if (mapUnit.Room.State != RoomState.Start)
                {
                    response.Error = ErrorCode.ERR_RoomTeamStateCanNotKick;
                    reply(response);
                    return;
                }

                // 剔除該隊員
                MapUnit kickMember = mapUnit.Room.GetMapUnitByUid(message.Uid);
                if (kickMember == null)
                {
                    response.Error = ErrorCode.ERR_RoomTeamCanNotFindPlayerForKick;
                    reply(response);
                    return;
                }
                RoomTeamHelper.KickMember(roomTeamComponent, kickMember, TeamLoseType.Kicked);

                response.Error = 0;
                reply(response);
            }
            catch (Exception e)
            {
                ReplyError(response, e, reply);
            }
        }
    }
}