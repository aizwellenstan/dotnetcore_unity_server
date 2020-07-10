using System;
using ETModel;

namespace ETHotfix
{
	[MessageHandler(AppType.Map)]
	public class L2M_MapUnitCreateHandler : AMRpcHandler<L2M_MapUnitCreate, M2L_MapUnitCreate>
	{
		protected override void Run(Session session, L2M_MapUnitCreate message, Action<M2L_MapUnitCreate> reply)
		{
			RunAsync(session, message, reply).Coroutine();
		}
		
		protected async ETTask RunAsync(Session session, L2M_MapUnitCreate message, Action<M2L_MapUnitCreate> reply)
		{
			M2L_MapUnitCreate response = new M2L_MapUnitCreate();
			try
			{
                MapUnitComponent mapUnitComponent = Game.Scene.GetComponent<MapUnitComponent>();
                RoomComponent roomComponent = Game.Scene.GetComponent<RoomComponent>();
                MapUnit mapUnit = mapUnitComponent.GetByUid(message.Uid);
                if(mapUnit != null)
                {
                    mapUnitComponent.Remove(mapUnit.Id);
                }
                // 建立MapUnit
                mapUnit = ComponentFactory.CreateWithId<MapUnit, MapUnitType>(IdGenerater.GenerateId(), MapUnitType.Hero);
                mapUnit.Uid = message.Uid;
                await mapUnit.AddComponent<MailBoxComponent>().AddLocation();
                mapUnit.AddComponent<MapUnitGateComponent, long>(message.GateSessionId);
                mapUnitComponent.Add(mapUnit);

                mapUnit.SetInfo(message.MapUnitInfo);
                await mapUnit.EnterRoom(message.MapUnitInfo.RoomId);
                await roomComponent.Update(mapUnit.Room);

                // 回傳MapUnitId給進入者
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