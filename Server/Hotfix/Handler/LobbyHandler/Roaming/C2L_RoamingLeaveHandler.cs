using System;
using System.Collections.Generic;
using ETModel;

namespace ETHotfix
{
    [ActorMessageHandler(AppType.Lobby)]
    public class C2L_RoamingLeaveHandler : AMActorLocationRpcHandler<Player, C2L_RoamingLeave, L2C_RoamingLeave>
    {
        protected override async ETTask Run(Player player, C2L_RoamingLeave message, Action<L2C_RoamingLeave> reply)
        {
            await ETTask.CompletedTask;

            L2C_RoamingLeave response = new L2C_RoamingLeave();
            try
            {
                var lobbyComponent = Game.Scene.GetComponent<LobbyComponent>();

                if(player.roomID == 0L)
                {
                    response.Error = ErrorCode.ERR_RoomIdNotFound;
                    reply(response);
                    return;
                }

                response.Error = await lobbyComponent.LeaveRoaming(player.roomID, player.uid);
                reply(response);
            }
            catch (Exception e)
            {
                ReplyError(response, e, reply);
            }
        }
    }
}
