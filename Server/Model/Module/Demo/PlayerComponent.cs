using System.Collections.Generic;

namespace ETModel
{
	public class PlayerComponent : Component
	{
        /// <summary>
        /// Player�֨��P�B�ե�(�֦���)
        /// </summary>
        public RedisEventSolverComponent MemorySync { get; set; }

        public Player GetByUid(long uid)
        {
            var gamer = MemorySync.Get<Player>(uid);
            return gamer;
        }
    }
}