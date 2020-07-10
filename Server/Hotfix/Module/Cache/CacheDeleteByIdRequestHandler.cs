using System;
using System.Collections.Generic;
using ETModel;

namespace ETHotfix
{
	[MessageHandler(AppType.DB)]
	public class CacheDeleteByIdRequestHandler : AMRpcHandler<CacheDeleteByIdRequest, CacheDeleteResponse>
	{
		protected override void Run(Session session, CacheDeleteByIdRequest message, Action<CacheDeleteResponse> reply)
		{
			RunAsync(session, message, reply).Coroutine();
		}
		
		protected async ETVoid RunAsync(Session session, CacheDeleteByIdRequest message, Action<CacheDeleteResponse> reply)
		{
            CacheDeleteResponse response = new CacheDeleteResponse();
			try
			{
                bool isSuccessful = await Game.Scene.GetComponent<CacheComponent>().Delete(message.CollectionName, message.Id);
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