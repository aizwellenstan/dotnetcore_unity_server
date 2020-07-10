using System;

namespace ETModel
{
    public class DateHelper
    {
        public static DateTime TimestampSecondToDateTimeUTC(long second)
        {
            return new DateTime(1970, 1, 1).AddSeconds(second);
        }

        public static DateTime TimestampMillisecondToDateTimeUTC(long milliseconds)
        {
            return new DateTime(1970, 1, 1).AddMilliseconds(milliseconds);
        }

        public static long DateTimeUTCToTimestampMillisecond(DateTime dateTime)
        {
            return (long)dateTime.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
        }
    }
}
