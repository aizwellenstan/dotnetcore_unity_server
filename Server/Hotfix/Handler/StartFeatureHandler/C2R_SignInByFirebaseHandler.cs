using System;
using ETModel;

namespace ETHotfix
{
    //[MessageHandler(AppType.Realm)]
    //public class C2R_SignInByFirebaseHandler : AMRpcHandler<C2R_SignInByFirebase, R2C_SignIn>
    //{
    //    protected override void Run(Session session, C2R_SignInByFirebase message, Action<R2C_SignIn> reply)
    //    {
    //        RunAsync(session, message, reply).Coroutine();
    //    }

    //    private async ETVoid RunAsync(Session session, C2R_SignInByFirebase message, Action<R2C_SignIn> reply)
    //    {
    //        R2C_SignIn response = new R2C_SignIn();
    //        try
    //        {
    //            //FirebaseToken 轉 FirebaseUid
    //            var decodedToken = await Game.Scene.GetComponent<FirebaseComponent>().firebaseAuth.VerifyIdTokenAsync(message.Token);
    //            string firebaseUid = decodedToken.Uid;

    //            //用FirebaseUid查詢User
    //            User user = await UserManager.FindOneUserByFirebase(firebaseUid);

    //            //用FirebaseUid註冊User
    //            if (user == null)
    //            {
    //                response.Error = await SignInHelper.SignUpByFirebase(firebaseUid);
    //                if (response.Error != ErrorCode.ERR_Success)
    //                {
    //                    reply(response);
    //                    return;
    //                }
    //                user = await UserManager.FindOneUserByFirebase(firebaseUid);
    //                if (user == null)
    //                {
    //                    response.Error = ErrorCode.ERR_SignUpFailed;
    //                    reply(response);
    //                    return;
    //                }
    //            }

    //            //更新User訊息
    //            user.lastOnlineAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    //            user.lastCreateTokenAt = user.lastOnlineAt;
    //            user.firebaseDeviceToken = message.FirebaseDeviceToken;
    //            await UserManager.UpsertUser(user);

    //            //從Realm轉登到Gate並傳給Client用戶資料
    //            await SignInHelper.RealmToGate(session, user, response, true);

    //            reply(response);
    //        }
    //        catch (Exception e)
    //        {
    //            ReplyError(response, e, reply);
    //        }
    //    }
    //}
}
