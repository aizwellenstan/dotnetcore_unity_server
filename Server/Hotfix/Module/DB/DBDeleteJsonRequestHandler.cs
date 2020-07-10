using System;
using System.Collections.Generic;
using ETModel;

namespace ETHotfix
{
	[MessageHandler(AppType.DB)]
	public class DBDeleteJsonRequestHandler : AMRpcHandler<DBDeleteJsonRequest, DBDeleteJsonResponse>
	{
		protected override void Run(Session session, DBDeleteJsonRequest message, Action<DBDeleteJsonResponse> reply)
		{
			RunAsync(session, message, reply).Coroutine();
		}
		
		protected async ETVoid RunAsync(Session session, DBDeleteJsonRequest message, Action<DBDeleteJsonResponse> reply)
		{
            DBDeleteJsonResponse response = new DBDeleteJsonResponse();
			try
			{
				await Game.Scene.GetComponent<DBComponent>().DeleteJson(message.CollectionName, message.Json);
				reply(response);
			}
			catch (Exception e)
			{
				ReplyError(response, e, reply);
			}
		}
	}
}