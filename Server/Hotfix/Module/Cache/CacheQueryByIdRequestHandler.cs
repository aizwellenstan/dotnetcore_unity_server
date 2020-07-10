using System;
using System.Collections.Generic;
using ETModel;

namespace ETHotfix
{
	[MessageHandler(AppType.DB)]
	public class CacheQueryByIdRequestHandler : AMRpcHandler<CacheQueryByIdRequest, CacheQueryResponse>
	{
		protected override void Run(Session session, CacheQueryByIdRequest message, Action<CacheQueryResponse> reply)
		{
			RunAsync(session, message, reply).Coroutine();
		}
		
		protected async ETTask RunAsync(Session session, CacheQueryByIdRequest message, Action<CacheQueryResponse> reply)
		{
            CacheQueryResponse response = new CacheQueryResponse();
			try
			{
                ComponentWithId component = await Game.Scene.GetComponent<CacheComponent>().QueryById(message.CollectionName, message.Id, message.Fields);
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