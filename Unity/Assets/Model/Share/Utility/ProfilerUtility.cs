using System;

namespace ETModel
{
    public static class ProfilerUtility
    {
        public class NetworkProfiler
        {
            public long sendCount = 0;
            public double sendSize = 0;
            public long recevieCount = 0;
            public double recevieSize = 0;
            public long totalSendCount = 0;
            public long totalSendSize = 0;
            public long totalReceiveCount = 0;
            public long totalReceiveSize = 0;

            public double maxSendSize = 0;
            public double maxReceiveSize = 0;
            public long maxSendCount = 0;
            public long maxReceiveCount = 0;

            public string Show(int fps)
            {
                sendSize = totalSendSize;
                sendCount = totalSendCount;
                recevieSize = totalReceiveSize;
                recevieCount = totalReceiveCount;

                totalSendCount = AChannel.totalSendedCount;
                totalSendSize = AChannel.totalSendedSize;
                totalReceiveCount = AChannel.totalReceiveCount;
                totalReceiveSize = AChannel.totalReceiveSize;

                sendSize = totalSendSize - sendSize;
                sendCount = totalSendCount - sendCount;
                recevieSize = totalReceiveSize - recevieSize;
                recevieCount = totalReceiveCount - recevieCount;
                sendSize /= 1024;
                recevieSize /= 1024;

                maxSendSize = Math.Max(maxSendSize, sendSize);
                maxReceiveSize = Math.Max(maxReceiveSize, recevieSize);
                maxSendCount = Math.Max(maxSendCount, sendCount);
                maxReceiveCount = Math.Max(maxReceiveCount, recevieCount);

                return $"Client count : {AChannel.totalConnectedSessionCount}, Send : {sendSize:F2}KB/{sendCount}, Receive : {recevieSize:F2}KB/{recevieCount}, MaxSend : {maxSendSize:F2}KB/{maxSendCount}, MaxReceive : {maxReceiveSize:F2}KB/{maxReceiveCount}, FPS : {fps}";
            }

            public void Reset()
            {
                maxSendSize = 0;
                maxReceiveSize = 0;
                maxSendCount = 0;
                maxReceiveCount = 0;
            }
        }
    }
}