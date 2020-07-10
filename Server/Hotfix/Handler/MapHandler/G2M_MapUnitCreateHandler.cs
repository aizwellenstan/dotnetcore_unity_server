using System;
using ETModel;

namespace ETHotfix
{
	[MessageHandler(AppType.Map)]
	public class G2M_MapUnitCreateHandler : AMRpcHandler<G2M_MapUnitCreate, M2G_MapUnitCreate>
	{
		protected override void Run(Session session, G2M_MapUnitCreate message, Action<M2G_MapUnitCreate> reply)
		{
			RunAsync(session, message, reply).Coroutine();
		}
		
		protected async ETVoid RunAsync(Session session, G2M_MapUnitCreate message, Action<M2G_MapUnitCreate> reply)
		{
            M2G_MapUnitCreate response = new M2G_MapUnitCreate();
			try
			{
                //建立MapUnit
                MapUnit mapUnit = ComponentFactory.CreateWithId<MapUnit, MapUnitType>(IdGenerater.GenerateId(), MapUnitType.Hero);
                mapUnit.Uid = message.Uid;
                await mapUnit.AddComponent<MailBoxComponent>().AddLocation();
                mapUnit.AddComponent<MapUnitGateComponent, long>(message.GateSessionId);
                Game.Scene.GetComponent<MapUnitComponent>().Add(mapUnit);

                mapUnit.SetInfo(message.MapUnitInfo);
                await mapUnit.EnterRoom(message.MapUnitInfo.RoomId);
                await Game.Scene.GetComponent<RoomComponent>().Update(mapUnit.Room);

                //回傳MapUnitId給進入者
                response.MapUnitId = mapUnit.Id;
                reply(response);
			}
			catch (Exception e)
			{
				ReplyError(response, e, reply);
			}
		}
	}
}