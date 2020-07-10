using ETModel;
using System;

namespace ETHotfix
{
    [Event(EventIdType.ConfigRoload)]
    public class ConfigRoload : AEvent
    {
        public override void Run()
        {
            Game.Scene.GetComponent<ConfigComponent>().Load();
            Game.EventSystem.Run(EventIdType.SyncAllRoamingRoom);
            Console.WriteLine("ok");
        }
    }

    [Event(EventIdType.ConfigRoload)]
    public class ConfigRoloadOnlyOn : AEvent<Type[]>
    {
        public override void Run(Type[] types)
        {
            for(int i = 0; i < types.Length; i++)
            {
                Game.Scene.GetComponent<ConfigComponent>().ReloadOnly(types[i]);
            }
            Console.WriteLine("ok");
        }
    }
}
