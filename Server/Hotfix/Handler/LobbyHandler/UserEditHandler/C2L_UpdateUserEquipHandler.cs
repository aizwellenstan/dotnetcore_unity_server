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
    public class C2L_UpdateUserEquipHandler : AMActorLocationRpcHandler<Player, C2L_UpdateUserEquip, L2C_UpdateUserEquip>
    {
        protected override async ETTask Run(Player player, C2L_UpdateUserEquip message, Action<L2C_UpdateUserEquip> reply)
        {
            await RunAsync(player, message, reply);
        }

        private async ETTask RunAsync(Player player, C2L_UpdateUserEquip message, Action<L2C_UpdateUserEquip> reply)
        {
            L2C_UpdateUserEquip response = new L2C_UpdateUserEquip();
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
                        // 蒐集Log資訊(可一致性，但複雜)
                        var log = new BsonDocument();
                        if(user.playerCharSetting.CharacterId != message.CharSetting.CharacterId)
                        {
                            log["characterId"] = message.CharSetting.CharacterId;
                        }
                        if (user.playerCharSetting.BicycleId != message.CharSetting.BicycleId)
                        {
                            log["bicycleId"] = message.CharSetting.BicycleId;
                        }
                        if (user.playerCharSetting.BodyId != message.CharSetting.BodyId)
                        {
                            log["bodyId"] = message.CharSetting.BodyId;
                        }
                        if (user.playerCharSetting.DecorationId != message.CharSetting.DecorationId)
                        {
                            log["decorationId"] = message.CharSetting.DecorationId;
                        }
                        if (user.playerCharSetting.MedalId != message.CharSetting.MedalId)
                        {
                            log["medalId"] = message.CharSetting.MedalId;
                        }
                        user.playerCharSetting = message.CharSetting;
                        await UserDataHelper.UpsertUser(user, DBLog.LogType.UpdateUserCharacterSetting, log);
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
