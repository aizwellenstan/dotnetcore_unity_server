using System.Collections.Generic;
using ETHotfix;

namespace ETModel
{
    [ObjectSystem]
    public class RelationshipSystem : AwakeSystem<RelationshipComponent>
    {
        public override void Awake(RelationshipComponent self)
        {
            self.Awake();
        }
    }
}