using System;
using ETModel;

namespace ETHotfix
{
    [ActorMessageHandler(AppType.Lobby)]
    public class C2L_AnnouncementHandler : AMActorLocationRpcHandler<Player, C2L_Announcement, L2C_Announcement>
    {
        protected override async ETTask Run(Player player, C2L_Announcement message, Action<L2C_Announcement> reply)
        {
            await ETTask.CompletedTask;
            RunAsync(player, message, reply).Coroutine();
        }

        private async ETVoid RunAsync(Player player, C2L_Announcement message, Action<L2C_Announcement> reply)
        {
            L2C_Announcement response = new L2C_Announcement();
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
                        ConfigComponent configComponent = Game.Scene.GetComponent<ConfigComponent>();
                        IConfig[] announcementSetting = configComponent.GetAll(typeof(AnnouncementSetting));
                        for (int i = 0; i < announcementSetting.Length; i++)
                        {
                            AnnouncementSetting setting = announcementSetting[i] as AnnouncementSetting;
                            if (setting == null)
                                continue;

                            var announcementInfo = new AnnouncementInfo()
                            {
                                Timestamp = setting.timestamp
                            };
                       
                            switch (user.language)
                            {
                                case 41:// zh_tw = 41;
                                    announcementInfo.Title = setting.zh_tw_title;
                                    announcementInfo.Context = setting.zh_tw_context;
                                    break;
                                case 40:// zh_cn = 40;
                                    announcementInfo.Title = setting.zh_cn_title;
                                    announcementInfo.Context = setting.zh_cn_context;
                                    break;
                                case 10:// en = 10;
                                    announcementInfo.Title = setting.en_title;
                                    announcementInfo.Context = setting.en_context;
                                    break;
                            }
                            response.AnnouncementInfos.Add(announcementInfo);
                        }
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
