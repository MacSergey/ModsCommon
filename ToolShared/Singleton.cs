using ModsCommon.Utilities;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace ModsCommon
{
    public abstract class SingletonTool<T> : SingletonItem<T>
        where T : BaseTool<T>
    {
        public static Shortcut Activation => Instance.Activation;
    }
}
