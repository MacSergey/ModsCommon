using ColossalFramework.UI;
using ModsCommon.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ModsCommon.UI
{
    public class UISegmented<ValueType> : UIPanel, IUISelector<ValueType>
    {
        public event Action<ValueType> OnSelectObjectChanged;

        public Func<ValueType, ValueType, bool> IsEqualDelegate { get; set; }
        List<ValueType> Objects { get; } = new List<ValueType>();
        List<UIButton> Buttons { get; } = new List<UIButton>();
        protected virtual int TextPadding => 8;

        int _selectedIndex = -1;
        int SelectedIndex
        {
            get => _selectedIndex;
            set
            {
                if (_selectedIndex == value)
                    return;

                if (_selectedIndex != -1)
                    SetSprite(Buttons[_selectedIndex], false);

                _selectedIndex = value;

                if (_selectedIndex != -1)
                {
                    SetSprite(Buttons[_selectedIndex], true);
                    OnSelectObjectChanged?.Invoke(SelectedObject);
                }
            }
        }

        public ValueType SelectedObject
        {
            get => SelectedIndex >= 0 ? Objects[SelectedIndex] : default;
            set => SelectedIndex = Objects.FindIndex(o => IsEqualDelegate?.Invoke(o, value) ?? ReferenceEquals(o, value) || o.Equals(value));
        }

        public UISegmented()
        {
            autoLayout = true;
            autoLayoutDirection = LayoutDirection.Horizontal;
            autoFitChildrenHorizontally = true;
            autoFitChildrenVertically = true;
        }

        public void AddItem(ValueType item, string label = null)
        {
            Objects.Add(item);

            var button = AddUIComponent<UIButton>();

            button.atlas = TextureHelper.CommonAtlas;
            button.text = label ?? item.ToString();
            button.textScale = 0.8f;
            button.textPadding = new RectOffset(TextPadding, TextPadding, 4, 0);
            button.autoSize = true;
            button.autoSize = false;
            button.height = 20;
            button.eventClick += ButtonClick;

            var last = Buttons.LastOrDefault();
            Buttons.Add(button);

            SetSprite(button, false);
            if (last != null)
                SetSprite(last, SelectedIndex == Buttons.Count - 2);
        }
        private void SetSprite(UIButton button, bool isSelect)
        {
            var index = Buttons.IndexOf(button);
            var suffix = Suffix(index);

            if (isSelect)
            {
                button.normalBgSprite = button.hoveredBgSprite = button.pressedBgSprite = button.disabledBgSprite = $"{TextureHelper.FieldFocused}{suffix}";
                button.disabledColor = new Color32(192, 192, 192, 255);
            }
            else
            {
                button.normalBgSprite = $"{TextureHelper.FieldNormal}{suffix}";
                button.hoveredBgSprite = button.pressedBgSprite = $"{TextureHelper.FieldHovered}{suffix}";
                button.disabledBgSprite = $"{TextureHelper.FieldDisabled}{suffix}";
                button.disabledColor = Color.white;
            }
        }

        private string Suffix(int index)
        {
            if (index == 0)
                return Buttons.Count == 1 ? string.Empty : "Left";
            else
                return index == Buttons.Count - 1 ? "Right" : "Middle";
        }

        private void ButtonClick(UIComponent component, UIMouseEventParameter eventParam) => SelectedIndex = Buttons.FindIndex(b => b == component);

        public void Clear()
        {
            _selectedIndex = -1;
            Objects.Clear();

            foreach (var button in Buttons)
            {
                RemoveUIComponent(button);
                Destroy(button);
            }

            Buttons.Clear();
        }

        public void SetDefaultStyle(Vector2? size = null) { }
    }
}
