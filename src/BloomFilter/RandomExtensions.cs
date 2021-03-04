using System;

namespace BloomFilter
{
    public static class RandomExtensions
    {
        public static uint NextUint32(this Random rnd)
        {
            var bytes = new byte[4];
            rnd.NextBytes(bytes);
            return BitConverter.ToUInt32(bytes);
        }
    }
}
