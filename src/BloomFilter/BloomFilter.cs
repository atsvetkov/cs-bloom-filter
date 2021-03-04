using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using static System.Math;

namespace BloomFilter
{
    public class BloomFilter
    {
        private static Random Rnd = new Random();

        private readonly BitArray _bits;
        private readonly uint[] _seeds;

        public BloomFilter(int expectedItemsCount, double falsePositiveProbability)
        {
            var bitCount = (int)Ceiling(expectedItemsCount * Log(falsePositiveProbability) / Log(1 / Pow(2, Log(2))));
            _bits = new BitArray(bitCount);

            var seedMap = new HashSet<uint>();
            var hasherCount = (int)Round(bitCount / expectedItemsCount * Log(2));
            while (seedMap.Count < hasherCount)
            {
                seedMap.Add(Rnd.NextUint32());
            }

            _seeds = seedMap.ToArray();
        }

        public void Add(string item)
        {
            using var ms = ToStream(item);
            using var reader = new BinaryReader(ms);
            var indices = new List<int>();
            foreach (var seed in _seeds)
            {
                ms.Position = 0;
                var hash = MurMurHasher.Hash(seed, reader);
                var bitIndex = GetBitIndex(hash);
                indices.Add(bitIndex);
                _bits[bitIndex] = true;
            }
        }

        public bool MightContain(string item)
        {
            using var ms = ToStream(item);
            using var reader = new BinaryReader(ms);
            foreach (var seed in _seeds)
            {
                ms.Position = 0;
                var hash = MurMurHasher.Hash(seed, reader);
                var bitIndex = GetBitIndex(hash);
                if (!_bits[bitIndex])
                {
                    return false;
                }
            }

            return true;
        }

        public int BitCount => _bits.Length;
        public int HasherCount => _seeds.Length;

        private Stream ToStream(string item)
        {
            var ms = new MemoryStream();
            var bw = new BinaryWriter(ms);
            var bytes = Encoding.Default.GetBytes(item);
            bw.Write(bytes);

            return ms;
        }

        private int GetBitIndex(int hash) => (hash % _bits.Length + _bits.Length) % _bits.Length;
    }
}
