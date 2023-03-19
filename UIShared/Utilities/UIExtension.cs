using ColossalFramework.UI;
using UnityEngine;

namespace ModsCommon.Utilities
{
    public static class UIExtension
    {
        public static bool IsHover(this UIComponent component, Vector3 mousePosition) => new Rect(component.absolutePosition, component.size).Contains(mousePosition);
        public static bool IsHoverAllParents(this UIComponent component, Vector3 mousePosition)
        {
            while (component != null)
            {
                if (component.IsHover(mousePosition))
                    return true;

                component = component.parent;
            }

            return false;
        }

        public static bool StartTop(this LayoutStart layout) => layout == LayoutStart.TopRight || layout == LayoutStart.TopLeft;
        public static bool StartBottom(this LayoutStart layout) => layout == LayoutStart.BottomLeft || layout == LayoutStart.BottomRight;
        public static bool StartRight(this LayoutStart layout) => layout == LayoutStart.TopRight || layout == LayoutStart.BottomRight;
        public static bool StartLeft(this LayoutStart layout) => layout == LayoutStart.TopLeft || layout == LayoutStart.BottomLeft;
    }
}
