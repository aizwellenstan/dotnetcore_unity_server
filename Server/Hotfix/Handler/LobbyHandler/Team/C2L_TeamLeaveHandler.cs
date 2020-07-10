using System;
using System.Collections.Generic;
using ETModel;

namespace ETHotfix
{
    [ActorMessageHandler(AppType.Lobby)]
    public class C2L_TeamLeaveHandler : AMActorLocationRpcHandler<Player, C2L_TeamLeave, L2C_TeamLeave>
    {
        protected override async ETTask Run(Player player, C2L_TeamLeave message, Action<L2C_TeamLeave> reply)
        {
            await ETTask.CompletedTask;

            L2C_TeamLeave response = new L2C_TeamLeave();
            try
            {
                var lobbyComponent = Game.Scene.GetComponent<LobbyComponent>();

                if(player.roomID == 0L)
                {
                    response.Error = ErrorCode.ERR_RoomIdNotFound;
                    reply(response);
                    return;
                }

                response.Error = await lobbyComponent.LeaveTeam(player.roomID, player.uid);
                reply(response);
            }
            catch (Exception e)
            {
                ReplyError(response, e, reply);
            }
        }
    }
}
