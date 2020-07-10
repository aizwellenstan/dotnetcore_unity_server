using System;
using ETModel;
using MongoDB.Bson.Serialization;

namespace ETHotfix
{
    //[MessageHandler(AppType.Realm)]
    //public class C2R_SignInHandler : AMRpcHandler<C2R_SignIn, R2C_SignIn>
    //{
    //    protected override void Run(Session session, C2R_SignIn message, Action<R2C_SignIn> reply)
    //    {
    //        RunAsync(session, message, reply).Coroutine();
    //    }

    //    private async ETTask RunAsync(Session session, C2R_SignIn message, Action<R2C_SignIn> reply)
    //    {
    //        R2C_SignIn response = new R2C_SignIn();
    //        try
    //        {
    //            var plain = CryptographyHelper.AESDecrypt(message.Secret);
    //            var req = BsonSerializer.Deserialize<C2R_SignIn>(plain);
    //            ThirdPartyUser thirdPartyUser = await UserManager.FindOneThirdPartyUser(req.Email, UserManager.tagJPlay);
    //            if (thirdPartyUser != null)
    //            {
    //                User user = await UserManager.FindOneUser(thirdPartyUser.uid);
    //                string hashPassword = CryptographyHelper.MD5Encoding(req.Password, user.salt);
    //                if (user.hashPassword != hashPassword)
    //                {
    //                    response.Error = ErrorCode.ERR_PasswordIncorrect;
    //                }
    //                else
    //                {
    //                    user.lastOnlineAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    //                    user.lastCreateTokenAt = user.lastOnlineAt;
    //                    user.firebaseDeviceToken = message.FirebaseDeviceToken;
    //                    user.language = message.SignInInfo.Language;
    //                    await UserManager.UpsertUser(user);

    //                    //從Realm轉登到Gate並傳給Client用戶資料
    //                    await AuthenticationHelper.RealmToGate(session, user, response, true);
    //                }
    //            }
    //            else
    //            {
    //                response.Error = ErrorCode.ERR_AccountDoesntExist;
    //            }
    //            reply(response);
    //        }
    //        catch (Exception e)
    //        {
    //            ReplyError(response, e, reply);
    //        }
    //    }
    //}
}
