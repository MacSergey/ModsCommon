using ColossalFramework.UI;
using ModsCommon.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModsCommon.UI
{
    public class CustomUIPanel : UIPanel
    {
#if NOPERFORMLAYOUT
        public override void PerformLayout()
        {
            ClassesExtension.Timer.Start();
            base.PerformLayout();
            ClassesExtension.Timer.Stop();
        }
#else
        public override void PerformLayout() { }
#endif
    }
    public class CustomUIScrollablePanel : UIScrollablePanel
    {
#if NOPERFORMLAYOUT
        public override void PerformLayout()
        {
            ClassesExtension.Timer.Start();
            base.PerformLayout();
            ClassesExtension.Timer.Stop();
        }
#else
        public override void PerformLayout() { }
#endif
    }
    public class CustomUILabel : UILabel
    {
#if NOPERFORMLAYOUT
        public override void PerformLayout()
        {
            ClassesExtension.Timer.Start();
            base.PerformLayout();
            ClassesExtension.Timer.Stop();
        }
#else
        public override void PerformLayout() { }
#endif
    }
    public class CustomUITextField : UITextField
    {
#if NOPERFORMLAYOUT
        public override void PerformLayout()
        {
            ClassesExtension.Timer.Start();
            base.PerformLayout();
            ClassesExtension.Timer.Stop();
        }
#else
        public override void PerformLayout() { }
#endif
    }
    public class CustomUIButton : UIButton
    {
#if NOPERFORMLAYOUT
        public override void PerformLayout()
        {
            ClassesExtension.Timer.Start();
            base.PerformLayout();
            ClassesExtension.Timer.Stop();
        }
#else
        public override void PerformLayout() { }
#endif
    }
    public class CustomUIDragHandle : UIDragHandle
    {
#if NOPERFORMLAYOUT
        public override void PerformLayout()
        {
            ClassesExtension.Timer.Start();
            base.PerformLayout();
            ClassesExtension.Timer.Stop();
        }
#else
        public override void PerformLayout() { }
#endif
    }
    public class CustomUIDropDown : UIDropDown
    {
#if NOPERFORMLAYOUT
        public override void PerformLayout()
        {
            ClassesExtension.Timer.Start();
            base.PerformLayout();
            ClassesExtension.Timer.Stop();
        }
#else
        public override void PerformLayout() { }
#endif
    }
    public class CustomUIScrollbar : UIScrollbar
    {
#if NOPERFORMLAYOUT
        public override void PerformLayout()
        {
            ClassesExtension.Timer.Start();
            base.PerformLayout();
            ClassesExtension.Timer.Stop();
        }
#else
        public override void PerformLayout() { }
#endif
    }
}
