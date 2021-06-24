using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

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
        public static string Description<T, TypeMod>(this T value)
            where T : Enum
            where TypeMod : BaseMod<TypeMod>
        {
            var description = value.GetAttr<DescriptionAttribute, T>()?.Description ?? value.ToString();
            return SingletonMod<TypeMod>.Instance.GetLocalizeString(description);
        }

        public static bool IsVisible<T>(this T value) where T : Enum => value.GetAttr<NotVisibleAttribute, T>() == null;
        public static bool IsItem<T>(this T value) where T : Enum => value.GetAttr<NotItemAttribute, T>() == null;

        public static int ToInt<T>(this T value) where T : Enum => (int)(object)value;
        public static T ToEnum<T>(this int value) where T : Enum => (T)(object)value;
        public static long ToLong<T>(this T value) where T : Enum => (long)(object)value;
        public static T ToEnum<T>(this long value) where T : Enum => (T)(object)value;
        public static ulong ToULong<T>(this T value) where T : Enum => (ulong)(object)value;
        public static T ToEnum<T>(this ulong value) where T : Enum => (T)(object)value;

        public static ToT ToEnum<ToT, FromT>(this FromT item) where ToT : Enum where FromT : Enum => (ToT)(object)item;

        public static bool IsSet<T>(this T flags, T flag) where T : Enum => (flags.ToInt() & flag.ToInt()) == flag.ToInt();

        public static bool CheckFlags<T>(this T value, T required, T forbidden)
            where T : Enum, IConvertible
        {
            return (value.ToInt() & (required.ToInt() | forbidden.ToInt())) == required.ToInt();
        }
        public static bool CheckFlags(this NetNode.Flags value, NetNode.Flags required, NetNode.Flags forbidden = 0) => (value & (required | forbidden)) == required;
        public static bool CheckFlags(this NetSegment.Flags value, NetSegment.Flags required, NetSegment.Flags forbidden = 0) => (value & (required | forbidden)) == required;
    }

    public class NotVisibleAttribute : Attribute { }
    public class NotItemAttribute : Attribute { }
}
