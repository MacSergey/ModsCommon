using ColossalFramework.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModsCommon.UI
{
    public interface IAutoLayoutPanel
    {
        public void StopLayout();
        public void StartLayout();
    }
    public abstract class UIAutoLayoutPanel : UIPanel, IAutoLayoutPanel
    {
        protected abstract LayoutDirection LayoutDirection { get; }
        protected abstract bool IsFitChildrenHorizontally { get; }
        protected abstract bool IsFitChildrenVertically { get; }
        public UIAutoLayoutPanel()
        {
            autoLayout = true;
            autoLayoutDirection = LayoutDirection;
            autoFitChildrenHorizontally = IsFitChildrenHorizontally;
            autoFitChildrenVertically = IsFitChildrenVertically;
        }
        public void StopLayout() => autoLayout = false;
        public void StartLayout() => autoLayout = true;
    }
}
