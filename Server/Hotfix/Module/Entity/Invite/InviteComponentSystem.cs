using ETModel;

namespace ETHotfix
{
    [ObjectSystem]
    public class InviteComponentAwakeSystem : StartSystem<InviteComponent>
    {
        public override void Start(InviteComponent self)
        {
            self.Start();
        }
    }

    [ObjectSystem]
    public class InviteComponentDestroySystem : DestroySystem<InviteComponent>
    {
        public override void Destroy(InviteComponent self)
        {
            self.Destroy();
        }
    }

    public static class InviteComponentEx
    {
        public static void Start(this InviteComponent self)
        {
            var proxy = Game.Scene.GetComponent<CacheProxyComponent>();
            self.MemorySync = proxy.GetMemorySyncSolver<Invite>();
            self.MemorySync.onCreate += self.OnCreated;
            self.MemorySync.onWillDelete += self.OnDeleted;
        }

        public static void Destroy(this InviteComponent self)
        {
            if (self.IsDisposed)
            {
                return;
            }
            self.Dispose();

            foreach (Invite invite in self._idInviteDict.Values)
            {
                invite.Dispose();
            }
            self._idInviteDict.Clear();
            self._uIdInviteDict.Clear();

            self.MemorySync.onCreate -= self.OnCreated;
            self.MemorySync.onWillDelete -= self.OnDeleted;
            // 非擁有者請勿操作Dispose
            self.MemorySync.Dispose();
        }

        private static void OnCreated(this InviteComponent self, long id)
        {
            //if (self.MemorySync.IsMine(id))
            //    return;
            var invite = self.MemorySync.Get<Invite>(id);
            if (invite == null)
            {
                // GG跑到這邊表示有問題
                Log.Error($"Invite[{id}] is missing!");
            }
            else
            {
                self._Create(invite);
            }
        }

        private static void OnDeleted(this InviteComponent self,long id)
        {
            //if (self.MemorySync.IsMine(id))
            //    return;
            var invite = self.MemorySync.Get<Invite>(id);
            if (invite == null)
            {
                // GG跑到這邊表示有問題
                Log.Error($"Invite[{id}] is missing!");
            }
            else
            {
                self._Remove(invite.Id);
            }
        }

        public static async ETTask<Invite> CreateInvite(this InviteComponent self, InviteData inviteData)
        {
            Invite invite = ComponentFactory.CreateWithId<Invite>(IdGenerater.GenerateId());
            invite.SetData(inviteData);
            self._Create(invite);
            await self.MemorySync.Create(invite);
            return invite;
        }

        public static async ETTask DestroyByInviteId(this InviteComponent self, long id)
        {
            if (self._Remove(id))
            {
                await self.MemorySync.Delete<Invite>(id);
            }
        }

        public static async ETTask DestroyByUid(this InviteComponent self, long uid)
        {
            if (self._uIdInviteDict.TryGetValue(uid, out var inviteList))
            {
                for (int i = inviteList.Count - 1; i >= 0; i--)
                {
                    self._idInviteDict.Remove(inviteList[i].Id);
                    inviteList[i].Dispose();
                    inviteList.RemoveAt(i);
                    await self.MemorySync.Delete<Invite>(inviteList[i].Id);
                }
                self._uIdInviteDict.Remove(uid);
            }
        }
    }
}