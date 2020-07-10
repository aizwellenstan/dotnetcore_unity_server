using System;
using System.Collections.Generic;
using System.Net;
using ETModel;

namespace ETHotfix
{
    [MessageHandler(AppType.Map)]
    public class L2M_DestroyMapUnitHandler : AMRpcHandler<L2M_DestroyMapUnit, M2L_DestroyMapUnit>
    {
        protected override void Run(Session session, L2M_DestroyMapUnit message, Action<M2L_DestroyMapUnit> reply)
        {
            RunAsync(session, message, reply);
        }

        private void RunAsync(Session session, L2M_DestroyMapUnit message, Action<M2L_DestroyMapUnit> reply)
        {
            M2L_DestroyMapUnit response = new M2L_DestroyMapUnit();
            try
            {
                var mapUnitComponent = Game.Scene.GetComponent<MapUnitComponent>();
                var mapUnit = mapUnitComponent.Get(message.MapUnitId);
                if(mapUnit == null)
                {
                    response.Error = ErrorCode.ERR_MapUnitMissing;
                    reply(response);
                    return;
                }
                mapUnitComponent.Remove(message.MapUnitId);
                reply(response);
            }
            catch (Exception e)
            {
                ReplyError(response, e, reply);
            }
        }
    }
}
