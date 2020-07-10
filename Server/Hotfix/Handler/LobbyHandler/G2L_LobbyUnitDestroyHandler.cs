using System;
using ETModel;

namespace ETHotfix
{
	[MessageHandler(AppType.Lobby)]
	public class G2L_LobbyUnitDestroyHandler : AMRpcHandler<G2L_LobbyUnitDestroy, L2G_LobbyUnitDestroy>
	{
		protected override void Run(Session session, G2L_LobbyUnitDestroy message, Action<L2G_LobbyUnitDestroy> reply)
		{
			RunAsync(session, message, reply).Coroutine();
		}
		
		protected async ETTask RunAsync(Session session, G2L_LobbyUnitDestroy message, Action<L2G_LobbyUnitDestroy> reply)
		{
			L2G_LobbyUnitDestroy response = new L2G_LobbyUnitDestroy();
			try
			{
                long uid = message.Uid;
				// 刪除Player單元
				var playerComponent = Game.Scene.GetComponent<PlayerComponent>();
				var player = playerComponent.GetByUid(uid);
				if(player != null)
				{
					await playerComponent.Remove(uid);
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