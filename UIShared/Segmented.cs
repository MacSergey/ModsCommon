using ColossalFramework.UI;
using ModsCommon.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ModsCommon.UI
{
    public readonly struct OptionData
    {
        public readonly string label;
        public readonly UITextureAtlas atlas;
        public readonly string sprite;

        public OptionData(string label = null, UITextureAtlas atlas = null, string sprite = null)
        {
            this.label = label;
            this.atlas = atlas;
            this.sprite = sprite;
        }
    }
    public abstract class UISegmented<ValueType> : UIAutoLayoutPanel, IReusable
    {
        bool IReusable.InCache { get; set; }
        public Func<ValueType, ValueType, bool> IsEqualDelegate { get; set; }
        protected List<ValueType> Objects { get; } = new List<ValueType>();
        protected List<CustomUIButton> Buttons { get; } = new List<CustomUIButton>();
        protected Dictionary<CustomUIButton, bool> Clickable { get; } = new Dictionary<CustomUIButton, bool>();
        protected virtual int TextPadding => AutoButtonSize ? 8 : (int)Mathf.Clamp((buttonWidth - 20f) / 2f, 0, 8);

        private bool autoButtonSize = true;
        private float buttonWidth = 50f;
        public bool AutoButtonSize
        {
            get => autoButtonSize;
            set
            {
                if (value != autoButtonSize)
                {
                    autoButtonSize = value;
                    SetButtonsWidth();
                }
            }
        }
        public float ButtonWidth
        {
            get => buttonWidth;
            set
            {
                if (value != buttonWidth)
                {
                    buttonWidth = value;
                    SetButtonsWidth();
                }
            }
        }

        private float textScale = 0.8f;
        public float TextScale
        {
            get => textScale;
            set
            {
                if (value != textScale)
                {
                    textScale = value;
                    SetButtonsWidth();
                }
            }
        }

        public UISegmented()
        {
            autoLayoutDirection = LayoutDirection.Horizontal;
            autoFitChildrenHorizontally = true;
            autoFitChildrenVertically = true;
        }

        public void AddItem(ValueType item, OptionData optionData) => AddItem(item, optionData, true, null);
        public void AddItem(ValueType item, OptionData optionData, bool clickable = true, float? width = null)
        {
            Objects.Add(item);

            var button = AddUIComponent<MultyAtlasUIButton>();
            button.atlas = CommonTextures.Atlas;
            button.textHorizontalAlignment = UIHorizontalAlignment.Center;

            if (optionData.atlas != null && !string.IsNullOrEmpty(optionData.sprite))
            {
                button.atlasForeground = optionData.atlas;
                button.normalFgSprite = optionData.sprite;
                button.tooltip = optionData.label ?? item.ToString();
                button.foregroundSpriteMode = UIForegroundSpriteMode.Scale;
            }
            else
                button.text = optionData.label ?? item.ToString();

            UpdateButton(button, width);
            if (clickable)
                button.eventClick += ButtonClick;

            var last = Buttons.LastOrDefault();
            Buttons.Add(button);
            Clickable.Add(button, clickable);

            SetSprite(button, false);
            if (last != null)
                SetSprite(last, IsSelect(Buttons.Count - 2));

        }
        private void SetButtonsWidth()
        {
            StopLayout();

            foreach (var button in Buttons)
                UpdateButton(button);

            StartLayout();
        }
        private void UpdateButton(CustomUIButton button = null, float? width = null)
        {
            button.textPadding = new RectOffset(TextPadding, TextPadding, 4, 0);
            button.textScale = TextScale;

            if (AutoButtonSize)
            {
                button.autoSize = true;
                button.autoSize = false;
            }
            else if (width.HasValue)
                button.width = width.Value;
            else
                button.width = ButtonWidth;

            button.height = 20;
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
            else if (!Clickable[button])
            {
                button.normalBgSprite = button.hoveredBgSprite = button.pressedBgSprite = button.disabledBgSprite = $"{CommonTextures.FieldDisabled}{suffix}";
                button.disabledColor = Color.white;
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

        protected abstract void ButtonClick(UIComponent component, UIMouseEventParameter eventParam = null);
        protected abstract bool IsSelect(int index);

        public virtual void DeInit()
        {
            Clear();

            autoButtonSize = true;
            buttonWidth = 50f;
            textScale = 0.8f;
        }
        public virtual void Clear()
        {
            Objects.Clear();

            foreach (var button in Buttons)
                ComponentPool.Free(button);

            Buttons.Clear();
            Clickable.Clear();
        }

        public void SetDefaultStyle(Vector2? size = null) { }
    }

    public abstract class UIOnceSegmented<ValueType> : UISegmented<ValueType>, IUIOnceSelector<ValueType>, IValueChanger<ValueType>
    {
        public event Action<ValueType> OnSelectObjectChanged;
        event Action<ValueType> IValueChanger<ValueType>.OnValueChanged
        {
            add => OnSelectObjectChanged += value;
            remove => OnSelectObjectChanged -= value;
        }

        private int SelectedIndex { get; set; } = -1;
        public ValueType SelectedObject
        {
            get => SelectedIndex >= 0 ? Objects[SelectedIndex] : default;
            set => SetSelected(Objects.FindIndex(o => IsEqualDelegate?.Invoke(o, value) ?? ReferenceEquals(o, value) || o.Equals(value)), false);
        }
        ValueType IValueChanger<ValueType>.Value
        {
            get => SelectedObject;
            set => SelectedObject = value;
        }
        public bool UseWheel { get; set; }
        public bool WheelTip
        {
            set { }
        }
        string IValueChanger<ValueType>.Format { set { } }


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
        public override void DeInit()
        {
            base.DeInit();
            OnSelectObjectChanged = null;
            UseWheel = false;
        }
        public override void Clear()
        {
            SelectedIndex = -1;
            base.Clear();
        }
        protected override void ButtonClick(UIComponent component, UIMouseEventParameter eventParam = null) => SetSelected(Buttons.FindIndex(b => b == component));
        protected override bool IsSelect(int index) => SelectedIndex == index;
    }
    public class BoolSegmented : UIOnceSegmented<bool>
    {
        public BoolSegmented()
        {
            IsEqualDelegate = (x, y) => x == y;
        }
    }
    public class IntSegmented : UIOnceSegmented<int>
    {
        public IntSegmented()
        {
            IsEqualDelegate = (x, y) => x == y;
        }
    }

    public abstract class UIMultySegmented<ValueType> : UISegmented<ValueType>, IUIMultySelector<ValueType>, IValueChanger<List<ValueType>>
    {
        public event Action<List<ValueType>> OnSelectObjectsChanged;
        event Action<List<ValueType>> IValueChanger<List<ValueType>>.OnValueChanged
        {
            add => OnSelectObjectsChanged += value;
            remove => OnSelectObjectsChanged -= value;
        }

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
        List<ValueType> IValueChanger<List<ValueType>>.Value
        {
            get => SelectedObjects;
            set => SelectedObjects = value;
        }
        string IValueChanger<List<ValueType>>.Format { set { } }

        private void SetSelected(HashSet<int> indices = null, bool callEvent = true)
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

        public override void DeInit()
        {
            base.DeInit();
            OnSelectObjectsChanged = null;
        }
        public override void Clear()
        {
            SelectedIndices.Clear();
            base.Clear();
        }
        protected override void ButtonClick(UIComponent component, UIMouseEventParameter eventParam = null)
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
