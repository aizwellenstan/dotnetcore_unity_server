using System;
using ETModel;
using MongoDB.Bson;

namespace ETHotfix
{
	[MessageHandler(AppType.Lobby)]
	public class G2L_LobbyUnitCreateHandler : AMRpcHandler<G2L_LobbyUnitCreate, L2G_LobbyUnitCreate>
	{
		protected override void Run(Session session, G2L_LobbyUnitCreate message, Action<L2G_LobbyUnitCreate> reply)
		{
			RunAsync(session, message, reply).Coroutine();
		}
		
		protected async ETTask RunAsync(Session session, G2L_LobbyUnitCreate message, Action<L2G_LobbyUnitCreate> reply)
		{
            L2G_LobbyUnitCreate response = new L2G_LobbyUnitCreate();
			try
			{
                long uid = message.Uid;
                // 建立Player單元
                var playerComponent = Game.Scene.GetComponent<PlayerComponent>();
                // 使用分布式進程鎖，防止data racing
                using (await ComponentFactory.Create<LockEvent, long>(uid).Wait())
                {
                    Player player = await Game.Scene.GetComponent<CacheProxyComponent>().QueryById<Player>(uid);
                    if (player == null)
                    {
                        player = ComponentFactory.CreateWithId<Player>(uid);
                        await playerComponent.Add(player);
                        await player.AddComponent<MailBoxComponent>().AddLocation();

                        player.mapAppId = SessionHelper.GetMapIdRandomly();
                    }

                    // 如果User已被登入就踢掉舊的
                    if (player.gateSessionActorId != 0 && player.gateSessionActorId != message.GateSessionId)
                    {
                        NetworkHelper.DisconnectSession(player.uid, NetworkHelper.DisconnectInfo.Multiple_Login);
                    }

                    player.gateSessionActorId = message.GateSessionId;
                    player.gateAppId = message.GateAppId;
                    player.lobbyAppId = message.LobbyAppId;
                    player.SetOnline(true);
                    await playerComponent.Update(player);
                    response.Uid = player.Id;
                    response.Json = player.ToJson();
                    reply(response);
                }
            }
			catch (Exception e)
			{
				ReplyError(response, e, reply);
			}
		}
	}
}