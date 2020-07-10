
namespace ETModel
{
	[ObjectSystem]
	public class MapUnitGateComponentAwakeSystem : AwakeSystem<MapUnitGateComponent, long>
	{
		public override void Awake(MapUnitGateComponent self, long a)
		{
			self.Awake(a);
		}
	}

	public class MapUnitGateComponent : Component, ISerializeToEntity
	{
        /// <summary>
        /// ���a�P�B���(�D�֦���)
        /// </summary>
        public RedisEventSolverComponent MemorySync { get; set; }

        public long GateSessionActorId;

		public bool IsDisconnect;

		public void Awake(long gateSessionId)
		{
            MemorySync = Game.Scene.GetComponent<CacheProxyComponent>().GetMemorySyncSolver<Player>();
            MemorySync.onUpdate += OnPlayerUpdated;

            this.IsDisconnect = false;
            this.GateSessionActorId = gateSessionId;
		}

        public override void Dispose()
        {
            if (IsDisposed)
            {
                return;
            }

            base.Dispose();

            MemorySync.onUpdate -= OnPlayerUpdated;
            MemorySync = null;
        }

        private void OnPlayerUpdated(long id)
        {
            MapUnit parent = GetParent<MapUnit>();
            Player player = MemorySync.Get<Player>(id);
            if (player != null && parent.Uid == player.uid)
            {
                this.GateSessionActorId = player.gateSessionActorId;
            }
        }
    }
}