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
        {
            return GameObject.FindObjectsOfType<T>().Where(c => c.name == name);
        }

        [Flags]
        public enum FindOptions
        {
            None = 0,
            NameContains = 1
        }

        [Obsolete]
        public static Color32 ButtonNormal => Color.white;
        [Obsolete]
        public static Color32 ButtonHovered => new Color32(224, 224, 224, 255);
        [Obsolete]
        public static Color32 ButtonPressed => new Color32(192, 192, 192, 255);
        [Obsolete]
        public static Color32 ButtonFocused => new Color32(160, 160, 160, 255);
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
