using System;
using System.Collections.Generic;
using System.Linq;

namespace PLQRefereeApp
{
    public static class Scrambler
    {
        private static Random rand = new Random();
        public static IEnumerable<T> Scramble<T>(this IEnumerable<T> seq) where T : class
        {
            var orig = seq.ToArray();
            var arr = new T[orig.Length];
            int remaining = arr.Length - 1;
            while (remaining >= 0)
            {
                var idx = rand.Next(arr.Length);
                if (arr[idx] != null)
                    continue;
                arr[idx] = orig[remaining];
                remaining--;
            }
            return arr;
        }
    }
}