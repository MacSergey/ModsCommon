using ColossalFramework.UI;
using ICities;
using UnityEngine;

namespace ModsCommon.Utilities
{
    public static class UIExtension
    {
        public static void SetAvailable(this UIComponent component, bool value)
        {
            component.isEnabled = value;
            component.opacity = value ? 1f : 0.15f;
        }
        public static bool IsHover(this UIComponent component, Vector3 mousePosition) => new Rect(component.absolutePosition, component.size).Contains(mousePosition);
    }
}
