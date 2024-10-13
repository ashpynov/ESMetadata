using System;
using System.Collections.Generic;
using System.Linq;

namespace ESMetadata.Extensions
{

    public static class IEnumerableExtension
    {
        public static bool IsNullOrEmpty<T>(this IEnumerable<T> source)
        {
            return !(source?.Count() > 0);
        }

        public static bool IsNullOrSingle<T>(this IEnumerable<T> source)
        {
            return !(source?.Count() > 1);
        }
        public static IEnumerable<T> AllOrDefault<T>(this IEnumerable<T> source, IEnumerable<T> fallback = default)
        {
            return !source.IsNullOrEmpty() ? source : fallback;
        }
        public static int GetUnorderedHashCode<T>(this IEnumerable<T> source)
        {
            List<int> codes = new List<int>();
            foreach (T item in source)
            {
                codes.Add(item.GetHashCode());
            }
            codes.Sort();
            int hash = 0;
            foreach (int code in codes)
            {
                unchecked
                {
                    hash *= 251; // multiply by a prime number
                    hash += code; // add next hash code
                }
            }
            return hash;
        }
    }
};
