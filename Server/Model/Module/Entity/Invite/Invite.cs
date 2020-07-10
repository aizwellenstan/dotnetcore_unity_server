using ETHotfix;

namespace ETModel
{
	[ObjectSystem]
	public class InviteEntityAwakeSystem : AwakeSystem<Invite>
	{
		public override void Awake(Invite self)
		{
			self.Awake();
		}
	}

    public sealed partial class Invite : Entity
	{
        public void Awake()
		{

        }

        public void SetData(InviteData data)
        {
            this.data = data;
            this.data.InviteId = Id;
        }

        public override void Dispose()
		{
			if (this.IsDisposed)
			{
				return;
			}

			base.Dispose();
		}
    }
}