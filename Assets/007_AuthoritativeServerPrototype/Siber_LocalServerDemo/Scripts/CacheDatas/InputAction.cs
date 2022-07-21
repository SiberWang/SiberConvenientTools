using System;

namespace _007_AuthoritativeServerPrototype.Siber_LocalServerDemo.Scripts
{
    public class InputAction : IAction
    {
        public string    ClientID;
        public string    PlayerID;
        public CacheTime CacheTime;
        public float     X;
    }

    public class CacheTime
    {
        private static readonly DateTime time = new DateTime(2022, 7, 21, 0, 0, 0, DateTimeKind.Utc);

        public int milliseconds;

        public CacheTime()
        {
            var t = DateTime.Now.ToUniversalTime() - time;
            milliseconds = (int)t.TotalMilliseconds;
        }

        public int GetTime()
        {
            return milliseconds;
        }
    }
}