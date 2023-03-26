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

        public static explicit operator OptionData(string label) => new OptionData(label);
        public static implicit operator string(OptionData data) => data.label;
    }
    public abstract class UISegmented<ValueType> : CustomUIPanel, IReusable
    {
        bool IReusable.InCache { get; set; }
        public Func<ValueType, ValueType, bool> IsEqualDelegate { get; set; }
        protected List<ValueType> Objects { get; } = new List<ValueType>();
        protected List<CustomUIButton> Buttons { get; } = new List<CustomUIButton>();
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
            autoLayout = AutoLayout.Horizontal;
            autoChildrenHorizontally = AutoLayoutChildren.Fit;
            autoChildrenVertically = AutoLayoutChildren.Fit;
        }

        public void AddItem(ValueType item, OptionData optionData) => AddItem(item, optionData, true, null);
        public void AddItem(ValueType item, OptionData optionData, bool clickable = true, float? width = null)
        {
            Objects.Add(item);

            var button = AddUIComponent<CustomUIButton>();
            button.Atlas = CommonTextures.Atlas;
            button.TextHorizontalAlignment = UIHorizontalAlignment.Center;
            SetColor(button);

            if (optionData.atlas != null && !string.IsNullOrEmpty(optionData.sprite))
            {
                button.AtlasForeground = optionData.atlas;
                button.FgSprites = optionData.sprite;
                button.tooltip = optionData.label ?? item.ToString();
                button.ForegroundSpriteMode = UIForegroundSpriteMode.Scale;
            }
            else
                button.text = optionData.label ?? item.ToString();

            UpdateButton(button, width);
            if (clickable)
                button.eventClick += ButtonClick;
            else
                button.isEnabled = false;

            Buttons.Add(button);
            SetSprite(button);

            if (Buttons.Count >= 2)
                SetSprite(Buttons[Buttons.Count - 2]);
        }
        private void SetButtonsWidth()
        {
            PauseLayout(() =>
            {
                foreach (var button in Buttons)
                    UpdateButton(button);
            });
        }
        private void UpdateButton(CustomUIButton button = null, float? width = null)
        {
            button.TextPadding = new RectOffset(TextPadding, TextPadding, 4, 0);
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
        protected void SetSprite(CustomUIButton button)
        {
            var index = Buttons.IndexOf(button);

            if (index == 0)
            {
                if (Buttons.Count == 1)
                    button.BgSprites = CommonTextures.FieldSingle;
                else
                    button.BgSprites = CommonTextures.FieldLeft;
            }
            else
            {
                if (index == Buttons.Count - 1)
                    button.BgSprites = CommonTextures.FieldRight;
                else
                    button.BgSprites = CommonTextures.FieldMiddle;
            }
        }
        protected void SetColor(CustomUIButton button)
        {
            if (Style != null)
                button.SetStyle(Style);
        }

        protected abstract void ButtonClick(UIComponent component, UIMouseEventParameter eventParam = null);

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
        }

        public void SetDefaultStyle(Vector2? size = null) { }

        private ButtonStyle Style { get; set; } = ControlStyle.Default.Button;
        public void SetStyle(ButtonStyle style)
        {
            Style = style;

            foreach (var button in Buttons)
                SetColor(button);
        }
    }

    public abstract class UIOnceSegmented<ValueType> : UISegmented<ValueType>, IUIOnceSelector<ValueType>, IValueChanger<ValueType>
    {
        public event Action<ValueType> OnSelectObject;
        event Action<ValueType> IValueChanger<ValueType>.OnValueChanged
        {
            add => OnSelectObject += value;
            remove => OnSelectObject -= value;
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
                Buttons[SelectedIndex].IsSelected = false;

            SelectedIndex = index;

            if (SelectedIndex != -1)
            {
                Buttons[SelectedIndex].IsSelected = true;
                if (callEvent)
                    OnSelectObject?.Invoke(SelectedObject);
            }
        }
        public override void DeInit()
        {
            base.DeInit();
            OnSelectObject = null;
            UseWheel = false;
        }
        public override void Clear()
        {
            SelectedIndex = -1;
            base.Clear();
        }
        protected override void ButtonClick(UIComponent component, UIMouseEventParameter eventParam = null) => SetSelected(Buttons.FindIndex(b => b == component));
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
        public event Action<List<ValueType>> OnSelectedObjectsChanged;
        event Action<List<ValueType>> IValueChanger<List<ValueType>>.OnValueChanged
        {
            add => OnSelectedObjectsChanged += value;
            remove => OnSelectedObjectsChanged -= value;
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
                    Buttons[index].IsSelected = false;
            }

            foreach (var index in indices)
            {
                if (!SelectedIndices.Contains(index))
                    Buttons[index].IsSelected = true;
            }

            SelectedIndices = new HashSet<int>(indices);

            if (callEvent)
                OnSelectedObjectsChanged?.Invoke(SelectedObjects);
        }

        public override void DeInit()
        {
            base.DeInit();
            OnSelectedObjectsChanged = null;
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
    }
}
