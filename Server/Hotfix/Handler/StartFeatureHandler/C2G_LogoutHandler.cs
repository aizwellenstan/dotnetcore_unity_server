using ETModel;
using System;

namespace ETHotfix
{
    [MessageHandler(AppType.Gate)]
    public class C2G_LogoutHandler : AMRpcHandler<C2G_Logout, G2C_Logout>
    {
        protected override async void Run(Session session, C2G_Logout message, Action<G2C_Logout> reply)
        {
            G2C_Logout response = new G2C_Logout();
            try
            {
                var player = session.GetComponent<SessionPlayerComponent>()?.Player;
                if (player == null)
                {
                    response.Error = ErrorCode.ERR_LogoutFailed;
                    reply(response);
                    return;
                }
                // 對Lobby提出登出流程
                Session lobbySession = SessionHelper.GetSession(player.lobbyAppId);
                G2L_LobbyUnitDestroy g2L_LobbyUnitDestroy = new G2L_LobbyUnitDestroy();
                g2L_LobbyUnitDestroy.Uid = player.uid;
                L2G_LobbyUnitDestroy l2G_LobbyUnitDestroy = (L2G_LobbyUnitDestroy)await lobbySession.Call(g2L_LobbyUnitDestroy);
                if(l2G_LobbyUnitDestroy.Error != ErrorCode.ERR_Success)
                {
                    response.Error = ErrorCode.ERR_LogoutFailed;
                    reply(response);
                    return;
                }
                //Game.Scene.GetComponent<PingComponent>().RemoveSession(session.Id);
                session.RemoveComponent<SessionPlayerComponent>();
                session.RemoveComponent<MailBoxComponent>();
                reply(response);
            }
            catch (Exception e)
            {
                ReplyError(response, e, reply);
            }
        }
    }
}
