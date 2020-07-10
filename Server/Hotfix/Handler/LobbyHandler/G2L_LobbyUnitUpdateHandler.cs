using System;
using ETModel;

namespace ETHotfix
{
	[MessageHandler(AppType.Lobby)]
	public class G2L_LobbyUnitUpdateHandler : AMRpcHandler<G2L_LobbyUnitUpdate, L2G_LobbyUnitUpdate>
	{
		protected override void Run(Session session, G2L_LobbyUnitUpdate message, Action<L2G_LobbyUnitUpdate> reply)
		{
			RunAsync(session, message, reply).Coroutine();
		}
		
		protected async ETTask RunAsync(Session session, G2L_LobbyUnitUpdate message, Action<L2G_LobbyUnitUpdate> reply)
		{
            L2G_LobbyUnitUpdate response = new L2G_LobbyUnitUpdate();
			try
			{
                long uid = message.Uid;
				// 更新Player單元
				var playerComponent = Game.Scene.GetComponent<PlayerComponent>();
				var player = playerComponent.GetByUid(uid);
				if(player != null)
				{
                    player.SetOnline(message.IsOnline);
					await playerComponent.Update(player);
					NetworkHelper.DisconnectPlayer(player);
				}
				else
				{
					response.Error = ErrorCode.ERR_LobbyUnitMisiing;
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