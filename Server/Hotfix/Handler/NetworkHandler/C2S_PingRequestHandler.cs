using System;
using ETModel;

namespace ETHotfix
{
    [MessageHandler(AppType.Realm | AppType.Gate)]
    public class C2S_PingRequestHandler : AMRpcHandler<C2S_Ping, S2C_Ping>
    {
        protected override void Run(Session session, C2S_Ping message, Action<S2C_Ping> reply)
        {
            var response = new S2C_Ping();

            try
            {
                Game.Scene.GetComponent<PingComponent>().UpsertSession(session.Id);
                
                reply(response);
            }
            catch (Exception e)
            {
                ReplyError(response, e, reply);
            }
        }
    }
}