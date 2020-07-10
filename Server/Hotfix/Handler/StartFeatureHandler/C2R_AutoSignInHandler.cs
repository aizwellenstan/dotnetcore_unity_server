using System;
using ETModel;
using MongoDB.Bson.Serialization;

namespace ETHotfix
{
    //[MessageHandler(AppType.Realm)]
    //public class C2R_AutoSignInHandler : AMRpcHandler<C2R_AutoSignIn, R2C_SignIn>
    //{
    //    protected override void Run(Session session, C2R_AutoSignIn message, Action<R2C_SignIn> reply)
    //    {
    //        RunAsync(session, message, reply).Coroutine();
    //    }

    //    private async ETVoid RunAsync(Session session, C2R_AutoSignIn message, Action<R2C_SignIn> reply)
    //    {
    //        R2C_SignIn response = new R2C_SignIn();
    //        try
    //        {
    //            SignInCryptographyHelper.Token tok = null;
    //            try
    //            {
    //                var plain = SignInCryptographyHelper.Decrypt(message.Token);
    //                tok = BsonSerializer.Deserialize<SignInCryptographyHelper.Token>(plain);
    //            }
    //            catch (Exception)
    //            {
    //                response.Error = ErrorCode.ERR_InvalidToken;
    //                reply(response);
    //                return;
    //            }
    //            User user = await UserManager.FindOneUser(tok.uid);
    //            if (user != null)
    //            {
    //                if (user.salt != tok.salt || user.lastCreateTokenAt != tok.lastCreateTokenAt)
    //                {
    //                    response.Error = ErrorCode.ERR_InvalidToken;
    //                }
    //                else
    //                {
    //                    user.lastOnlineAt = DateTimeOffset.Now.ToUnixTimeMilliseconds();
    //                    user.firebaseDeviceToken = message.FirebaseDeviceToken;
    //                    await UserManager.UpsertUser(user);

    //                    //從Realm轉登到Gate並傳給Client用戶資料
    //                    await AuthenticationHelper.RealmToGate(session, user, response, false);
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
