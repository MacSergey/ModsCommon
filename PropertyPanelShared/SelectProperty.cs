using ColossalFramework.UI;
using ModsCommon.Utilities;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ModsCommon.UI
{
    public abstract class SelectPropertyPanel<Type, PanelType> : EditorPropertyPanel, IReusable
        where PanelType : SelectPropertyButton<Type>
    {
        public event Action<Type> OnValueChanged;
        public event Action<PanelType> OnSelect;
        public event Action<PanelType> OnEnter;
        public event Action<PanelType> OnLeave;

        bool IReusable.InCache { get; set; }
        public PanelType Selector { get; private set; }
        protected abstract float Width { get; }

        public Type Value
        {
            get => Selector.Value;
            set => Selector.Value = value;
        }

        public SelectPropertyPanel()
        {
            AddSelector();
        }
        private void AddSelector()
        {
            Selector = Content.AddUIComponent<PanelType>();
            Selector.width = Width;
            Selector.OnValueChanged += ValueChanged;

            Selector.Button.eventClick += ButtonClick;
            Selector.Button.eventMouseEnter += ButtonMouseEnter;
            Selector.Button.eventMouseLeave += ButtonMouseLeave;
        }

        public override void DeInit()
        {
            base.DeInit();

            OnSelect = null;
            OnEnter = null;
            OnLeave = null;
            OnValueChanged = null;
        }
        public bool Selected
        {
            get => Selector.Selected;
            set => Selector.Selected = value;
        }
        public new void Focus() => Selector.Focus();

        protected void ValueChanged(Type value) => OnValueChanged?.Invoke(value);
        protected virtual void ButtonClick(UIComponent component, UIMouseEventParameter eventParam) => OnSelect?.Invoke(component.parent as PanelType);
        protected virtual void ButtonMouseEnter(UIComponent component, UIMouseEventParameter eventParam) => OnEnter?.Invoke(component.parent as PanelType);
        protected virtual void ButtonMouseLeave(UIComponent component, UIMouseEventParameter eventParam) => OnLeave?.Invoke(component.parent as PanelType);
    }

    public abstract class ResetableSelectPropertyPanel<Type, PanelType> : SelectPropertyPanel<Type, PanelType>
        where PanelType : SelectListPropertyButton<Type>
    {
        public event Action<PanelType> OnReset;
        protected abstract string ResetToolTip { get; }

        public ResetableSelectPropertyPanel()
        {
            AddReset();
        }
        private void AddReset()
        {
            var button = AddButton(Content);

            button.size = new Vector2(20f, 20f);
            button.text = "×";
            button.tooltip = ResetToolTip;
            button.textScale = 1.3f;
            button.textPadding = new RectOffset(0, 0, 0, 0);
            button.eventClick += ResetClick;
        }

        public override void DeInit()
        {
            base.DeInit();

            OnReset = null;
        }

        protected virtual void ResetClick(UIComponent component, UIMouseEventParameter eventParam)
        {
            Value = default;
            OnReset?.Invoke(component as PanelType);
        }
    }
    public abstract class SelectPropertyButton<Type> : CustomUIButton
    {
        public override bool containsFocus => false;
        public CustomUIButton Button { get; }
        protected abstract string NotSet { get; }


        public event Action<Type> OnValueChanged;
        public abstract Type Value { get; set; }

        public SelectPropertyButton()
        {
            text = NotSet;
            atlas = CommonTextures.Atlas;
            normalBgSprite = CommonTextures.FieldNormal;
            hoveredBgSprite = CommonTextures.FieldHovered;
            disabledBgSprite = CommonTextures.FieldDisabled;
            focusedBgSprite = CommonTextures.FieldFocused;
            isInteractive = false;
            enabled = true;
            autoSize = false;
            textHorizontalAlignment = UIHorizontalAlignment.Left;
            textVerticalAlignment = UIVerticalAlignment.Middle;
            height = 20;
            textScale = 0.6f;
            textPadding = new RectOffset(8, 0, 4, 0);

            Button = AddUIComponent<CustomUIButton>();
            Button.atlas = TextureHelper.InGameAtlas;
            Button.text = string.Empty;
            Button.size = size;
            Button.relativePosition = new Vector3(0f, 0f);
            Button.textVerticalAlignment = UIVerticalAlignment.Middle;
            Button.textHorizontalAlignment = UIHorizontalAlignment.Left;
            Button.normalFgSprite = "IconDownArrow";
            Button.hoveredFgSprite = "IconDownArrowHovered";
            Button.pressedFgSprite = "IconDownArrowPressed";
            Button.focusedFgSprite = "IconDownArrow";
            Button.foregroundSpriteMode = UIForegroundSpriteMode.Scale;
            Button.horizontalAlignment = UIHorizontalAlignment.Right;
            Button.verticalAlignment = UIVerticalAlignment.Middle;
            Button.textScale = 0.8f;
            Button.eventIsEnabledChanged += ButtonIsEnabledChanged;
        }

        protected override void OnSizeChanged()
        {
            base.OnSizeChanged();
            if (Button != null)
                Button.size = size;
        }
        private void ButtonIsEnabledChanged(UIComponent component, bool value) => Button.normalFgSprite = value ? "IconDownArrow" : "Empty";

        protected void ValueChanged()
        {
            OnValueChanged?.Invoke(Value);
            text = Value?.ToString() ?? NotSet;
        }
        public new void Focus() => Button.Focus();
        public bool Selected
        {
            get => state == ButtonState.Focused;
            set => state = value ? ButtonState.Focused : ButtonState.Normal;
        }
    }
    public abstract class SelectListPropertyButton<Type> : SelectPropertyButton<Type>
    {
        private int SelectIndex { get; set; } = -1;

        private List<Type> ObjectsList { get; set; } = new List<Type>();
        public IEnumerable<Type> Objects => ObjectsList;
        public override Type Value
        {
            get => SelectIndex == -1 ? default : ObjectsList[SelectIndex];
            set => SetSelected(ObjectsList.FindIndex(o => IsEqual(value, o)));
        }
        private void SetSelected(int index)
        {
            if (index != SelectIndex)
            {
                SelectIndex = index;
                ValueChanged();
            }
        }

        public void Add(Type item) => ObjectsList.Add(item);
        public void AddRange(IEnumerable<Type> items) => ObjectsList.AddRange(items);
        public void Clear()
        {
            ObjectsList.Clear();
            Value = default;
        }

        protected abstract bool IsEqual(Type first, Type second);
    }
    public abstract class SelectItemPropertyButton<Type> : SelectPropertyButton<Type>
    {
        private Type _value;
        public override Type Value
        {
            get => _value;
            set
            {
                _value = value;
                ValueChanged();
            }
        }
    }
}
