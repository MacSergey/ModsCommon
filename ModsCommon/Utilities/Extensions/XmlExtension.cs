using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace ModsCommon.Utilities
{
    public static class XmlExtension
    {
        public static T GetAttrValue<T>(this XElement element, string attrName, T defaultValue = default, Func<T, bool> predicate = null) => Convert(element.Attribute(attrName)?.Value, defaultValue, predicate);
        public static object GetAttrValue(this XElement element, string attrName, Type type) => Convert(element.Attribute(attrName)?.Value, type);
        public static T GetValue<T>(this XElement element, T defaultValue = default, Func<T, bool> predicate = null) => Convert(element.Value, defaultValue, predicate);
        private static T Convert<T>(string str, T defaultValue = default, Func<T, bool> predicate = null)
        {
            try
            {
                var value = Convert(str, typeof(T));
                return value is T tValue && predicate?.Invoke(tValue) != false ? tValue : defaultValue;
            }
            catch
            {
                return defaultValue;
            }
        }
        private static object Convert(string str, Type type)
        {
            try
            {
                if (type == typeof(string))
                    return str;
                else if (string.IsNullOrEmpty(str))
                    return null;
                else
                    return TypeDescriptor.GetConverter(type).ConvertFromString(str);
            }
            catch
            {
                return null;
            }
        }
    }
}
