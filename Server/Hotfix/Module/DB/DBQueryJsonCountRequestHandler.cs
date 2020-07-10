using System;
using System.Collections.Generic;
using ETModel;

namespace ETHotfix
{
	[MessageHandler(AppType.DB)]
	public class DBQueryJsonCountRequestHandler : AMRpcHandler<DBQueryJsonCountRequest, DBQueryJsonCountResponse>
	{
		protected override void Run(Session session, DBQueryJsonCountRequest message, Action<DBQueryJsonCountResponse> reply)
		{
			RunAsync(session, message, reply).Coroutine();
		}
		
		protected async ETVoid RunAsync(Session session, DBQueryJsonCountRequest message, Action<DBQueryJsonCountResponse> reply)
		{
            DBQueryJsonCountResponse response = new DBQueryJsonCountResponse();
			try
			{
				long count = await Game.Scene.GetComponent<DBComponent>().GetCountByJson(message.CollectionName, message.Json);
				response.Count = count;

				reply(response);
			}
			catch (Exception e)
			{
				ReplyError(response, e, reply);
			}
		}
	}
}