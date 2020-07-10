using ETModel;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ETHotfix
{
	[ActorMessageHandler(AppType.Map)]
	public class C2M_GiveEmoticonHandler : AMActorLocationHandler<MapUnit, C2M_GiveEmoticon>
    {
        protected override void Run(MapUnit mapUnit, C2M_GiveEmoticon message)
        {
            RunAsync(mapUnit, message).Coroutine();
        }

        protected async ETTask RunAsync(MapUnit mapUnit, C2M_GiveEmoticon message)
        {
            await ETTask.CompletedTask;
            try
            {
                M2C_GiveEmoticon m2C_GiveEmoticon = new M2C_GiveEmoticon();
                if(mapUnit.Id == message.MapUnitId)
                {
                    Log.Error("給自己按讚是不行的!");
                    return;
                }
                MapUnit target = mapUnit.Room.GetMapUnitById(message.MapUnitId);
                User user = await UserDataHelper.FindOneUser(mapUnit.Uid);
                m2C_GiveEmoticon.Name = user?.name;
                m2C_GiveEmoticon.EmoticonIndex = message.EmoticonIndex;
                MapMessageHelper.BroadcastTarget(m2C_GiveEmoticon, new List<MapUnit> { target });
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }
    }
}