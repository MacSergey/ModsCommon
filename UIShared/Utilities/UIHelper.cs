using ColossalFramework.UI;
using ModsCommon.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ModsCommon.UI
{
    public static class UIHelper
    {
        private static UIView UIRoot { get; set; } = null;
        public static CustomUIScrollbar ScrollBar { get; private set; }

        static UIHelper()
        {
            var gameObject = new GameObject(typeof(CustomUIScrollbar).Name);
            GameObject.DontDestroyOnLoad(gameObject);
            ScrollBar = gameObject.AddComponent<CustomUIScrollbar>();
            ScrollBar.orientation = UIOrientation.Vertical;
            ScrollBar.pivot = UIPivotPoint.TopLeft;
            ScrollBar.minValue = 0;
            ScrollBar.value = 0;
            ScrollBar.incrementAmount = 50;
            ScrollBar.autoHide = true;
            ScrollBar.width = 10;

            UISlicedSprite trackSprite = ScrollBar.AddUIComponent<UISlicedSprite>();
            trackSprite.relativePosition = Vector2.zero;
            trackSprite.autoSize = true;
            trackSprite.anchor = UIAnchorStyle.All;
            trackSprite.size = trackSprite.parent.size;
            trackSprite.fillDirection = UIFillDirection.Vertical;
            trackSprite.spriteName = "ScrollbarTrack";
            ScrollBar.trackObject = trackSprite;

            UISlicedSprite thumbSprite = trackSprite.AddUIComponent<UISlicedSprite>();
            thumbSprite.relativePosition = Vector2.zero;
            thumbSprite.fillDirection = UIFillDirection.Vertical;
            thumbSprite.autoSize = true;
            thumbSprite.width = thumbSprite.parent.width;
            thumbSprite.atlas = CommonTextures.Atlas;
            thumbSprite.spriteName = CommonTextures.FieldNormal;
            ScrollBar.thumbObject = thumbSprite;
        }

        private static void FindUIRoot()
        {
            UIRoot = null;
            foreach (UIView uiview in UnityEngine.Object.FindObjectsOfType<UIView>())
            {
                bool flag = uiview.transform.parent == null && uiview.name == "UIView";
                if (flag)
                {
                    UIRoot = uiview;
                    break;
                }
            }
        }
        public static bool FindComponent<T>(string name, out T component, UIComponent parent = null, FindOptions options = FindOptions.None)
            where T : MonoBehaviour
        {
            if (FindComponents<T>(name, parent, options).FirstOrDefault() is T found)
            {
                component = found;
                return true;
            }
            else
            {
                component = null;
                return false;
            }
        }
        public static IEnumerable<T> FindComponents<T>(string name, UIComponent parent = null, FindOptions options = FindOptions.None)
            where T : MonoBehaviour
        {
            if (UIRoot == null)
            {
                FindUIRoot();
                if (UIRoot == null)
                    yield break;
            }
            foreach (T component in UnityEngine.Object.FindObjectsOfType<T>())
            {
                if ((options & FindOptions.NameContains) > FindOptions.None ? component.name.Contains(name) : component.name == name)
                {
                    var transform = (parent ?? (MonoBehaviour)UIRoot).transform;
                    var currentParent = component.transform.parent;
                    while (currentParent != null && currentParent != transform)
                        currentParent = currentParent.parent;

                    if (currentParent != null)
                        yield return component;
                }
            }
        }

        public static IEnumerable<T> GetCompenentsWithName<T>(string name)
            where T : UIComponent
            => GameObject.FindObjectsOfType<T>().Where(c => c.name == name);

        [Flags]
        public enum FindOptions
        {
            None = 0,
            NameContains = 1
        }

        public static void AddScrollbar(this UIComponent parent, UIScrollablePanel scrollablePanel)
        {
            var gameObject = GameObject.Instantiate(ScrollBar.gameObject);
            parent.AttachUIComponent(gameObject);
            var scrollbar = gameObject.GetComponent<CustomUIScrollbar>();

            scrollbar.eventValueChanged += (component, value) => scrollablePanel.scrollPosition = new Vector2(0, value);

            parent.eventMouseWheel += (component, eventParam) =>
            {
                scrollbar.value -= (int)eventParam.wheelDelta * scrollbar.incrementAmount;
            };

            scrollablePanel.eventMouseWheel += (component, eventParam) =>
            {
                scrollbar.value -= (int)eventParam.wheelDelta * scrollbar.incrementAmount;
            };

            scrollablePanel.eventSizeChanged += (component, eventParam) =>
            {
                scrollbar.relativePosition = scrollablePanel.relativePosition + new Vector3(scrollablePanel.width, 0);
                scrollbar.height = scrollablePanel.height;
            };

            scrollablePanel.verticalScrollbar = scrollbar;
        }
        public static void ScrollIntoViewRecursive(this UIScrollablePanel panel, UIComponent component)
        {
            var rect = new Rect(panel.scrollPosition.x + panel.scrollPadding.left, panel.scrollPosition.y + panel.scrollPadding.top, panel.size.x - panel.scrollPadding.horizontal, panel.size.y - panel.scrollPadding.vertical).RoundToInt();

            var relativePosition = Vector3.zero;
            for (var current = component; current != null && current != panel; current = current.parent)
            {
                relativePosition += current.relativePosition;
            }

            var size = component.size;
            var other = new Rect(panel.scrollPosition.x + relativePosition.x, panel.scrollPosition.y + relativePosition.y, size.x, size.y).RoundToInt();
            if (!rect.Intersects(other))
            {
                Vector2 scrollPosition = panel.scrollPosition;
                if (other.xMin < rect.xMin)
                    scrollPosition.x = other.xMin - panel.scrollPadding.left;
                else if (other.xMax > rect.xMax)
                    scrollPosition.x = other.xMax - Mathf.Max(panel.size.x, size.x) + panel.scrollPadding.horizontal;

                if (other.y < rect.y)
                    scrollPosition.y = other.yMin - panel.scrollPadding.top;
                else if (other.yMax > rect.yMax)
                    scrollPosition.y = other.yMax - Mathf.Max(panel.size.y, size.y) + panel.scrollPadding.vertical;

                panel.scrollPosition = scrollPosition;
            }
        }

        public static Color32 ButtonNormal = Color.white;
        public static Color32 ButtonHovered = new Color32(224, 224, 224, 255);
        public static Color32 ButtonPressed = new Color32(192, 192, 192, 255);
        public static Color32 ButtonFocused = new Color32(160, 160, 160, 255);
        public static void SetDefaultStyle(this UIButton button)
        {
            button.atlas = TextureHelper.InGameAtlas;
            button.normalBgSprite = "ButtonWhite";
            button.disabledBgSprite = "ButtonWhite";
            button.hoveredBgSprite = "ButtonWhite";
            button.pressedBgSprite = "ButtonWhite";
            button.color = ButtonNormal;
            button.hoveredColor = ButtonHovered;
            button.pressedColor = ButtonPressed;
            button.disabledColor = ButtonFocused;
            button.textColor = button.hoveredTextColor = button.focusedTextColor = Color.black;
            button.pressedTextColor = button.disabledTextColor = Color.white;
        }
    }
}
