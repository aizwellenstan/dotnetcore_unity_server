using ETModel;

namespace ETHotfix
{
    [ObjectSystem]
    public class SessionPlayerComponentAwakeSystem : AwakeSystem<SessionPlayerComponent, long>
    {
        public override void Awake(SessionPlayerComponent self, long gateSessionActorId)
        {
            self.gateSessionActorId = gateSessionActorId;
        }
    }

    [ObjectSystem]
	public class SessionPlayerComponentDestroySystem : DestroySystem<SessionPlayerComponent>
	{
		public override async void Destroy(SessionPlayerComponent self)
		{
            //OtherHelper.ShowCallStackMessage();
            if (!self.isAlive)
                return;
			Session lobbySession = SessionHelper.GetSession(self.Player.lobbyAppId);
			G2L_LobbyUnitUpdate g2L_LobbyUnitUpdate = new G2L_LobbyUnitUpdate();
			g2L_LobbyUnitUpdate.Uid = self.Player.uid;
			g2L_LobbyUnitUpdate.IsOnline = false;
			await lobbySession.Call(g2L_LobbyUnitUpdate);

            Game.Scene.GetComponent<PingComponent>().RemoveSession(self.gateSessionActorId);
            //NetworkHelper.DisconnectPlayer(self.Player);
        }
	}
}