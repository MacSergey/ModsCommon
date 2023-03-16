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
        public static float PropertyScrollTimeout => 3f;
        private static UIView UIRoot { get; set; } = null;

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

        public static CustomUIScrollbar AddScrollbar(this UIComponent parent)
        {
            var scrollbar = parent.AddUIComponent<CustomUIScrollbar>();
            scrollbar.name = "Scrollbar";
            scrollbar.orientation = UIOrientation.Vertical;
            scrollbar.pivot = UIPivotPoint.TopLeft;
            scrollbar.minValue = 0;
            scrollbar.value = 0;
            scrollbar.incrementAmount = 50;
            scrollbar.autoHide = true;
            scrollbar.width = 12;
            scrollbar.thumbPadding = new RectOffset(2, 2, 2, 2);

            var trackSprite = scrollbar.AddUIComponent<UISlicedSprite>();
            trackSprite.name = "Scrollbar Track";
            trackSprite.atlas = CommonTextures.Atlas;
            trackSprite.relativePosition = Vector2.zero;
            trackSprite.autoSize = true;
            trackSprite.anchor = UIAnchorStyle.All;
            trackSprite.size = trackSprite.parent.size;
            trackSprite.fillDirection = UIFillDirection.Vertical;
            scrollbar.trackObject = trackSprite;

            var thumb = scrollbar.AddUIComponent<UISlicedSprite>();
            thumb.name = "Scrollbar Thumb";
            thumb.relativePosition = Vector2.zero;
            thumb.fillDirection = UIFillDirection.Vertical;
            thumb.width = 8;
            thumb.atlas = CommonTextures.Atlas;
            thumb.spriteName = CommonTextures.FieldSingle;
            thumb.color = ComponentStyle.FieldNormalColor;
            thumb.minimumSize = new Vector2(0f, 20f);
            scrollbar.thumbObject = thumb;

            return scrollbar;
        }
        public static void AddScrollbar(this UIComponent parent, UIScrollablePanel scrollablePanel)
        {
            var scrollbar = parent.AddScrollbar();
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
        [Obsolete]
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
    public struct SpriteSet
    {
        public string normal;
        public string hovered;
        public string pressed;
        public string focused;
        public string disabled;

        public SpriteSet(string normal, string hovered, string pressed, string focused, string disabled)
        {
            this.normal = normal;
            this.hovered = hovered;
            this.pressed = pressed;
            this.focused = focused;
            this.disabled = disabled;
        }
        public SpriteSet(string sprite)
        {
            this.normal = sprite;
            this.hovered = sprite;
            this.pressed = sprite;
            this.focused = sprite;
            this.disabled = sprite;
        }
    }
    public struct ColorSet
    {
        public Color32? normal;
        public Color32? hovered;
        public Color32? pressed;
        public Color32? focused;
        public Color32? disabled;

        public ColorSet(Color32? normal, Color32? hovered, Color32? pressed, Color32? focused, Color32? disabled)
        {
            this.normal = normal;
            this.hovered = hovered;
            this.pressed = pressed;
            this.focused = focused;
            this.disabled = disabled;
        }
        public ColorSet(Color32? color)
        {
            this.normal = color;
            this.hovered = color;
            this.pressed = color;
            this.focused = color;
            this.disabled = color;
        }
    }

    public struct ItemStyle
    {
        public UITextureAtlas backgroundAtlas;
        public UITextureAtlas foregroundAtlas;

        public SpriteSet spritesBg;
        public SpriteSet selectedSpritesBg;
    }
}
