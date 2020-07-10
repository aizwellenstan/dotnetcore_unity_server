using ETModel;
using System;
using System.Collections.Generic;
using MongoDB.Bson;
using System.Linq;

namespace ETHotfix
{
    [Event(EventIdType.ShowWatcher)]
    public class Event_ShowWatcher : AEvent<List<int>>
    {
        public override void Run(List<int> list)
        {
            var benchmarkComponent = Game.Scene.GetComponent<BenchmarkComponent>();
            if (benchmarkComponent == null)
            {
                Console.WriteLine("benchmarkComponent is null");
                return;
            }
            benchmarkComponent.watchTargetList.AddRange(list);
        }
    }

    [Event(EventIdType.HideWatcher)]
    public class Event_HideWatcher : AEvent
    {
        public override void Run()
        {
            var benchmarkComponent = Game.Scene.GetComponent<BenchmarkComponent>();
            if (benchmarkComponent == null)
            {
                Console.WriteLine("benchmarkComponent is null");
                return;
            }
            benchmarkComponent.watchTargetList.Clear();
        }
    }
}
