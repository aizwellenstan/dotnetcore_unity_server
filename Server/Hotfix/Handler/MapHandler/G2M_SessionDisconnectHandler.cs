using ETModel;
using System.Collections.Generic;

namespace ETHotfix
{
	[ActorMessageHandler(AppType.Map)]
	public class G2M_SessionDisconnectHandler : AMActorLocationHandler<MapUnit, G2M_SessionDisconnect>
	{
		protected override void Run(MapUnit mapUnit, G2M_SessionDisconnect message)
		{
            //先Player離開地圖再移除mapUnit
            var proxy = Game.Scene.GetComponent<CacheProxyComponent>();
            var playerSync = proxy.GetMemorySyncSolver<Player>();
            var player = playerSync.Get<Player>(mapUnit.Uid);
            player?.LeaveRoom();

            Room room = mapUnit.Room;
            if (room != null)
            {
                RoomTeamComponent roomTeamComponent = room.GetComponent<RoomTeamComponent>();

                //刪除MapUnit
                M2C_MapUnitDestroy m2C_MapUnitDestroy = new M2C_MapUnitDestroy();
                m2C_MapUnitDestroy.MapUnitId = mapUnit.Id;

                //製作廣播列表
                List<MapUnit> broadcastMapUnits = new List<MapUnit>();
                broadcastMapUnits.AddRange(room.GetAll());
                for (int i = 0; i < broadcastMapUnits.Count; i++)
                {
                    //過濾自己
                    if (broadcastMapUnits[i].Uid == mapUnit.Uid)
                    {
                        broadcastMapUnits.RemoveAt(i);
                        break;
                    }
                }
                MapMessageHelper.BroadcastTarget(m2C_MapUnitDestroy, broadcastMapUnits);


                switch (room.Type)
                {
                    case RoomType.Roaming:
                        {

                        }
                        break;
                    case RoomType.Team:
                        {
                            //廣播成員異動訊息
                            M2C_TeamModifyMember m2c_TeamModifyMember = new M2C_TeamModifyMember();
                            m2c_TeamModifyMember.Uid = mapUnit.Uid;
                            MapMessageHelper.BroadcastTarget(m2c_TeamModifyMember, broadcastMapUnits);

                            //判斷自身是否為隊長
                            bool isLeader = mapUnit.Uid == roomTeamComponent.Data.LeaderUid;

                            //廣播替換隊長訊息
                            if (isLeader)
                            {
                                M2C_TeamModifyData m2c_TeamModifyData = new M2C_TeamModifyData();
                                m2c_TeamModifyData.Data = roomTeamComponent.Data;
                                MapMessageHelper.BroadcastTarget(m2c_TeamModifyData, broadcastMapUnits);
                            }
                        }
                        break;
                }
            }

            //中斷指定玩家與Map的連接
            mapUnit.GetComponent<MapUnitGateComponent>().IsDisconnect = true;
            Game.Scene.GetComponent<MapUnitComponent>().Remove(mapUnit.Id);
        }
	}
}