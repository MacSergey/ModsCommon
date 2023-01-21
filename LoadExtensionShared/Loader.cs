using ColossalFramework.Globalization;
using HarmonyLib;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Xml.Linq;

namespace ModsCommon.Utilities
{
    public abstract class Loader
    {
        public static string GetString(XElement config) => config.ToString(SaveOptions.DisableFormatting);

        public static byte[] Compress(string xml)
        {
            var buffer = Encoding.UTF8.GetBytes(xml);

            using var outStream = new MemoryStream();
            using (var zipStream = new GZipStream(outStream, CompressionMode.Compress))
            {
                zipStream.Write(buffer, 0, buffer.Length);
            }
            var compresed = outStream.ToArray();
            return compresed;
        }

        public static string Decompress(byte[] data)
        {
            using var inStream = new MemoryStream(data);
            using var zipStream = new GZipStream(inStream, CompressionMode.Decompress);
            using var outStream = new MemoryStream();
            byte[] buffer = new byte[1000000];
            int readed;
            while ((readed = zipStream.Read(buffer, 0, buffer.Length)) > 0)
            {
                outStream.Write(buffer, 0, readed);
            }

            var decompressed = outStream.ToArray();
            var xml = Encoding.UTF8.GetString(decompressed);
            return xml;
        }

        public static string GetSaveName()
        {
            var lastSaveField = AccessTools.Field(typeof(SavePanel), "m_LastSaveName");
            var lastSave = lastSaveField.GetValue(null) as string;
            if (string.IsNullOrEmpty(lastSave))
                lastSave = Locale.Get("DEFAULTSAVENAMES", "NewSave");

            return lastSave;
        }
    }
}
