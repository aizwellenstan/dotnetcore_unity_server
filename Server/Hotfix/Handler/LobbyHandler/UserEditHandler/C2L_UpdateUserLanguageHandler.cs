using System;
using ETModel;
using MongoDB.Bson;

namespace ETHotfix
{
    [ActorMessageHandler(AppType.Lobby)]
    public class C2L_UpdateUserLanguageHandler : AMActorLocationRpcHandler<Player, C2L_UpdateUserLanguage, L2C_UpdateUserLanguage>
    {
        protected override async ETTask Run(Player player, C2L_UpdateUserLanguage message, Action<L2C_UpdateUserLanguage> reply)
        {
            await RunAsync(player, message, reply);
        }

        private async ETTask RunAsync(Player player, C2L_UpdateUserLanguage message, Action<L2C_UpdateUserLanguage> reply)
        {
            L2C_UpdateUserLanguage response = new L2C_UpdateUserLanguage();
            try
            {
                long uid = player.uid;
                if (uid <= 0)
                {
                    //未被Gate授權的帳戶
                    response.Error = ErrorCode.ERR_ConnectGateKeyError;
                }
                else
                {
                    User user = await UserDataHelper.FindOneUser(uid);
                    if (user == null)
                    {
                        response.Error = ErrorCode.ERR_AccountDoesntExist;
                    }
                    else
                    {
                        var log = new BsonDocument();
                        if(user.language != message.Language)
                        {
                            log["language"] = message.Language;
                            user.language = message.Language;
                        }
                        await UserDataHelper.UpsertUser(user, DBLog.LogType.UpdateUserLanguage, log);
                        response.Error = ErrorCode.ERR_Success;
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
