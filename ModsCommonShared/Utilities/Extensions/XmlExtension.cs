using System;
using System.ComponentModel;
using System.IO;
using System.Xml;
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
        public static void AddAttr(this XElement element, string name, object value) => element.Add(new XAttribute(name, value));

        public static XDocument Load(string file, LoadOptions options = LoadOptions.None)
        {
            using FileStream input = new FileStream(file, FileMode.Open);
            XmlReaderSettings xmlReaderSettings = GetXmlReaderSettings(options);
            using XmlReader reader = XmlReader.Create(input, xmlReaderSettings);
            return XDocument.Load(reader, options);
        }
        public static XElement Parse(string text, LoadOptions options = LoadOptions.None)
        {
            using StringReader input = new StringReader(text);
            XmlReaderSettings xmlReaderSettings = GetXmlReaderSettings(options);
            using XmlReader reader = XmlReader.Create(input, xmlReaderSettings);
            return XElement.Load(reader, options);
        }

        private static XmlReaderSettings GetXmlReaderSettings(LoadOptions o)
        {
            XmlReaderSettings xmlReaderSettings = new XmlReaderSettings();
            if ((o & LoadOptions.PreserveWhitespace) == 0)
            {
                xmlReaderSettings.IgnoreWhitespace = true;
            }
            xmlReaderSettings.ProhibitDtd = false;
            xmlReaderSettings.XmlResolver = null;
            return xmlReaderSettings;
        }
    }

    public interface IXml
    {
        string XmlSection { get; }
    }
    public interface IToXml : IXml
    {
        XElement ToXml();
    }
    public interface IFromXml : IXml
    {
        void FromXml(XElement config);
    }
}
