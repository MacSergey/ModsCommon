using ColossalFramework.UI;
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

        public void Ignore(UIComponent item, bool ignore);
    }
    public enum AutoLayout
    {
        Disabled,
        Horizontal,
        Vertical
    }
    public enum AutoLayoutChildren
    {
        None,
        Fit,
        Fill
    }

    [Flags]
    public enum LayoutStart
    {
        Top = 1,
        Middle = 2,
        Bottom = 4,

        Left = 8,
        Centre = 16,
        Right = 32,

        TopLeft = Top | Left,
        TopCentre = Top | Centre,
        TopRight = Top | Right,

        MiddleLeft = Middle | Left,
        MiddleCentre = Middle | Centre,
        MiddleRight = Middle | Right,

        BottomLeft = Bottom | Left,
        BottomCentre = Bottom | Centre,
        BottomRight = Bottom | Right,

        Vertical = Top | Middle | Bottom,
        Horizontal = Left | Centre | Right,
    }

    public static class AutoLayoutExpansion
    {
        public static LayoutStart Correct(this LayoutStart layout)
        {
            if ((layout & LayoutStart.Top) != 0)
                layout &= ~(LayoutStart.Middle | LayoutStart.Bottom);
            else if ((layout & LayoutStart.Bottom) != 0)
                layout &= ~(LayoutStart.Top | LayoutStart.Middle);
            else if ((layout & LayoutStart.Middle) != 0)
                layout &= ~(LayoutStart.Top | LayoutStart.Bottom);
            else
                layout |= LayoutStart.Top;

            if ((layout & LayoutStart.Left) != 0)
                layout &= ~(LayoutStart.Centre | LayoutStart.Right);
            else if ((layout & LayoutStart.Right) != 0)
                layout &= ~(LayoutStart.Left | LayoutStart.Centre);
            else if ((layout & LayoutStart.Centre) != 0)
                layout &= ~(LayoutStart.Left | LayoutStart.Right);
            else
                layout |= LayoutStart.Left;

            return layout;
        }
    }
}
