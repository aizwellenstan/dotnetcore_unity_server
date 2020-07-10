using System;
using ETModel;

namespace ETHotfix
{
    [MessageHandler(AppType.Location)]
    public class S2L_UnlockEventHandler : AMRpcHandler<S2L_UnlockEvent, L2S_UnlockEvent>
    {
        protected override void Run(Session session, S2L_UnlockEvent message, Action<L2S_UnlockEvent> reply)
        {
            var response = new L2S_UnlockEvent();

            try
            {
                var result = Game.Scene.GetComponent<LocationComponent>().UnlockEvent(message.Id, message.Key);
                response.Error = result ? ErrorCode.ERR_Success : ErrorCode.ERR_LocationUnlockFailed;
                reply(response);
            }
            catch (Exception e)
            {
                ReplyError(response, e, reply);
            }
        }
    }
}