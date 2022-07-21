using System;

namespace _007_AuthoritativeServerPrototype.Siber_LocalServerDemo.Scripts
{
    public class CacheTime
    {
        public float Sec { get; }

        public CacheTime()
        {
            var dateTime = DateTime.Now;
            Sec = dateTime.Second;
        }
    }
}