using ETModel;
using System;
using System.Collections.Generic;
using MongoDB.Bson;
using System.Linq;

namespace ETHotfix
{
    [Event(EventIdType.ShowProfiler)]
    public class Event_ShowProfiler : AEvent<int>
    {
        public override void Run(int milisec)
        {
            var profileComponent = Game.Scene.GetComponent<ProfileComponent>();
            if (profileComponent != null)
            {
                profileComponent.ShowMessage(milisec);
            }
            var benchmarkComponent = Game.Scene.GetComponent<BenchmarkComponent>();
            if (benchmarkComponent != null)
            {
                benchmarkComponent.isOnProfiler = true;
            }
        }
    }

    [Event(EventIdType.HideProfiler)]
    public class Event_HideProfiler : AEvent
    {
        public override void Run()
        {
            var profileComponent = Game.Scene.GetComponent<ProfileComponent>();
            if (profileComponent != null)
            {
                profileComponent.HideMessage();
            }
            var benchmarkComponent = Game.Scene.GetComponent<BenchmarkComponent>();
            if (benchmarkComponent != null)
            {
                benchmarkComponent.isOnProfiler = false;
            }
        }
    }
}
