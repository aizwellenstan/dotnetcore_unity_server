using ETModel;
using MongoDB.Bson;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ETHotfix
{
    public static class UserUtility
    {
        public class SignInResult
        {
            public string Message { get; set; }

            public int Error { get; set; }

            public PlayerBaseInfo playerBaseInfo { get; set; }

            public string Token { get; set; }
        }

        public static async ETTask<SignInResult> SignIn(Func<Session> realmSessionFunc, Func<string, Session> gateSessionFunc, C2R_Authentication c2R_SignIn)
        {
            Log.Info("連接Realm");
            Session realmSession = realmSessionFunc?.Invoke();

            R2C_Authentication r2CLogin = (R2C_Authentication)await realmSession.Call(c2R_SignIn);

            if (r2CLogin.Error != ErrorCode.ERR_Success)
            {
                if (r2CLogin.Error == ErrorCode.ERR_AccountDoesntExist)
                {
                    Log.Info("登錄失敗-帳號不存在");
                }
                else if (r2CLogin.Error == ErrorCode.ERR_PasswordIncorrect)
                {
                    Log.Info("登錄失敗-密碼不正確");
                }
                return new SignInResult
                {
                    Message = r2CLogin.Message,
                    Error = r2CLogin.Error
                };
            }
            else
            {
                return await OnSignInSuccessAsync(realmSessionFunc, gateSessionFunc, r2CLogin);
            }
        }

        private static async ETTask<SignInResult> OnSignInSuccessAsync(Func<Session> realmSessionFunc, Func<string, Session> gateSessionFunc, R2C_Authentication r2CLogin)
        {
            Log.Info("連接Gate");
            Session gateSession = gateSessionFunc?.Invoke(r2CLogin.Address);

            G2C_LoginGate g2CLoginGate = (G2C_LoginGate)await gateSession.Call(new C2G_LoginGate()
            {
                Key = r2CLogin.Key
            });

            return new SignInResult
            {
                Message = g2CLoginGate.Message,
                Error = g2CLoginGate.Error,
                playerBaseInfo = r2CLogin.Data,
                Token = r2CLogin.Token
            };
        }

    }
}