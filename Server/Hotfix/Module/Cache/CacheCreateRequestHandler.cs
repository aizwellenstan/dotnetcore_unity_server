using System;
using System.Collections.Generic;
using ETModel;

namespace ETHotfix
{
	[MessageHandler(AppType.DB)]
	public class CacheCreateRequestHandler : AMRpcHandler<CacheCreateRequest, CacheQueryResponse>
	{
		protected override void Run(Session session, CacheCreateRequest message, Action<CacheQueryResponse> reply)
		{
			RunAsync(session, message, reply).Coroutine();
		}
		
		protected async ETVoid RunAsync(Session session, CacheCreateRequest message, Action<CacheQueryResponse> reply)
		{
            CacheQueryResponse response = new CacheQueryResponse();
			try
			{
                ComponentWithId component = await Game.Scene.GetComponent<CacheComponent>().Create(message.CollectionName, message.Component);
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