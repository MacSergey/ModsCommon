using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

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
    }
}
