using ColossalFramework.UI;
using ModsCommon.UI;
using ModsCommon.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ModsCommon.UI
{
    public abstract class SelectPropertyPanel<Type> : EditorPropertyPanel, IReusable
    {
        public event Action<Type> OnSelectChanged;
        public event Action OnSelect;
        public event Action OnHover;
        public event Action OnLeave;

        int _selectIndex = -1;

        protected abstract string NotSet { get; }
        SelectPropertyButton Selector { get; set; }
        UIButton Button { get; set; }
        protected abstract float Width { get; }

        int SelectIndex
        {
            get => _selectIndex;
            set
            {
                if (value != _selectIndex)
                {
                    _selectIndex = value;
                    OnSelectChanged?.Invoke(SelectedObject);
                    Selector.text = SelectedObject?.ToString() ?? NotSet;
                }
            }
        }
        List<Type> ObjectsList { get; set; } = new List<Type>();
        public IEnumerable<Type> Objects => ObjectsList;
        public Type SelectedObject
        {
            get => SelectIndex == -1 ? default : ObjectsList[SelectIndex];
            set => SelectIndex = ObjectsList.FindIndex(o => IsEqual(value, o));
        }

        public bool Selected
        {
            get => Selector.state == UIButton.ButtonState.Focused;
            set => Selector.state = value ? UIButton.ButtonState.Focused : UIButton.ButtonState.Normal;
        }

        public SelectPropertyPanel()
        {
            AddSelector();
        }
        private void AddSelector()
        {
            Selector = Control.AddUIComponent<SelectPropertyButton>();
            Selector.text = NotSet;
            Selector.atlas = TextureHelper.CommonAtlas;
            Selector.normalBgSprite = TextureHelper.FieldNormal;
            Selector.hoveredBgSprite = TextureHelper.FieldHovered;
            Selector.disabledBgSprite = TextureHelper.FieldDisabled;
            Selector.focusedBgSprite = TextureHelper.FieldFocused;
            Selector.isInteractive = false;
            Selector.enabled = true;
            Selector.autoSize = false;
            Selector.textHorizontalAlignment = UIHorizontalAlignment.Left;
            Selector.textVerticalAlignment = UIVerticalAlignment.Middle;
            Selector.height = 20;
            Selector.width = Width;
            Selector.textScale = 0.6f;
            Selector.textPadding = new RectOffset(8, 0, 4, 0);

            Button = Selector.AddUIComponent<UIButton>();
            Button.atlas = TextureHelper.InGameAtlas;
            Button.text = string.Empty;
            Button.size = Selector.size;
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

            Button.eventClick += ButtonClick;
            Button.eventMouseEnter += ButtonMouseEnter;
            Button.eventMouseLeave += ButtonMouseLeave;
            Button.eventIsEnabledChanged += ButtonIsEnabledChanged;
        }

        private void ButtonIsEnabledChanged(UIComponent component, bool value) => Button.normalFgSprite = value ? "IconDownArrow" : "Empty";

        protected virtual void ButtonClick(UIComponent component, UIMouseEventParameter eventParam) => OnSelect?.Invoke();
        protected virtual void ButtonMouseEnter(UIComponent component, UIMouseEventParameter eventParam) => OnHover?.Invoke();
        protected virtual void ButtonMouseLeave(UIComponent component, UIMouseEventParameter eventParam) => OnLeave?.Invoke();

        public void Add(Type item)
        {
            ObjectsList.Add(item);
        }
        public void AddRange(IEnumerable<Type> items)
        {
            ObjectsList.AddRange(items);
        }
        public void Clear()
        {
            ObjectsList.Clear();
            SelectedObject = default;
        }

        protected abstract bool IsEqual(Type first, Type second);
        public new void Focus() => Button.Focus();
    }
    public class SelectPropertyButton : UIButton
    {
        public override bool containsFocus => false;
    }
}
