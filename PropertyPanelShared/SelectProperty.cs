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

        protected override void FillContent()
        {
            Selector = Content.AddUIComponent<PanelType>();
            Selector.name = nameof(Selector);
            Selector.width = Width;
            Selector.OnValueChanged += ValueChanged;

            Selector.eventClick += ButtonClick;
            Selector.eventMouseEnter += ButtonMouseEnter;
            Selector.eventMouseLeave += ButtonMouseLeave;
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
            get => Selector.IsSelected;
            set => Selector.IsSelected = value;
        }

        protected void ValueChanged(Type value) => OnValueChanged?.Invoke(value);
        protected virtual void ButtonClick(UIComponent component, UIMouseEventParameter eventParam) => OnSelect?.Invoke(Selector);
        protected virtual void ButtonMouseEnter(UIComponent component, UIMouseEventParameter eventParam) => OnEnter?.Invoke(Selector);
        protected virtual void ButtonMouseLeave(UIComponent component, UIMouseEventParameter eventParam) => OnLeave?.Invoke(Selector);
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
            var button = Content.AddUIComponent<CustomUIButton>();
            button.SetDefaultStyle();
            button.size = new Vector2(20f, 20f);
            button.text = "×";
            button.tooltip = ResetToolTip;
            button.textScale = 1.3f;
            button.TextPadding = new RectOffset(0, 0, 0, 0);
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
        protected abstract string NotSet { get; }


        public event Action<Type> OnValueChanged;
        public abstract Type Value { get; set; }

        public SelectPropertyButton()
        {
            text = NotSet;

            AtlasBackground = CommonTextures.Atlas;
            bgSprites = CommonTextures.FieldSingle;
            selBgSprites = CommonTextures.FieldSingle;
            bgColors = new ColorSet(ComponentStyle.FieldNormalColor, ComponentStyle.FieldHoveredColor, ComponentStyle.FieldHoveredColor, ComponentStyle.FieldNormalColor, ComponentStyle.FieldDisabledColor);
            selBgColors = new ColorSet(ComponentStyle.FieldFocusedColor, ComponentStyle.FieldFocusedColor, ComponentStyle.FieldFocusedColor, ComponentStyle.FieldFocusedColor, ComponentStyle.FieldDisabledFocusedColor);

            AtlasForeground = CommonTextures.Atlas;
            fgSprites = CommonTextures.ArrowDown;
            fgColors = Color.black;

            ForegroundSpriteMode = UIForegroundSpriteMode.Scale;
            ScaleFactor = 0.7f;
            HorizontalAlignment = UIHorizontalAlignment.Right;
            VerticalAlignment = UIVerticalAlignment.Middle;
            SpritePadding = new RectOffset(0, 5, 0, 0);

            enabled = true;
            AutoSize = AutoSize.None;
            TextHorizontalAlignment = UIHorizontalAlignment.Left;
            TextVerticalAlignment = UIVerticalAlignment.Middle;
            height = 20;
            textScale = 0.6f;
            TextPadding = new RectOffset(8, 0, 4, 0);
        }

        protected void ValueChanged()
        {
            OnValueChanged?.Invoke(Value);
            text = Value?.ToString() ?? NotSet;
        }

        public void SetStyle(DropDownStyle style)
        {
            bgColors = style.BgColors;
            fgColors = style.FgColors;
            textColors = style.TextColors;

            Invalidate();
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
        private Type value;
        public override Type Value
        {
            get => value;
            set
            {
                this.value = value;
                ValueChanged();
            }
        }
    }
}
