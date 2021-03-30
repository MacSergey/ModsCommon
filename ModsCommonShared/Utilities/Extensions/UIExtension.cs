using ColossalFramework.UI;
using ICities;
using UnityEngine;

namespace ModsCommon.Utilities
{
    public static class UIExtension
    {
        public static UIHelperBase AddGroup(this UIHelperBase helper)
        {
            var newGroup = helper.AddGroup("aaa") as UIHelper;
            var panel = newGroup.self as UIPanel;
            if (panel.parent.Find<UILabel>("Label") is UILabel label)
                label.isVisible = false;
            return newGroup;
        }
        public static void SetAvailable(this UIComponent component, bool value)
        {
            component.isEnabled = value;
            component.opacity = value ? 1f : 0.15f;
        }
        public static bool IsHover(this UIComponent component, Vector3 mousePosition) => new Rect(component.absolutePosition, component.size).Contains(mousePosition);
    }
}
