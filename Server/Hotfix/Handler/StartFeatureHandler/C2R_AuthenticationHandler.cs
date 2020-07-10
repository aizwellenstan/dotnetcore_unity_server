using System;
using ETModel;

namespace ETHotfix
{
    [MessageHandler(AppType.Realm)]
    public class C2R_AuthenticationHandler : AMRpcHandler<C2R_Authentication, R2C_Authentication>
    {
        protected override void Run(Session session, C2R_Authentication message, Action<R2C_Authentication> reply)
        {
            RunAsync(session, message, reply).Coroutine();
        }
        
        public async ETVoid RunAsync(Session session, C2R_Authentication request, Action<R2C_Authentication> reply)
        {
            R2C_Authentication response = new R2C_Authentication();
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
                        case AuthenticationType.Token:
                            await AuthenticationHelper.AuthenticationByToken(session, request.Info, response);
                            break;
                        case AuthenticationType.Guest:
                            await AuthenticationHelper.AuthenticationByGuest(session, request.Info, response);
                            break;
                        case AuthenticationType.FaceBook:
                            await AuthenticationHelper.AuthenticationByFaceBook(session, request.Info, response);
                            break;
                        case AuthenticationType.AppleId:
                            await AuthenticationHelper.AuthenticationByAppleId(session, request.Info, response);
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
