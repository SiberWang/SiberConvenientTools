using System;

namespace LocalServerDemo.Scripts
{
    public class CacheTime
    {
        private static readonly DateTime _st = new DateTime(2022, 7, 22, 12, 0, 0, DateTimeKind.Utc);

        // 精算毫秒等級的時間差 (By demo)
        public static ulong Now()
        {
            TimeSpan universalTime = DateTime.Now.ToUniversalTime() - _st;
            ulong    now           = (ulong)universalTime.TotalMilliseconds;
            return now;
        }
    }
}