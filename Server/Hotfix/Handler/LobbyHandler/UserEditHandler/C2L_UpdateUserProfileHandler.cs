using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Net;
using System.Text;
using ETModel;
using Google.Protobuf.Collections;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;

namespace ETHotfix
{
    [ActorMessageHandler(AppType.Lobby)]
    public class C2L_UpdateUserProfileHandler : AMActorLocationRpcHandler<Player, C2L_UpdateUserProfile, L2C_UpdateUserProfile>
    {
        protected override async ETTask Run(Player player, C2L_UpdateUserProfile message, Action<L2C_UpdateUserProfile> reply)
        {
            await RunAsync(player, message, reply);
        }

        private async ETTask RunAsync(Player player, C2L_UpdateUserProfile message, Action<L2C_UpdateUserProfile> reply)
        {
            L2C_UpdateUserProfile response = new L2C_UpdateUserProfile();
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
                        if(user.name != message.Name)
                        {
                            log["name"] = message.Name;
                            user.name = message.Name;
                        }
                        if (user.gender != message.Sex)
                        {
                            log["gender"] = message.Sex;
                            user.gender = message.Sex;
                        }
                        if (user.location != message.Location)
                        {
                            log["location"] = message.Location;
                            user.location = message.Location;
                        }
                        if (user.height != message.Height)
                        {
                            log["height"] = message.Height;
                            user.height = message.Height;
                        }
                        if (user.weight != message.Weight)
                        {
                            log["weight"] = message.Weight;
                            user.weight = message.Weight;
                        }
                        if (user.birthday != message.Birthday)
                        {
                            log["birthday"] = message.Birthday;
                            user.birthday = message.Birthday;
                        }
                        await UserDataHelper.UpsertUser(user, DBLog.LogType.UpdateUserProfiler, log);
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
