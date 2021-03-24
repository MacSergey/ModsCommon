using ColossalFramework.UI;
using ModsCommon.Utilities;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ModsCommon.UI
{
    public abstract class UIDropDown<ValueType> : CustomUIDropDown, IUIOnceSelector<ValueType>
    {
        public event Action<ValueType> OnSelectObjectChanged;

        public Func<ValueType, ValueType, bool> IsEqualDelegate { get; set; }
        private List<ValueType> Objects { get; } = new List<ValueType>();
        public ValueType SelectedObject
        {
            get => selectedIndex >= 0 ? Objects[selectedIndex] : default;
            set => selectedIndex = Objects.FindIndex(o => IsEqualDelegate?.Invoke(o, value) ?? ReferenceEquals(o, value) || o.Equals(value));
        }

        public UIDropDown()
        {
            eventSelectedIndexChanged += IndexChanged;
        }
        protected virtual void IndexChanged(UIComponent component, int value) => OnSelectObjectChanged?.Invoke(SelectedObject);

        public void AddItem(ValueType item, string label = null)
        {
            Objects.Add(item);
            AddItem(label ?? item.ToString());
        }
        public void Clear()
        {
            selectedIndex = -1;
            Objects.Clear();
            items = new string[0];
        }
        protected override void OnMouseWheel(UIMouseEventParameter p) { }
        protected override void OnSizeChanged()
        {
            base.OnSizeChanged();

            listWidth = (int)width;
            if (triggerButton is UIComponent button && button != this)
                button.size = size;
        }

        public void SetDefaultStyle(Vector2? size = null)
        {
            atlas = TextureHelper.CommonAtlas;
            listBackground = TextureHelper.FieldHovered;
            itemHeight = 20;
            itemHover = TextureHelper.FieldNormal;
            itemHighlight = TextureHelper.FieldFocused;
            normalBgSprite = TextureHelper.FieldNormal;
            hoveredBgSprite = TextureHelper.FieldHovered;
            listHeight = 700;
            listPosition = PopupListPosition.Below;
            clampListToScreen = true;
            foregroundSpriteMode = UIForegroundSpriteMode.Stretch;
            popupColor = new Color32(45, 52, 61, 255);
            popupTextColor = new Color32(170, 170, 170, 255);
            textScale = 0.7f;
            textFieldPadding = new RectOffset(8, 0, 6, 0);
            popupColor = Color.white;
            popupTextColor = Color.black;
            verticalAlignment = UIVerticalAlignment.Middle;
            horizontalAlignment = UIHorizontalAlignment.Left;
            itemPadding = new RectOffset(14, 0, 5, 0);

            var button = AddUIComponent<CustomUIButton>();
            button.atlas = TextureHelper.InGameAtlas;
            button.text = string.Empty;
            button.relativePosition = new Vector3(0f, 0f);
            button.textVerticalAlignment = UIVerticalAlignment.Middle;
            button.textHorizontalAlignment = UIHorizontalAlignment.Left;
            button.normalFgSprite = "IconDownArrow";
            button.hoveredFgSprite = "IconDownArrowHovered";
            button.pressedFgSprite = "IconDownArrowPressed";
            button.focusedFgSprite = "IconDownArrow";
            button.disabledFgSprite = "IconDownArrowDisabled";
            button.foregroundSpriteMode = UIForegroundSpriteMode.Scale;
            button.horizontalAlignment = UIHorizontalAlignment.Right;
            button.verticalAlignment = UIVerticalAlignment.Middle;
            button.textScale = 0.8f;

            triggerButton = button;

            this.size = size ?? new Vector2(230, 20);
        }

        public void SetSettingsStyle(Vector2? size = null)
        {
            atlas = TextureHelper.InGameAtlas;
            listBackground = "OptionsDropboxListbox";
            itemHeight = 24;
            itemHover = "ListItemHover";
            itemHighlight = "ListItemHighlight";
            normalBgSprite = "OptionsDropbox";
            hoveredBgSprite = "OptionsDropboxHovered";
            focusedBgSprite = "OptionsDropboxFocused";
            autoListWidth = true;
            listHeight = 700;
            listPosition = PopupListPosition.Below;
            clampListToScreen = false;
            foregroundSpriteMode = UIForegroundSpriteMode.Stretch;
            popupColor = Color.white;
            popupTextColor = new Color32(170, 170, 170, 255);
            textScale = 1f;
            textFieldPadding = new RectOffset(14, 40, 7, 0);
            popupColor = Color.white;
            popupTextColor = new Color32(170, 170, 170, 255);
            verticalAlignment = UIVerticalAlignment.Middle;
            horizontalAlignment = UIHorizontalAlignment.Left;
            itemPadding = new RectOffset(14, 14, 4, 0);
            triggerButton = this;

            this.size = size ?? new Vector2(400, 31);
        }

        public void StopLayout() { }
        public void StartLayout(bool layoutNow = true) { }
    }
}
