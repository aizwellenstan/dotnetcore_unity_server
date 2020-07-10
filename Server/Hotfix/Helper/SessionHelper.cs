using ETModel;
using System;
using System.Net;
using System.Threading;

namespace ETHotfix
{
    public static class SessionHelper
    {
        /// <summary>
        /// 根據AppId取得相應服務的Session
        /// </summary>
        /// <param name="appId"></param>
        /// <returns></returns>
        public static Session GetSession(int appId) 
        {
            var startComponent = Game.Scene.GetComponent<StartConfigComponent>();
            var instance = startComponent.Get(appId);
            IPEndPoint instanceAddress = instance.GetComponent<InnerConfig>().IPEndPoint;
            return Game.Scene.GetComponent<NetInnerComponent>().Get(instanceAddress);
        }

        public static Session GetMapSession(int appId)
        {
            return GetSession(appId);
        }

        public static Session GetLobbySession(int appId)
        {
            return GetSession(appId);
        }

        /// <summary>
        /// 隨機選取Lobby伺服器
        /// </summary>
        /// <returns></returns>
        public static int GetLobbyIdRandomly()
        {
            var startComponent = Game.Scene.GetComponent<StartConfigComponent>();
            var lobbyIndex = RandomHelper.RandomNumber(0, startComponent.LobbyConfigs.Count);
            var lobbyInst = startComponent.LobbyConfigs[lobbyIndex];
            return lobbyInst.AppId;
        }

        /// <summary>
        /// 隨機選取Map伺服器
        /// </summary>
        /// <returns></returns>
        public static int GetMapIdRandomly()
        {
            var startComponent = Game.Scene.GetComponent<StartConfigComponent>();
            var mapIndex = RandomHelper.RandomNumber(0, startComponent.MapConfigs.Count);
            var mapInst = startComponent.MapConfigs[mapIndex];
            return mapInst.AppId;
        }

        public static Session GetGateSession(int appId)
        {
            return GetSession(appId);
        }
    }
}
