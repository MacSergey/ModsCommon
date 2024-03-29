﻿using System.Reflection;

namespace ModsCommon.Utilities
{
    public static class ResourceUtility
    {
        public static byte[] LoadResource(string name)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var manifestResourceStream = assembly.GetManifestResourceStream(name);
            var data = new byte[manifestResourceStream.Length];
            manifestResourceStream.Read(data, 0, data.Length);
            return data;
        }
    }
}
