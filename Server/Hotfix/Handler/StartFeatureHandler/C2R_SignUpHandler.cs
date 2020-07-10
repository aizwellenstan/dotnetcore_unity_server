using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Net;
using System.Text;
using ETModel;
using Google.Protobuf.Collections;
using MongoDB.Bson.Serialization;

namespace ETHotfix
{
    [MessageHandler(AppType.Realm)]
    public class C2R_SignUpHandler : AMRpcHandler<C2R_SignUp, R2C_SignUp>
    {
        protected override void Run(Session session, C2R_SignUp message, Action<R2C_SignUp> reply)
        {
            RunAsync(session, message, reply).Coroutine();
        }

        private async ETVoid RunAsync(Session session, C2R_SignUp message, Action<R2C_SignUp> reply)
        {
            R2C_SignUp response = new R2C_SignUp();
            try
            {
                var plain = CryptographyHelper.AESDecrypt(message.Secret);
                var req = BsonSerializer.Deserialize<C2R_SignUp>(plain);
                response.Error = await UserDataHelper.SignUp(req.Email, req.Password);
                if(response.Error != ErrorCode.ERR_Success)
                {
                    response.ErrorCodeList = new RepeatedField<int> { ErrorCode.ERR_AccountSisnUpRepeatly };
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
