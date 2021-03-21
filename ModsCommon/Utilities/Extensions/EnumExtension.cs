using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModsCommon.Utilities
{
    public static class EnumExtension
    {
        public static AttrType GetAttr<AttrType, T>(this T value)
            where T : Enum where AttrType : Attribute
        {
            return typeof(T).GetField(value.ToString()).GetCustomAttributes(typeof(AttrType), false).OfType<AttrType>().FirstOrDefault();
        }
        private static Func<T, bool> GetVisibleSelector<T>() where T : Enum => (value) => value.IsVisible();
        public static IEnumerable<T> GetEnumValues<T>(Func<T, bool> selector = null)
            where T : Enum
        {
            return Enum.GetValues(typeof(T)).OfType<T>().Where(selector ?? GetVisibleSelector<T>());
        }
        public static IEnumerable<T> GetEnumValues<T>(this T value) where T : Enum => GetEnumValues<T>().Where(v => (value.ToInt() & v.ToInt()) != 0);
        public static T GetEnum<T>(this List<T> values) where T : Enum => values.Aggregate(0, (r, v) => r | v.ToInt()).ToEnum<T>();
        public static bool IsVisible<T>(this T value) where T : Enum => value.GetAttr<NotVisibleAttribute, T>() == null;
        public static bool IsItem<T>(this T value) where T : Enum => value.GetAttr<NotItemAttribute, T>() == null;

        public static int ToInt<T>(this T value) where T : Enum => (int)(object)value;
        public static T ToEnum<T>(this int value) where T : Enum => (T)(object)value;
    }

    public class NotVisibleAttribute : Attribute { }
    public class NotItemAttribute : Attribute { }
}
