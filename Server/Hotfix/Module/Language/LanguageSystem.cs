using ETModel;

namespace ETHotfix
{
    [ObjectSystem]
    public class LanguageAwakeSystem : AwakeSystem<LanguageComponent>
    {
        public override void Awake(LanguageComponent self)
        {
            self.Awake();
        }
    }
}