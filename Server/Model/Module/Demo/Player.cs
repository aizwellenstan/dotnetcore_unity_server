using ETHotfix;
using MongoDB.Bson.Serialization.Attributes;

namespace ETModel
{
    public sealed partial class Player : Entity
	{
        [CacheIgnore]
        public Room Room { get; set; }

        [CacheIgnore]
        public long uid => Id;

        [CacheIgnore]
        public bool isOnDisconnectingStage { get; set; }

        public void Awake()
        {
            if (uid <= 0)
            {
                Log.Error($"uid:{uid} 不可為0");
            }

            isOnDisconnectingStage = false;
        }

        public override void Dispose()
		{
			if (this.IsDisposed)
			{
				return;
			}

			base.Dispose();
        }

        public void EnterRoom(long mapUnitId, Room room)
        {
            this.mapUnitId = mapUnitId;
            this.roomID = room.Id;
            this.mapAppId = IdGenerater.GetAppId(room.Id);
            Room = room;
            SetClientState(PlayerStateData.Types.StateType.EnterRoom);
        }

        public void StartRoom()
        {
            SetClientState(PlayerStateData.Types.StateType.StartRoom);
        }

        public void LeaveRoom()
        {
            mapUnitId = 0;
            roomID = 0;
            mapAppId = 0;
            Room = null;
            SetClientState(PlayerStateData.Types.StateType.Lobby);
        }

        public bool InRoom()
        {
            return mapUnitId != 0;
        }

        #region PlayerStateData

        public void SetClientState(PlayerStateData.Types.StateType stateType, params ulong[] parameters)
        {
            playerStateData.Type = stateType;
            playerStateData.Parameters.Clear();
            playerStateData.Parameters.AddRange(parameters);
        }

        #endregion
    }
}