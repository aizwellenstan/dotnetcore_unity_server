using ETModel;

namespace ETHotfix
{
    [ObjectSystem]
    public class PlayerAwakeSystem : AwakeSystem<Player>
    {
        public override void Awake(Player self)
        {
            self.Awake();
        }
    }

    [ObjectSystem]
    public class PlayerUpdateSystem : UpdateSystem<Player>
    {
        public override void Update(Player self)
        {
            if (!self.isOnDisconnectingStage)
            {
                CheckOnline(self);
            }
        }

        private void CheckOnline(Player self)
        {
            if (!self.isOnline && TimeHelper.ClientNowMilliSeconds() > self.disconnectTime)
            {
                NetworkHelper.DisconnectPlayer(self);
            }
        }
    }

    public static class PlayerSystem
    {
        #region Online/Offline

        public static void SetOnline(this Player self, bool isOnline)
        {
            if (!isOnline && self.isOnline != isOnline)
            {
                self.disconnectTime = TimeHelper.ClientNowMilliSeconds() + 30000;
            }
            self.isOnline = isOnline;
        }

        #endregion
    }
}