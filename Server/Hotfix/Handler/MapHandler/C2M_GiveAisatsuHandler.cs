using ETModel;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ETHotfix
{
	[ActorMessageHandler(AppType.Map)]
	public class C2M_GiveAisatsuHandler : AMActorLocationHandler<MapUnit, C2M_GiveAisatsu>
    {
        protected override void Run(MapUnit mapUnit, C2M_GiveAisatsu message)
        {
            RunAsync(mapUnit, message).Coroutine();
        }

        protected async ETTask RunAsync(MapUnit mapUnit, C2M_GiveAisatsu message)
        {
            await ETTask.CompletedTask;
            try
            {
                M2C_GiveAisatsu m2C_GiveAisatsu = new M2C_GiveAisatsu();
                if(mapUnit.Id == message.MapUnitId)
                {
                    Log.Error("µ¹¦Û¤v«öÆg¬O¤£¦æªº!");
                    return;
                }
                MapUnit target = mapUnit.Room.GetMapUnitById(message.MapUnitId);
                User user = await UserDataHelper.FindOneUser(mapUnit.Uid);
                m2C_GiveAisatsu.Name = user?.name;
                m2C_GiveAisatsu.Content = message.Content;
                MapMessageHelper.BroadcastTarget(m2C_GiveAisatsu, new List<MapUnit> { target });
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }
    }
}
