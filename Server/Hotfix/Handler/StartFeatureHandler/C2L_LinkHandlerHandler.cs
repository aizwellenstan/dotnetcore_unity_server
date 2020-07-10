using System;
using ETModel;

namespace ETHotfix
{
    [ActorMessageHandler(AppType.Lobby)]
    public class C2L_LinkHandlerHandler : AMActorLocationRpcHandler<Player, C2L_Link, L2C_Link>
    {
        protected override async ETTask Run(Player player, C2L_Link message, Action<L2C_Link> reply)
        {
            await ETTask.CompletedTask;
            RunAsync(player, message, reply).Coroutine();
        }
        
        public async ETVoid RunAsync(Player player, C2L_Link request, Action<L2C_Link> reply)
        {
            L2C_Link response = new L2C_Link();
            try
            {
                if (request.Info == null)
                {
                    response.Error = ErrorCode.ERR_AuthenticationIsNull;
                }
                else
                {
                    switch (request.Info.Type)
                    {
                        case AuthenticationType.FaceBook:
                            await AuthenticationHelper.LinkByFaceBook(player, request.Info, response);
                            break;
                        case AuthenticationType.AppleId:
                            await AuthenticationHelper.LinkByAppleId(player, request.Info, response);
                            break;
                        default:
                            response.Error = ErrorCode.ERR_AuthenticationTypeError;
                            break;
                    }
                }
                reply(response);
            }
            catch (Exception e)
            {
                ReplyError(response, e, reply);
            }
        }
    }
}
