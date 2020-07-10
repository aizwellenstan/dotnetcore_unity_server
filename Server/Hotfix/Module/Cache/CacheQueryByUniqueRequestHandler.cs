using System;
using System.Collections.Generic;
using ETModel;

namespace ETHotfix
{
	[MessageHandler(AppType.DB)]
	public class CacheQueryByUniqueRequestHandler : AMRpcHandler<CacheQueryByUniqueRequest, CacheQueryResponse>
	{
		protected override void Run(Session session, CacheQueryByUniqueRequest message, Action<CacheQueryResponse> reply)
		{
			RunAsync(session, message, reply).Coroutine();
		}
		
		protected async ETVoid RunAsync(Session session, CacheQueryByUniqueRequest message, Action<CacheQueryResponse> reply)
		{
            CacheQueryResponse response = new CacheQueryResponse();
			try
			{
                ComponentWithId component = await Game.Scene.GetComponent<CacheComponent>().QueryByUnique(message.CollectionName, message.UniqueName, message.Json);
                response.Component = component;

                reply(response);
			}
			catch (Exception e)
			{
				ReplyError(response, e, reply);
			}
		}
	}
}