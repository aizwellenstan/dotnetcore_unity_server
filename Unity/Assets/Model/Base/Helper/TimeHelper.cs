using System;

namespace ETModel
{
	public static class TimeHelper
	{
		private static readonly long epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).Ticks;

		/// <summary>
		/// 客户端时间(毫秒)
		/// </summary>
		/// <returns></returns>
		public static long ClientNowMilliSeconds()
		{
			return (DateTime.UtcNow.Ticks - epoch) / 10000;
		}

        /// <summary>
        /// 客户端时间(秒)
        /// </summary>
        /// <returns></returns>
        public static long ClientNowSeconds()
		{
			return (DateTime.UtcNow.Ticks - epoch) / 10000000;
		}

        public static long NowAfterTimeMillisecond(long millisecond)
        {
            return ClientNowMilliSeconds() + millisecond;
        }

        public static long NowAfterTimeSeconds(double second)
        {
            return NowAfterTimeMillisecond((long)(second * 1000));
        }
    }
}