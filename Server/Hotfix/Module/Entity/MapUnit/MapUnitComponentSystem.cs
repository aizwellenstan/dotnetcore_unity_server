using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;
using ETModel;

namespace ETHotfix
{
    [ObjectSystem]
    public class MapUnitComponentAwakeSystem : AwakeSystem<MapUnitComponent>
    {
        public override void Awake(MapUnitComponent self)
        {
            
        }
    }

    public static class MapUnitComponentSystem
    {
        
    }
}