using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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
        public static Vector3 AverageOrDefault<TSource>(this IEnumerable<TSource> source, Func<TSource, Vector3> selector, Vector3 defaultValue) => source.Select(selector).AverageOrDefault(defaultValue);

        public static float AverageOrDefault(this IEnumerable<float> source, float defaultValue) => source.AverageOrDefault(0f, PlusFloat, DivFloat, defaultValue);
        public static Vector3 AverageOrDefault(this IEnumerable<Vector3> source, Vector3 defaultValue) => source.AverageOrDefault(Vector3.zero, VectorPlus, VectorDiv, defaultValue);

        private delegate TSource Plus<TSource>(TSource x, TSource y);
        private delegate TSource Div<TSource>(TSource x, float count);
        private static TSource AverageOrDefault<TSource>(this IEnumerable<TSource> source, TSource startValue, Plus<TSource> plus, Div<TSource> div, TSource defaultValue)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            var sum = startValue;
            var count = 0L;
            foreach (var item in source)
            {
                sum = plus(sum, item);
                count = checked(count + 1);
            }

            return count > 0 ? div(sum, count) : defaultValue;
        }

        private static float PlusFloat(float x, float y) => x + y;
        private static float DivFloat(float x, float count) => x / count;

        private static Plus<Vector3> VectorPlus = (Plus<Vector3>)Delegate.CreateDelegate(typeof(Plus<Vector3>), AccessTools.Method(typeof(Vector3), "op_Addition"));
        private static Div<Vector3> VectorDiv = (Div<Vector3>)Delegate.CreateDelegate(typeof(Div<Vector3>), AccessTools.Method(typeof(Vector3), "op_Division"));
    }
}
