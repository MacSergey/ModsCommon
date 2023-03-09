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
            get => Selector.isSelected;
            set => Selector.isSelected = value;
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
        protected abstract string NotSet { get; }


        public event Action<Type> OnValueChanged;
        public abstract Type Value { get; set; }

        public SelectPropertyButton()
        {
            text = NotSet;

            atlasBackground = CommonTextures.Atlas;
            SetBgSprite(new UI.SpriteSet(CommonTextures.FieldSingle));
            SetBgColor(new ColorSet(ComponentStyle.FieldNormalColor, ComponentStyle.FieldHoveredColor, ComponentStyle.FieldHoveredColor, ComponentStyle.FieldNormalColor, ComponentStyle.FieldDisabledColor));
            SetSelectedBgColor(new ColorSet(ComponentStyle.FieldFocusedColor, ComponentStyle.FieldFocusedColor, ComponentStyle.FieldFocusedColor, ComponentStyle.FieldFocusedColor, ComponentStyle.FieldDisabledFocusedColor));

            atlasForeground = CommonTextures.Atlas;
            SetFgSprite(new UI.SpriteSet(CommonTextures.ArrowDown));
            SetFgColor(new ColorSet(new Color32(0, 0, 0, 255)));
            SetSelectedFgColor(new ColorSet(new Color32(0, 0, 0, 255)));

            foregroundSpriteMode = UIForegroundSpriteMode.Scale;
            scaleFactor = 0.7f;
            horizontalAlignment = UIHorizontalAlignment.Right;
            verticalAlignment = UIVerticalAlignment.Middle;
            spritePadding = new RectOffset(0, 5, 0, 0);

            enabled = true;
            autoSize = false;
            textHorizontalAlignment = UIHorizontalAlignment.Left;
            textVerticalAlignment = UIVerticalAlignment.Middle;
            height = 20;
            textScale = 0.6f;
            textPadding = new RectOffset(8, 0, 4, 0);
        }

        protected void ValueChanged()
        {
            OnValueChanged?.Invoke(Value);
            text = Value?.ToString() ?? NotSet;
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
