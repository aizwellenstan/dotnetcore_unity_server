using ETModel;
using System.Collections.Generic;

namespace ETHotfix
{
    [Event(EventIdType.MessageTip)]
    public class Event_MessageTip : AEvent<long>
    {
        public readonly HashSet<AppType> appTypes = new HashSet<AppType>
        {
            AppType.Gate, AppType.Lobby, AppType.Map
        };

        public async override void Run(long messageId)
        {
            StartConfig startConfig = Game.Scene.GetComponent<StartConfigComponent>().StartConfig;
            if (!appTypes.Contains(startConfig.AppType))
            {
                Log.Warning($"not supported server type on event: {typeof(Event_MessageTip).Name}!");
                return;
            }

            var proxy = Game.Scene.GetComponent<CacheProxyComponent>();
            var playerSync = proxy.GetMemorySyncSolver<Player>();

            var messageTipSetting = Game.Scene.GetComponent<ConfigComponent>().Get(typeof(MessageTipSetting), messageId) as MessageTipSetting;
            if (messageTipSetting != null)
            {
                foreach (var playerData in playerSync.Data)
                {
                    var uid = playerData.Key;
                    var user = await UserDataHelper.FindOneUser(uid);
                    var tipContent = string.Empty;
                    switch (user.language)
                    {
                        //SystemLanguage.English
                        default:
                        case 10:
                            tipContent = messageTipSetting.en_context;
                            break;
                        //SystemLanguage.ChineseTraditional
                        case 41:
                            tipContent = messageTipSetting.zh_tw_context;
                            break;
                    }
                    GateMessageHelper.BroadcastTarget(new G2C_MessageTip() { TipContent = tipContent }, uid);
                }
            }
        }
    }
}
