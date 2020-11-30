using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModsCommon.Utilities
{
    public class CommonNetInfo : NetInfo
    {
        protected HashSet<float> MarkingPointsRow {get; private set; } = new HashSet<float>();
        protected void Clear() => MarkingPointsRow.Clear();
        public float[] MarkupPoints => MarkingPointsRow.OrderBy(p => p).ToArray();
        public bool HasMarkingPoints => MarkingPointsRow.Any();
    }
}
