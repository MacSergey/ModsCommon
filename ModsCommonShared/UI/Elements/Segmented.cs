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

            button.atlas = CommonTextures.Atlas;
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
                button.normalBgSprite = button.hoveredBgSprite = button.pressedBgSprite = button.disabledBgSprite = $"{CommonTextures.FieldFocused}{suffix}";
                button.disabledColor = new Color32(192, 192, 192, 255);
            }
            else
            {
                button.normalBgSprite = $"{CommonTextures.FieldNormal}{suffix}";
                button.hoveredBgSprite = button.pressedBgSprite = $"{CommonTextures.FieldHovered}{suffix}";
                button.disabledBgSprite = $"{CommonTextures.FieldDisabled}{suffix}";
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

        private int SelectedIndex { get; set; } = -1;
        public ValueType SelectedObject
        {
            get => SelectedIndex >= 0 ? Objects[SelectedIndex] : default;
            set => SetSelected(Objects.FindIndex(o => IsEqualDelegate?.Invoke(o, value) ?? ReferenceEquals(o, value) || o.Equals(value)), false);
        }
        private void SetSelected(int index, bool callEvent = true)
        {
            if (SelectedIndex == index)
                return;

            if (SelectedIndex != -1)
                SetSprite(Buttons[SelectedIndex], false);

            SelectedIndex = index;

            if (SelectedIndex != -1)
            {
                SetSprite(Buttons[SelectedIndex], true);
                if (callEvent)
                    OnSelectObjectChanged?.Invoke(SelectedObject);
            }
        }
        public override void Clear()
        {
            SelectedIndex = -1;
            base.Clear();
        }
        protected override void ButtonClick(UIComponent component, UIMouseEventParameter eventParam) => SetSelected(Buttons.FindIndex(b => b == component));
        protected override bool IsSelect(int index) => SelectedIndex == index;
    }

    public abstract class UIMultySegmented<ValueType> : UISegmented<ValueType>, IUIMultySelector<ValueType>
    {
        public event Action<List<ValueType>> OnSelectObjectsChanged;

        private HashSet<int> SelectedIndices { get; set; } = new HashSet<int>();
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

                SetSelected(selectedIndices, false);
            }
        }
        private void SetSelected(HashSet<int> indices, bool callEvent = true)
        {
            foreach (var index in SelectedIndices)
            {
                if (!indices.Contains(index))
                    SetSprite(Buttons[index], false);
            }

            foreach (var index in indices)
            {
                if (!SelectedIndices.Contains(index))
                    SetSprite(Buttons[index], true);
            }

            SelectedIndices = new HashSet<int>(indices);

            if (callEvent)
                OnSelectObjectsChanged?.Invoke(SelectedObjects);
        }

        public override void Clear()
        {
            SelectedIndices.Clear();
            base.Clear();
        }
        protected override void ButtonClick(UIComponent component, UIMouseEventParameter eventParam)
        {
            var indices = new HashSet<int>(SelectedIndices);
            var index = Buttons.FindIndex(b => b == component);

            if (indices.Contains(index))
                indices.Remove(index);
            else
                indices.Add(index);

            SetSelected(indices);
        }
        protected override bool IsSelect(int index) => SelectedIndices.Contains(index);
    }
}
