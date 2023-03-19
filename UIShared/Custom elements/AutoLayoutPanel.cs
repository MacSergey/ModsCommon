using System;
using UnityEngine;

namespace ModsCommon.UI
{
    public interface IAutoLayoutPanel
    {
        public Vector2 ItemSize { get; }
        public RectOffset LayoutPadding { get; }

        public bool IsLayoutSuspended { get; }
        public void StopLayout();
        public void StartLayout(bool layoutNow = true, bool force = false);
        public void PauseLayout(Action action, bool layoutNow = true, bool force = false);
    }
    public enum AutoLayout
    {
        Disabled,
        Horizontal,
        Vertical
    }
}
