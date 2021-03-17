using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModsCommon.Utilities
{
    public static class EnumExtension
    {
        public static AttrType GetAttr<AttrType, T>(this T value) where T : Enum where AttrType : Attribute
            => typeof(T).GetField(value.ToString()).GetCustomAttributes(typeof(AttrType), false).OfType<AttrType>().FirstOrDefault();
        public static IEnumerable<Type> GetEnumValues<Type>() where Type : Enum => Enum.GetValues(typeof(Type)).OfType<Type>().Where(v => v.IsVisible());
        public static IEnumerable<Type> GetEnumValues<Type>(this Type value) where Type : Enum => GetEnumValues<Type>().Where(v => (value.ToInt() & v.ToInt()) != 0);
        public static Type GetEnum<Type>(this List<Type> values) where Type : Enum => values.Aggregate(0, (r, v) => r & v.ToInt()).ToEnum<Type>();
        public static bool IsVisible<T>(this T value) where T : Enum => value.GetAttr<NotVisibleAttribute, T>() == null;

        public static int ToInt<T>(this T value) where T : Enum => (int)(object)value;
        public static T ToEnum<T>(this int value) where T : Enum => (T)(object)value;
    }

    public class NotVisibleAttribute : Attribute { }
}
