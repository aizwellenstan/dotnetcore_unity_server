namespace ETModel
{
	public class SessionPlayerComponent : Component
	{
		public Player Player;

        /// <summary>
        /// 用來判斷Session是新的或舊的
        /// </summary>
        public long gateSessionActorId;

        public bool isAlive => Player.gateSessionActorId == gateSessionActorId;
    }
}