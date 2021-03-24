using ColossalFramework.UI;
using ModsCommon.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ModsCommon.UI
{
    public class CustomUIPanel : UIPanel
    {
#if NOPERFORMLAYOUT
        Vector3 positionBefore;
        public override void ResetLayout() => positionBefore = relativePosition;
        public override void PerformLayout() => relativePosition = positionBefore;
#endif
    }
    public class CustomUIScrollablePanel : UIScrollablePanel
    {
#if NOPERFORMLAYOUT
        Vector3 positionBefore;
        public override void ResetLayout() => positionBefore = relativePosition;
        public override void PerformLayout() => relativePosition = positionBefore;
#endif
    }
    public class CustomUILabel : UILabel
    {
#if NOPERFORMLAYOUT
        Vector3 positionBefore;
        public override void ResetLayout() => positionBefore = relativePosition;
        public override void PerformLayout() => relativePosition = positionBefore;
#endif
    }
    public class CustomUITextField : UITextField
    {
#if NOPERFORMLAYOUT
        Vector3 positionBefore;
        public override void ResetLayout() => positionBefore = relativePosition;
        public override void PerformLayout() => relativePosition = positionBefore;
#endif
    }
    public class CustomUIButton : UIButton
    {
#if NOPERFORMLAYOUT
        Vector3 positionBefore;
        public override void ResetLayout() => positionBefore = relativePosition;
        public override void PerformLayout() => relativePosition = positionBefore;
#endif
    }
    public class CustomUIDragHandle : UIDragHandle
    {
#if NOPERFORMLAYOUT
        Vector3 positionBefore;
        public override void ResetLayout() => positionBefore = relativePosition;
        public override void PerformLayout() => relativePosition = positionBefore;
#endif
    }
    public class CustomUIDropDown : UIDropDown
    {
#if NOPERFORMLAYOUT
        Vector3 positionBefore;
        public override void ResetLayout() => positionBefore = relativePosition;
        public override void PerformLayout() => relativePosition = positionBefore;
#endif
    }
    public class CustomUIScrollbar : UIScrollbar
    {
#if NOPERFORMLAYOUT
        Vector3 positionBefore;
        public override void ResetLayout() => positionBefore = relativePosition;
        public override void PerformLayout() => relativePosition = positionBefore;
#endif
    }
}
