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
    public class UIAutoLayoutPanel : UIPanel, IAutoLayoutPanel
    {
        public UIAutoLayoutPanel()
        {
            autoLayout = true;
        }
        public void StopLayout() => autoLayout = false;
        public void StartLayout() => autoLayout = true;
    }
    public class UIAutoLayoutScrollablePanel : UIScrollablePanel, IAutoLayoutPanel
    {
        public UIAutoLayoutScrollablePanel()
        {
            autoLayout = true;
        }

        public void StopLayout() => autoLayout = false;
        public void StartLayout() => autoLayout = true;
    }
}
