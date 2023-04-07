using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace ModsCommon.Utilities
{
    public static class EnumExtension
    {
        public static AttrType GetAttr<AttrType, T>(this T value)
            where T : Enum
            where AttrType : Attribute
        {
            return typeof(T).GetField(value.ToString()).GetCustomAttributes(false).OfType<AttrType>().FirstOrDefault();
        }
        private static Func<T, bool> GetVisibleSelector<T>() where T : Enum => (value) => value.IsVisible();

        public static IEnumerable<T> GetEnumValues<T>(Func<T, bool> selector = null)
            where T : Enum
        {
            return Enum.GetValues(typeof(T)).OfType<T>().Where(selector ?? GetVisibleSelector<T>());
        }
        public static IEnumerable<T> Order<T>(this IEnumerable<T> values, bool direct = true)
            where T : Enum
        {
            if (direct)
                return values.OrderBy(v => v.Order());
            else
                return values.OrderByDescending(v => v.Order());
        }

        public static IEnumerable<T> GetEnumValues<T>(this T value, Func<T, bool> selector = null)
            where T : Enum
        {
            var underlyingType = Enum.GetUnderlyingType(typeof(T));

            if (underlyingType == typeof(int))
            {
                var intValue = value.ToInt();
                return GetEnumValues<T>(selector).Where(v => (intValue & v.ToInt()) != 0);
            }
            else if (underlyingType == typeof(long))
            {
                var longValue = value.ToLong();
                return GetEnumValues<T>(selector).Where(v => (longValue & v.ToLong()) != 0);
            }
            else if (underlyingType == typeof(ulong))
            {
                var ulongValue = value.ToULong();
                return GetEnumValues<T>(selector).Where(v => (ulongValue & v.ToULong()) != 0);
            }
            else
                return Enumerable.Empty<T>();
        }

        public static T GetEnum<T>(this IEnumerable<T> values)
            where T : Enum
        {
            var underlyingType = Enum.GetUnderlyingType(typeof(T));

            if (underlyingType == typeof(int))
                return values.Aggregate(0, (r, v) => r | v.ToInt()).ToEnum<T>();
            else if (underlyingType == typeof(long))
                return values.Aggregate(0L, (r, v) => r | v.ToLong()).ToEnum<T>();
            else if (underlyingType == typeof(ulong))
                return values.Aggregate(0UL, (r, v) => r | v.ToULong()).ToEnum<T>();
            else
                return default;
        }
        public static string Description<T, TypeMod>(this T value)
            where T : Enum
            where TypeMod : ICustomMod
        {
            var description = value.GetAttr<DescriptionAttribute, T>()?.Description ?? value.ToString();
            return SingletonMod<TypeMod>.Instance.GetLocalizedString(description);
        }

        public static bool IsVisible<T>(this T value) where T : Enum => value.GetAttr<NotVisibleAttribute, T>() == null;
        public static bool IsItem<T>(this T value) where T : Enum => value.GetAttr<NotItemAttribute, T>() == null;
        public static int Order<T>(this T value) where T : Enum => value.GetAttr<OrderAttribute, T>()?.Order ?? int.MaxValue;
        public static string Sprite<T>(this T value, string tag = null)
            where T : Enum
        {
            var attr = typeof(T).GetField(value.ToString()).GetCustomAttributes(typeof(SpriteAttribute), false).FirstOrDefault(a => (a as SpriteAttribute).Tag == tag) as SpriteAttribute;
            var sprite = attr?.Sprite ?? string.Empty;
            return sprite;
        }

        public static int ToInt<T>(this T value) where T : Enum => (int)(object)value;
        public static T ToEnum<T>(this int value) where T : Enum => (T)(object)value;
        public static long ToLong<T>(this T value) where T : Enum => (long)(object)value;
        public static T ToEnum<T>(this long value) where T : Enum => (T)(object)value;
        public static ulong ToULong<T>(this T value) where T : Enum => (ulong)(object)value;
        public static T ToEnum<T>(this ulong value) where T : Enum => (T)(object)value;

        public static ToT ToEnum<ToT, FromT>(this FromT item) where ToT : Enum where FromT : Enum => (ToT)(object)item;

        public static bool IsSet<T>(this T flags, T flag) where T : Enum => (flags.ToInt() & flag.ToInt()) == flag.ToInt();

        public static bool CheckFlags<T>(this T value, T required, T forbidden)
            where T : Enum
        {
            return (value.ToInt() & (required.ToInt() | forbidden.ToInt())) == required.ToInt();
        }
        public static bool CheckFlags(this NetNode.Flags value, NetNode.Flags required, NetNode.Flags forbidden = 0) => (value & (required | forbidden)) == required;
        public static bool CheckFlags(this NetSegment.Flags value, NetSegment.Flags required, NetSegment.Flags forbidden = 0) => (value & (required | forbidden)) == required;
    }

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class NotVisibleAttribute : Attribute { }


    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class NotItemAttribute : Attribute { }


    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class OrderAttribute : Attribute
    {
        public int Order { get; }
        public OrderAttribute(int order)
        {
            Order = order;
        }
    }


    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class SpriteAttribute : Attribute
    {
        public string Name { get; }
        public string Tag { get; }
        public string Sprite => $"{Name}{Tag}";
        public SpriteAttribute(string name, string tag = null)
        {
            Name = name;
            Tag = tag;
        }
    }
}
