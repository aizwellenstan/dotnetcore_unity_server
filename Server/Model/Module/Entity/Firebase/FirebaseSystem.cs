using System.Collections.Generic;
using ETHotfix;

namespace ETModel
{
    [ObjectSystem]
    public class FirebaseComponentAwakeSystem : AwakeSystem<FirebaseComponent>
    {
        public override void Awake(FirebaseComponent self)
        {
            self.Awake();
        }
    }
}