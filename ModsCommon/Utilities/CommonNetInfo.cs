using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModsCommon.Utilities
{
    public interface IMarkingNetInfo
    {
        IEnumerable<float> MarkupPoints { get; }
    }
}
