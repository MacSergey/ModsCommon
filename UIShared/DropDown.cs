using ColossalFramework.UI;
using ModsCommon.Utilities;
using System;
using System.Collections.Generic;
using UnityEngine;
using static ColossalFramework.UI.UIButton;

namespace ModsCommon.UI
{
    public abstract class UIDropDown<ValueType> : CustomUIDropDown, IUIOnceSelector<ValueType>
    {
        public event Action<ValueType> OnSelectObjectChanged;
        public event PropertyChangedEventHandler<ButtonState> eventButtonStateChanged;

        bool IReusable.InCache { get; set; }
        public Func<ValueType, ValueType, bool> IsEqualDelegate { get; set; }
        private List<ValueType> Objects { get; } = new List<ValueType>();
        public ValueType SelectedObject
        {
            get => selectedIndex >= 0 ? Objects[selectedIndex] : default;
            set => selectedIndex = Objects.FindIndex(o => IsEqualDelegate?.Invoke(o, value) ?? ReferenceEquals(o, value) || (o != null && o.Equals(value)));
        }
        public bool CanWheel { get; set; }
        public bool UseWheel { get; set; }
        public bool WheelTip
        {
            set => tooltip = value ? CommonLocalize.ListPanel_ScrollWheel : string.Empty;
        }
        public bool UseScrollBar { get; set; }

        protected ButtonState m_State;
        public ButtonState state
        {
            get => m_State;
            set
            {
                if (value != m_State)
                {
                    OnButtonStateChanged(value);
                    Invalidate();
                }
            }
        }

        Color32? _bgColor;
        Color32? _bgFocusedColor;
        Color32? _bgHoveredColor;
        Color32? _bgPressedColor;
        Color32? _bgDisabledColor;
        public Color32 BgColor
        {
            get => _bgColor ?? color;
            set
            {
                _bgColor = value;
                Invalidate();
            }
        }
        public Color32 BgFocusedColor
        {
            get => _bgFocusedColor ?? color;
            set
            {
                _bgFocusedColor = value;
                Invalidate();
            }
        }
        public Color32 BgHoveredColor
        {
            get => _bgHoveredColor ?? color;
            set
            {
                _bgHoveredColor = value;
                Invalidate();
            }
        }
        public Color32 BgPressedColor
        {
            get => _bgPressedColor ?? color;
            set
            {
                _bgPressedColor = value;
                Invalidate();
            }
        }
        public Color32 BgDisabledColor
        {
            get => _bgDisabledColor ?? disabledColor;
            set
            {
                _bgDisabledColor = value;
                Invalidate();
            }
        }

        public UIDropDown()
        {
            eventSelectedIndexChanged += IndexChanged;
        }

        protected virtual void IndexChanged(UIComponent component, int value) => OnSelectObjectChanged?.Invoke(SelectedObject);

        public void AddItem(ValueType item, string label = null)
        {
            Objects.Add(item);
            AddItem(label ?? item.ToString());
        }
        public void Clear()
        {
            selectedIndex = -1;
            Objects.Clear();
            items = new string[0];
        }
        protected override void OnMouseMove(UIMouseEventParameter p)
        {
            base.OnMouseMove(p);
            CanWheel = true;
        }

        public void StopLayout() { }
        public void StartLayout(bool layoutNow = true) { }

        void IReusable.DeInit()
        {
            Clear();
            UseWheel = false;
            WheelTip = false;
            UseScrollBar = false;
        }

        public void SetDefaultStyle(Vector2? size = null)
        {
            ComponentStyle.DefaultStyle(this, size);

            if (UseScrollBar)
                listScrollbar = UIHelper.ScrollBar;
        }
        public void SetSettingsStyle(Vector2? size = null)
        {
            ComponentStyle.DefaultSettingsStyle(this, size);

            if (UseScrollBar)
                listScrollbar = UIHelper.ScrollBar;
        }

        protected override void OnRebuildRenderData()
        {
            if (atlas != null && font != null && font.isValid)
            {
                if (textRenderData != null)
                    textRenderData.Clear();
                else
                {
                    UIRenderData item = UIRenderData.Obtain();
                    m_RenderData.Add(item);
                }

                renderData.material = base.atlas.material;
                textRenderData.material = base.atlas.material;
                RenderBackground();
                RenderText();
            }
        }

        protected override void RenderBackground()
        {
            if (GetBackgroundSprite() is UITextureAtlas.SpriteInfo backgroundSprite)
            {
                var color = ApplyOpacity(GetBackgroundColor());
                RenderOptions renderOptions = new RenderOptions()
                {
                    _atlas = atlas,
                    _color = color,
                    _fillAmount = 1f,
                    _flip = UISpriteFlip.None,
                    _offset = pivot.TransformToUpperLeft(size, arbitraryPivotOffset),
                    _pixelsToUnits = PixelsToUnits(),
                    _size = size,
                    _spriteInfo = backgroundSprite,
                };

                if (backgroundSprite.isSliced)
                    Render.RenderSlicedSprite(renderData, renderOptions);
                else
                    Render.RenderSprite(renderData, renderOptions);
            }
        }
        private void RenderText()
        {
            if (selectedIndex < 0 || selectedIndex >= items.Length)
                return;

            var text = items[selectedIndex];
            var num = PixelsToUnits();
            var maxSize = new Vector2(size.x - textFieldPadding.horizontal, size.y - textFieldPadding.vertical);
            var vector = pivot.TransformToUpperLeft(size, arbitraryPivotOffset);
            var vectorOffset = new Vector3(vector.x + textFieldPadding.left, vector.y - textFieldPadding.top, 0f) * num;
            var defaultColor = isEnabled ? textColor : disabledColor;
            using UIFontRenderer uIFontRenderer = font.ObtainRenderer();
            uIFontRenderer.wordWrap = false;
            uIFontRenderer.maxSize = maxSize;
            uIFontRenderer.pixelRatio = num;
            uIFontRenderer.textScale = textScale;
            uIFontRenderer.characterSpacing = characterSpacing;
            uIFontRenderer.vectorOffset = vectorOffset;
            uIFontRenderer.multiLine = false;
            uIFontRenderer.textAlign = UIHorizontalAlignment.Left;
            uIFontRenderer.processMarkup = processMarkup;
            uIFontRenderer.colorizeSprites = colorizeSprites;
            uIFontRenderer.defaultColor = defaultColor;
            uIFontRenderer.bottomColor = useGradient ? new Color32?(bottomColor) : null;
            uIFontRenderer.overrideMarkupColors = false;
            uIFontRenderer.opacity = CalculateOpacity();
            uIFontRenderer.outline = useOutline;
            uIFontRenderer.outlineSize = outlineSize;
            uIFontRenderer.outlineColor = outlineColor;
            uIFontRenderer.shadow = useDropShadow;
            uIFontRenderer.shadowColor = dropShadowColor;
            uIFontRenderer.shadowOffset = dropShadowOffset;
            if (uIFontRenderer is UIDynamicFont.DynamicFontRenderer dynamicFontRenderer)
            {
                dynamicFontRenderer.spriteAtlas = atlas;
                dynamicFontRenderer.spriteBuffer = renderData;
            }
            uIFontRenderer.Render(text, textRenderData);
        }

        protected virtual void OnButtonStateChanged(ButtonState value)
        {
            if (isEnabled || value == ButtonState.Disabled)
            {
                m_State = value;
                if (eventButtonStateChanged != null)
                    eventButtonStateChanged(this, value);

                Invoke("OnButtonStateChanged", value);
                Invalidate();
            }
        }
        protected override void OnEnterFocus(UIFocusEventParameter p)
        {
            if (state != ButtonState.Pressed)
                state = ButtonState.Focused;

            base.OnEnterFocus(p);
        }
        protected override void OnLeaveFocus(UIFocusEventParameter p)
        {
            state = containsMouse ? ButtonState.Hovered : ButtonState.Normal;
            base.OnLeaveFocus(p);
        }
        protected override void OnMouseDown(UIMouseEventParameter p)
        {
            if (state != ButtonState.Focused)
                state = ButtonState.Pressed;

            base.OnMouseDown(p);
        }
        protected override void OnMouseUp(UIMouseEventParameter p)
        {
            if (m_IsMouseHovering)
            {
                if (containsFocus)
                    state = ButtonState.Focused;
                else
                    state = ButtonState.Hovered;
            }
            else if (hasFocus)
                state = ButtonState.Focused;
            else
                state = ButtonState.Normal;

            base.OnMouseUp(p);
        }
        protected override void OnMouseEnter(UIMouseEventParameter p)
        {
            if (state != ButtonState.Focused)
                state = ButtonState.Hovered;

            base.OnMouseEnter(p);
        }
        protected override void OnMouseLeave(UIMouseEventParameter p)
        {
            if (containsFocus)
                state = ButtonState.Focused;
            else
                state = ButtonState.Normal;

            base.OnMouseLeave(p);
            CanWheel = false;
        }
        protected sealed override void OnMouseWheel(UIMouseEventParameter p)
        {
            m_TooltipShowing = true;
            tooltipBox.Hide();

            if (UseWheel && (CanWheel || Time.realtimeSinceStartup - m_HoveringStartTime >= 1f))
            {
                if (p.wheelDelta > 0 && selectedIndex > 0)
                    selectedIndex -= 1;
                else if (p.wheelDelta < 0 && selectedIndex < Objects.Count - 1)
                    selectedIndex += 1;

                p.Use();
            }
        }
        protected override void OnSizeChanged()
        {
            base.OnSizeChanged();

            listWidth = (int)width;
            if (triggerButton is UIComponent button && button != this)
                button.size = size;
        }
        protected override void OnIsEnabledChanged()
        {
            if (!isEnabled)
                state = ButtonState.Disabled;
            else
                state = ButtonState.Normal;

            base.OnIsEnabledChanged();
        }

        private Color32 GetBackgroundColor()
        {
            return state switch
            {
                ButtonState.Focused => BgFocusedColor,
                ButtonState.Hovered => BgHoveredColor,
                ButtonState.Pressed => BgPressedColor,
                ButtonState.Disabled => BgDisabledColor,
                _ => BgColor,
            };
        }
        protected override UITextureAtlas.SpriteInfo GetBackgroundSprite()
        {
            var spriteInfo = state switch
            {
                ButtonState.Normal => atlas[normalBgSprite],
                ButtonState.Focused => atlas[focusedBgSprite],
                ButtonState.Hovered => atlas[hoveredBgSprite],
                //ButtonState.Pressed => atlas[pressedBgSprite],
                ButtonState.Disabled => atlas[disabledBgSprite],
            };

            return spriteInfo ?? atlas[normalBgSprite];
        }
    }
}
