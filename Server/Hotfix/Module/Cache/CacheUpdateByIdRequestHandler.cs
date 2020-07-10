using System;
using System.Collections.Generic;
using ETModel;

namespace ETHotfix
{
	[MessageHandler(AppType.DB)]
	public class CacheUpdateByIdRequestHandler : AMRpcHandler<CacheUpdateByIdRequest, CacheQueryResponse>
	{
		protected override void Run(Session session, CacheUpdateByIdRequest message, Action<CacheQueryResponse> reply)
		{
			RunAsync(session, message, reply).Coroutine();
		}
		
		protected async ETTask RunAsync(Session session, CacheUpdateByIdRequest message, Action<CacheQueryResponse> reply)
		{
            CacheQueryResponse response = new CacheQueryResponse();
			try
			{
                ComponentWithId component = await Game.Scene.GetComponent<CacheComponent>().UpdateById(message.CollectionName, message.Id, message.DataJson);
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