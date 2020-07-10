using System;
using System.Collections.Generic;
using System.IO;
using ETModel;

namespace ETHotfix
{
    public static class LanguageComponentEx 
	{
        public static void Awake(this LanguageComponent self)
        {
            self._configComponent = Game.Scene.GetComponent<ConfigComponent>();
        }

        public static string GetString(this LanguageComponent self, int type, long id)
        {
            var languageSettingServer = self._configComponent.Get(typeof(LanguageSetting_Server), id) as LanguageSetting_Server;
            if (languageSettingServer != null)
            {
                switch (type)
                {
                    //SystemLanguage.English
                    case 10:
                        return languageSettingServer.en;
                    //SystemLanguage.ChineseTraditional
                    case 41:
                        return languageSettingServer.zh_tw;
                }
            }
            return id.ToString();
        }
    }
}