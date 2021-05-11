using System;
using System.Collections.Generic;
using System.Linq;

namespace ModsCommon.Utilities
{
    public static class ClassesExtension
    {
        public static string Unique(this Guid guid) => guid.ToString().Substring(0, 8);
        public static void AddRange<T>(this HashSet<T> hashSet, IEnumerable<T> values)
        {
            foreach (var value in values)
                hashSet.Add(value);
        }
        public static HashSet<T> ToHashSet<T>(this IEnumerable<T> values)
        {
            var hashSet = new HashSet<T>();
            hashSet.AddRange(values);
            return hashSet;
        }
        public static float AverageOrDefault<TSource>(this IEnumerable<TSource> source, Func<TSource, float> selector, float defaultValue) => source.Select(selector).AverageOrDefault(defaultValue);
        public static float AverageOrDefault(this IEnumerable<float> source, float defaultValue)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            var sum = 0.0;
            var count = 0L;
            foreach (var item in source)
            {
                sum += item;
                count = checked(count + 1);
            }

            return count > 0 ? (float)(sum / count) : defaultValue;
        }
    }
}
