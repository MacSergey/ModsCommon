using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModsCommon.Utilities
{
    public class CommonNetInfo : NetInfo
    {
        protected HashSet<float> MarkingPointsRow {get; set; } = new HashSet<float>();
        public float[] MarkupPoints => MarkingPointsRow.OrderBy(p => p).ToArray();
        public bool HasMarkingPoints => MarkingPointsRow.Any();
    }
}
