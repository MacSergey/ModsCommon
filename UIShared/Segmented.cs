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
    public class CustomUISegmentedButton : CustomUIButton
    {
        public SegmentedButtonType Type { get; set; }
        public ButtonStyle SegmentedStyle
        {
            set
            {
                bgColors = value.BgColors;
                selBgColors = value.SelBgColors;

                fgColors = value.FgColors;
                selFgColors = value.SelFgColors;

                textColors = value.TextColors;
                selTextColors = value.SelTextColors;

                OnColorChanged();
            }
        }
    }
    public enum SegmentedButtonType
    {
        Single,
        Left,
        Middle,
        Right,
    }
    public abstract class UISegmented<ValueType> : CustomUIPanel, IReusable
    {
        bool IReusable.InCache { get; set; }
        public Func<ValueType, ValueType, bool> IsEqualDelegate { get; set; }
        protected List<ValueType> Objects { get; } = new List<ValueType>();
        protected List<CustomUISegmentedButton> Buttons { get; } = new List<CustomUISegmentedButton>();
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

            var button = AddUIComponent<CustomUISegmentedButton>();
            button.name = optionData.label ?? item.ToString();
            button.Atlas = CommonTextures.Atlas;
            button.TextHorizontalAlignment = UIHorizontalAlignment.Center;

            if (optionData.atlas != null && !string.IsNullOrEmpty(optionData.sprite))
            {
                button.FgAtlas = optionData.atlas;
                button.AllFgSprites = optionData.sprite;
                button.tooltip = optionData.label ?? item.ToString();
                button.ForegroundSpriteMode = SpriteMode.Scale;
            }
            else
                button.text = optionData.label ?? item.ToString();

            UpdateButton(button, width);
            if (clickable)
                button.eventClick += ButtonClick;
            else
                button.isEnabled = false;

            Buttons.Add(button);
            SetType(button);
            SetStyle(button);

            if (Buttons.Count >= 2)
            {
                SetType(Buttons[Buttons.Count - 2]);
                SetStyle(Buttons[Buttons.Count - 2]);
            }
        }
        private void SetButtonsWidth()
        {
            PauseLayout(() =>
            {
                foreach (var button in Buttons)
                    UpdateButton(button);
            });
        }
        private void UpdateButton(CustomUISegmentedButton button = null, float? width = null)
        {
            button.TextPadding = new RectOffset(TextPadding, TextPadding, 4, 0);
            button.textScale = TextScale;

            if (AutoButtonSize)
            {
                button.height = 20f;
                button.PerformAutoWidth();
            }
            else if (width.HasValue)
                button.size = new Vector2(width.Value, 20f);
            else
                button.size = new Vector2(ButtonWidth, 20f);
        }
        private void SetType(CustomUISegmentedButton button)
        {
            var index = Buttons.IndexOf(button);

            if (index == 0)
            {
                if (Buttons.Count == 1)
                    button.Type = SegmentedButtonType.Single;
                else
                    button.Type = SegmentedButtonType.Left;
            }
            else
            {
                if (index == Buttons.Count - 1)
                    button.Type = SegmentedButtonType.Right;
                else
                    button.Type = SegmentedButtonType.Middle;
            }
        }
        protected void SetStyle(CustomUISegmentedButton button)
        {
            if (Style == null)
            {
                switch (button.Type)
                {
                    case SegmentedButtonType.Single:
                        button.AllBgSprites = CommonTextures.FieldSingle;
                        break;
                    case SegmentedButtonType.Left:
                        button.AllBgSprites = CommonTextures.FieldLeft;
                        break;
                    case SegmentedButtonType.Middle:
                        button.AllBgSprites = CommonTextures.FieldMiddle;
                        break;
                    case SegmentedButtonType.Right:
                        button.AllBgSprites = CommonTextures.FieldRight;
                        break;
                }
            }
            else
            {
                var fgAtlas = button.FgAtlas;
                var fgSprites = button.FgSprites;
                var selFgSprites = button.SelFgSprites;

                switch (button.Type)
                {
                    case SegmentedButtonType.Single:
                        button.ButtonStyle = Style.Single;
                        break;
                    case SegmentedButtonType.Left:
                        button.ButtonStyle = Style.Left;
                        break;
                    case SegmentedButtonType.Middle:
                        button.ButtonStyle = Style.Middle;
                        break;
                    case SegmentedButtonType.Right:
                        button.ButtonStyle = Style.Right;
                        break;
                }

                button.FgAtlas = fgAtlas;
                button.FgSprites = fgSprites;
                button.SelFgSprites = selFgSprites;
            }
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

            PauseLayout(() =>
            {
                foreach (var button in Buttons)
                    ComponentPool.Free(button);
            });

            Buttons.Clear();
        }

        public void SetDefaultStyle(Vector2? size = null) { }

        private SegmentedStyle Style { get; set; } = ComponentStyle.Default.Segmented;
        public void SetStyle(SegmentedStyle style)
        {
            Style = style;

            foreach (var button in Buttons)
                SetStyle(button);
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
