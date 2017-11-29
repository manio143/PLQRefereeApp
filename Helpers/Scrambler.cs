using System;
using System.Collections.Generic;
using System.Linq;

namespace PLQRefereeApp
{
    public static class Scrambler
    {
        private static Random rand = new Random();
        public static IEnumerable<T> Scramble<T>(this IEnumerable<T> seq)
        {
            var arr = seq.ToArray();
            for (int i = 0; i < 2.5 * arr.Length; i++) {
                var first = rand.Next(arr.Length);
                var second = rand.Next(arr.Length);
                var temp = arr[second];
                arr[second] = arr[first];
                arr[first] = temp;
            }
            return arr;
        }
    }
}