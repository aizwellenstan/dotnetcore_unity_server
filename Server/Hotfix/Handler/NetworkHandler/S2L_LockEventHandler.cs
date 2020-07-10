using System;
using ETModel;

namespace ETHotfix
{
    [MessageHandler(AppType.Location)]
    public class S2L_LockEventHandler : AMRpcHandler<S2L_LockEvent, L2S_LockEvent>
    {
        protected override void Run(Session session, S2L_LockEvent message, Action<L2S_LockEvent> reply)
        {
            var response = new L2S_LockEvent();

            try
            {
                var result = Game.Scene.GetComponent<LocationComponent>().LockEvent(message.Id, message.Key, message.Timeout);
                response.Error = result ? ErrorCode.ERR_Success : ErrorCode.ERR_LocationWaitLock;
                reply(response);
            }
            catch (Exception e)
            {
                ReplyError(response, e, reply);
            }
        }
    }
}