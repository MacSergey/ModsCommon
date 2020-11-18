using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModsCommon.Utilities
{
    public static class VersionExtension
    {
        public static Version Build(this Version version) => new Version(version.Major, version.Minor, version.Build);
        public static Version Minor(this Version version) => new Version(version.Major, version.Minor);
        public static Version PrevMinor(this Version version, List<Version> versions)
        {
            var build = version.Build();
            var isMinor = build.IsMinor();
            var toFind = build.Minor();
            var index = versions.FindIndex(v => v == toFind);
            if (index != -1 && versions.Skip(index + 1).FirstOrDefault(v => isMinor || v.IsMinor()) is Version minor)
                return minor;
            else
                return versions.Last();
        }
        public static bool IsMinor(this Version version) => version.Build <= 0 && version.Revision <= 0;
        public static string GetString(this Version version)
        {
            if (version.Revision > 0)
                return version.ToString(4);
            else if (version.Build > 0)
                return version.ToString(3);
            else
                return version.ToString(2);
        }
    }
}
