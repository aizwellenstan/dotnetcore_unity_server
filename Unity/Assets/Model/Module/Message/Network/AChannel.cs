using System;
using System.IO;
using System.Net;

namespace ETModel
{
	public enum ChannelType
	{
		Connect,
		Accept,
	}

	public abstract class AChannel : ComponentWithId
	{
		public ChannelType ChannelType { get; }

		public AService Service { get; }

		public abstract MemoryStream Stream { get; }

		public int Error { get; set; }

		public IPEndPoint RemoteAddress { get; protected set; }

        private Action<AChannel, int> errorCallback;

        public static long totalConnectedSessionCount { private set; get; } = 0;

        public static long totalSendedSize { private set; get; } = 0;

        public static long totalSendedCount { private set; get; } = 0;

        public static long totalReceiveSize { private set; get; } = 0;

        public static long totalReceiveCount { private set; get; } = 0;

        public event Action<AChannel, int> ErrorCallback
		{
			add
			{
				this.errorCallback += value;
			}
			remove
			{
				this.errorCallback -= value;
			}
		}

		private Action<MemoryStream> readCallback;

		public event Action<MemoryStream> ReadCallback
		{
			add
			{
				this.readCallback += value;
			}
			remove
			{
				this.readCallback -= value;
			}
		}

		protected void OnRead(MemoryStream memoryStream)
		{
			this.readCallback.Invoke(memoryStream);
		}

		protected void OnError(int e)
		{
			this.Error = e;
			this.errorCallback?.Invoke(this, e);
		}

		protected AChannel(AService service, ChannelType channelType)
		{
			this.Id = IdGenerater.GenerateId();
			this.ChannelType = channelType;
			this.Service = service;
		}

		public abstract void Start();

		public abstract void Send(MemoryStream stream);

		public override void Dispose()
		{
			if (this.IsDisposed)
			{
				return;
			}

			base.Dispose();

			this.Service.Remove(this.Id);
		}

        public static void IncreaseTotalSended(long size, long count)
        {
            totalSendedSize += size;
            totalSendedCount += count;
        }

        public static void IncreaseTotalReceiced(long size, long count)
        {
            totalReceiveSize += size;
            totalReceiveCount += count;
        }

        public static void IncreaseConnectedSessionCount()
        {
            totalConnectedSessionCount++;
        }

        public static void DecreaseConnectedSessionCount()
        {
            totalConnectedSessionCount--;
        }
    }
}