using System.Collections.Generic;
using ETHotfix;

namespace ETModel
{
    public class InviteComponent : Component
	{
        /// <summary>
        /// 邀請資訊同步控制器(擁有者)
        /// </summary>
        public RedisEventSolverComponent MemorySync { get; set; }

        public readonly Dictionary<long, Invite> _idInviteDict = new Dictionary<long, Invite>();

        public readonly Dictionary<long, List<Invite>> _uIdInviteDict = new Dictionary<long, List<Invite>>();

        public Invite GetByInviteId(long id)
		{
			_idInviteDict.TryGetValue(id, out var invite);
			return invite;
		}

        public List<Invite> GetByUid(long uid)
        {
            _uIdInviteDict.TryGetValue(uid, out var inviteList);
            return inviteList;
        }

		public int Count
		{
			get
			{
				return _idInviteDict.Count;
			}
		}

        public void _Create(Invite invite)
        {
            _idInviteDict.Add(invite.Id, invite);
            List<Invite> inviteList = null;
            if (!_uIdInviteDict.TryGetValue(invite.data.ReceiverUid, out inviteList))
            {
                inviteList = new List<Invite>();
                _uIdInviteDict.Add(invite.data.ReceiverUid, inviteList);
            }
            inviteList.Add(invite);
        }

        public bool _Remove(long id)
        {
            if (_idInviteDict.TryGetValue(id, out var invite))
            {
                _idInviteDict.Remove(id);
                if (_uIdInviteDict.TryGetValue(invite.data.ReceiverUid, out var inviteList))
                {
                    inviteList.Remove(invite);
                }
                invite.Dispose();
                return true;
            }
            return false;
        }
    }
}