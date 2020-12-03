using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModsBridge
{
    public interface IMarkingNetInfo
    {
        IEnumerable<float> MarkupPoints { get; }
    }
}
