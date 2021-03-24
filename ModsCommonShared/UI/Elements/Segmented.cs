using ColossalFramework.UI;
using ModsCommon.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ModsCommon.UI
{
    public abstract class UISegmented<ValueType> : UIAutoLayoutPanel
    {
        public Func<ValueType, ValueType, bool> IsEqualDelegate { get; set; }
        protected List<ValueType> Objects { get; } = new List<ValueType>();
        protected List<CustomUIButton> Buttons { get; } = new List<CustomUIButton>();
        protected virtual int TextPadding => 8;

        public UISegmented()
        {
            autoLayoutDirection = LayoutDirection.Horizontal;
            autoFitChildrenHorizontally = true;
            autoFitChildrenVertically = true;
        }

        public void AddItem(ValueType item, string label = null)
        {
            Objects.Add(item);

            var button = AddUIComponent<CustomUIButton>();

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
                SetSprite(last, IsSelect(Buttons.Count - 2));
        }
        protected void SetSprite(CustomUIButton button, bool isSelect)
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

        protected abstract void ButtonClick(UIComponent component, UIMouseEventParameter eventParam);
        protected abstract bool IsSelect(int index);

        public virtual void Clear()
        {
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

    public abstract class UIOnceSegmented<ValueType> : UISegmented<ValueType>, IUIOnceSelector<ValueType>
    {
        public event Action<ValueType> OnSelectObjectChanged;

        private int _selectedIndex = -1;

        private int SelectedIndex
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

        public override void Clear()
        {
            _selectedIndex = -1;
            base.Clear();
        }
        protected override void ButtonClick(UIComponent component, UIMouseEventParameter eventParam) => SelectedIndex = Buttons.FindIndex(b => b == component);
        protected override bool IsSelect(int index) => _selectedIndex == index;
    }

    public abstract class UIMultySegmented<ValueType> : UISegmented<ValueType>, IUIMultySelector<ValueType>
    {
        public event Action<List<ValueType>> OnSelectObjectsChanged;

        private HashSet<int> _selectedIndices = new HashSet<int>();

        private HashSet<int> SelectedIndices
        {
            get => new HashSet<int>(_selectedIndices);
            set
            {
                foreach (var index in _selectedIndices)
                {
                    if (!value.Contains(index))
                        SetSprite(Buttons[index], false);
                }

                foreach (var index in value)
                {
                    if (!_selectedIndices.Contains(index))
                        SetSprite(Buttons[index], true);
                }

                _selectedIndices = new HashSet<int>(value);

                OnSelectObjectsChanged?.Invoke(SelectedObjects);
            }
        }
        public List<ValueType> SelectedObjects
        {
            get => SelectedIndices.Select(i => Objects[i]).ToList();
            set
            {
                var selectedIndices = new HashSet<int>();
                foreach (var item in value)
                {
                    var index = Objects.FindIndex(o => IsEqualDelegate?.Invoke(o, item) ?? ReferenceEquals(o, item) || o.Equals(item));
                    if (index >= 0)
                        selectedIndices.Add(index);
                }

                SelectedIndices = selectedIndices;
            }
        }
        public override void Clear()
        {
            _selectedIndices.Clear();
            base.Clear();
        }
        protected override void ButtonClick(UIComponent component, UIMouseEventParameter eventParam)
        {
            var indices = SelectedIndices;
            var index = Buttons.FindIndex(b => b == component);

            if (indices.Contains(index))
                indices.Remove(index);
            else
                indices.Add(index);

            SelectedIndices = indices;
        }
        protected override bool IsSelect(int index) => _selectedIndices.Contains(index);
    }
}
