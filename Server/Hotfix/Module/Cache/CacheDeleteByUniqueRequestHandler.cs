using System;
using System.Collections.Generic;
using ETModel;

namespace ETHotfix
{
	[MessageHandler(AppType.DB)]
	public class CacheDeleteByUniqueRequestHandler : AMRpcHandler<CacheDeleteByUniqueRequest, CacheDeleteResponse>
	{
		protected override void Run(Session session, CacheDeleteByUniqueRequest message, Action<CacheDeleteResponse> reply)
		{
			RunAsync(session, message, reply).Coroutine();
		}
		
		protected async ETVoid RunAsync(Session session, CacheDeleteByUniqueRequest message, Action<CacheDeleteResponse> reply)
		{
            CacheDeleteResponse response = new CacheDeleteResponse();
			try
			{
                bool isSuccessful = await Game.Scene.GetComponent<CacheComponent>().DeleteByUnique(message.CollectionName, message.UniqueName, message.Json);
                response.IsSuccessful = isSuccessful;

                reply(response);
			}
			catch (Exception e)
			{
				ReplyError(response, e, reply);
			}
		}
	}
}