using System;
using System.Collections.Generic;
using ETModel;

namespace ETHotfix
{
	[MessageHandler(AppType.DB)]
	public class CacheGetAllIdsRequestHandler : AMRpcHandler<CacheGetAllIdsRequest, CacheGetAllIdsResponse>
	{
		protected override void Run(Session session, CacheGetAllIdsRequest message, Action<CacheGetAllIdsResponse> reply)
		{
			RunAsync(session, message, reply).Coroutine();
		}
		
		protected async ETTask RunAsync(Session session, CacheGetAllIdsRequest message, Action<CacheGetAllIdsResponse> reply)
		{
            CacheGetAllIdsResponse response = new CacheGetAllIdsResponse();
			try
			{
                List<long> list = await Game.Scene.GetComponent<CacheComponent>().GetAllIds(message.CollectionName);
                response.IdList = list;

                reply(response);
			}
			catch (Exception e)
			{
				ReplyError(response, e, reply);
			}
		}
	}
}