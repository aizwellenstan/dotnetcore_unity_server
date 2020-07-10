using System;
using ETModel;

namespace ETHotfix
{
    [ActorMessageHandler(AppType.Lobby)]
    public class C2L_GetUserAllEquipmentHandler : AMActorLocationRpcHandler<Player, C2L_GetUserAllEquipment, L2C_GetUserAllEquipment>
    {
        protected override async ETTask Run(Player player, C2L_GetUserAllEquipment message, Action<L2C_GetUserAllEquipment> reply)
        {
            await RunAsync(player, message, reply);
        }

        private async ETTask RunAsync(Player player, C2L_GetUserAllEquipment message, Action<L2C_GetUserAllEquipment> reply)
        {
            L2C_GetUserAllEquipment response = new L2C_GetUserAllEquipment();
            try
            {
                long uid = player.uid;
                response.Uid = uid;
                response.EquipmentInfoList = await EquipmentDataHelper.GetUserAllEquipmentInfo(uid);
                reply(response);
            }
            catch (Exception e)
            {
                ReplyError(response, e, reply);
            }
        }
    }
}
