using ETModel;
using System;
using System.Collections.Generic;

namespace ETHotfix
{
	[ActorMessageHandler(AppType.Map)]
	public class C2M_TeamGoBattleProgressHandler : AMActorLocationHandler<MapUnit, C2M_TeamGoBattleProgress>
    {
        protected override void Run(MapUnit mapUnit, C2M_TeamGoBattleProgress message)
        {
            RunAsync(mapUnit, message);
        }

        protected void RunAsync(MapUnit mapUnit, C2M_TeamGoBattleProgress message)
        {
            try
            {
                if (mapUnit.Room == null)
                    return;

                if (mapUnit.Room.Type != RoomType.Team)
                    return;

                var roomTeamComponent = mapUnit.Room.GetComponent<RoomTeamComponent>();
                if (roomTeamComponent == null)
                    return;

                //設置目前進度
                message.Progress = Math.Clamp(message.Progress, 0, 1);
                roomTeamComponent.SetProgress(mapUnit.Uid, message.Progress);

                //廣播給所有玩家 更新該玩家進度
                M2C_TeamGoBattleProgressReceiver m2c_TeamGoBattleProgressReceiver = new M2C_TeamGoBattleProgressReceiver();
                m2c_TeamGoBattleProgressReceiver.Uid = mapUnit.Uid;
                m2c_TeamGoBattleProgressReceiver.Progress = message.Progress;
                MapMessageHelper.BroadcastRoom(mapUnit.Room.Id, m2c_TeamGoBattleProgressReceiver);

                //判斷是否全部載入完畢
                if (roomTeamComponent.IsAllLoadingDone())
                {
                    RoomTeamHelper.LoadingFinish(roomTeamComponent);
                }
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }
    }
}