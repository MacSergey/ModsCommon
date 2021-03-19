using ColossalFramework;
using ColossalFramework.PlatformServices;
using ColossalFramework.UI;
using ModsCommon.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace ModsCommon.UI
{
    public abstract class MessageBoxBase : UIPanel
    {
        public static float DefaultWidth => 573f;
        public static float DefaultHeight => 200f;
        public static float ButtonHeight => 47f;
        public static int Padding => 16;
        public static float MaxContentHeight => 500f;

        public static T ShowModal<T>()
        where T : MessageBoxBase
        {
            var uiObject = new GameObject();
            uiObject.transform.parent = UIView.GetAView().transform;
            var messageBox = uiObject.AddComponent<T>();

            UIView.PushModal(messageBox);
            messageBox.Show(true);
            messageBox.Focus();

            var view = UIView.GetAView();

            if (view.panelsLibraryModalEffect != null)
            {
                view.panelsLibraryModalEffect.FitTo(null);
                if (!view.panelsLibraryModalEffect.isVisible || view.panelsLibraryModalEffect.opacity != 1f)
                {
                    view.panelsLibraryModalEffect.Show(false);
                    ValueAnimator.Animate("ModalEffect67419", delegate (float val)
                    {
                        view.panelsLibraryModalEffect.opacity = val;
                    }, new AnimatedFloat(0f, 1f, 0.7f, EasingType.CubicEaseOut));
                }
            }

            return messageBox;
        }
        public static void HideModal(MessageBoxBase messageBox)
        {
            UIView.PopModal();

            var view = UIView.GetAView();
            if (view.panelsLibraryModalEffect != null)
            {
                if (!UIView.HasModalInput())
                {
                    ValueAnimator.Animate("ModalEffect67419", delegate (float val)
                    {
                        view.panelsLibraryModalEffect.opacity = val;
                    }, new AnimatedFloat(1f, 0f, 0.7f, EasingType.CubicEaseOut), delegate ()
                    {
                        view.panelsLibraryModalEffect.Hide();
                    });
                }
                else
                {
                    view.panelsLibraryModalEffect.zOrder = UIView.GetModalComponent().zOrder - 1;
                }
            }

            messageBox.Hide();
            Destroy(messageBox.gameObject);
        }

        public string CaptionText { set => Caption.text = value; }

        private UILabel Caption { get; set; }
        protected UIPanel ButtonPanel { get; private set; }
        protected ScrollableContent ScrollableContent { get; private set; }
        private UIDragHandle Handle { get; set; }

        public MessageBoxBase()
        {
            isVisible = true;
            canFocus = true;
            isInteractive = true;
            relativePosition = new Vector3((GetUIView().fixedWidth - width) / 2, (GetUIView().fixedHeight - height) / 2);
            size = new Vector2(DefaultWidth, DefaultHeight);
            color = new Color32(58, 88, 104, 255);
            backgroundSprite = "MenuPanel";

            AddHandle();
            AddPanel();
            FillContent();
            AddButtonPanel();
            Init();

            ScrollableContent.eventSizeChanged += ContentSizeChanged;
        }

        private Vector2 SizeBefore { get; set; } = new Vector2();
        protected override void OnSizeChanged()
        {
            base.OnSizeChanged();

            var view = GetUIView();
            var delta = (size - SizeBefore) / 2;
            SizeBefore = size;

            var x = Mathf.Clamp(relativePosition.x - delta.x, 0f, view.fixedWidth - size.x);
            var y = Mathf.Clamp(relativePosition.y - delta.y, 0f, view.fixedHeight - size.y);

            relativePosition = new Vector2(x, y);
        }

        private void AddHandle()
        {
            Handle = AddUIComponent<UIDragHandle>();
            Handle.size = new Vector2(DefaultWidth, 42);
            Handle.relativePosition = new Vector2(0, 0);
            Handle.eventSizeChanged += (component, size) =>
            {
                Caption.size = size;
                Caption.CenterToParent();
            };

            Caption = Handle.AddUIComponent<UILabel>();
            Caption.textAlignment = UIHorizontalAlignment.Center;
            Caption.textScale = 1.3f;
            Caption.anchor = UIAnchorStyle.Top;

            Caption.eventTextChanged += (component, text) => Caption.CenterToParent();

            var cancel = Handle.AddUIComponent<UIButton>();
            cancel.normalBgSprite = "buttonclose";
            cancel.hoveredBgSprite = "buttonclosehover";
            cancel.pressedBgSprite = "buttonclosepressed";
            cancel.size = new Vector2(32, 32);
            cancel.relativePosition = new Vector2(527, 4);
            cancel.eventClick += (UIComponent component, UIMouseEventParameter eventParam) => Close();
        }
        private void AddPanel()
        {
            ScrollableContent = AddUIComponent<ScrollableContent>();
            ScrollableContent.width = DefaultWidth;
            ScrollableContent.autoLayout = true;
            ScrollableContent.autoLayoutDirection = LayoutDirection.Vertical;
            ScrollableContent.autoLayoutPadding = new RectOffset(Padding, Padding, 0, 0);
            ScrollableContent.clipChildren = true;
            ScrollableContent.builtinKeyNavigation = true;
            ScrollableContent.scrollWheelDirection = UIOrientation.Vertical;
            ScrollableContent.maximumSize = new Vector2(DefaultWidth, MaxContentHeight);
            this.AddScrollbar(ScrollableContent);
        }
        private void ContentSizeChanged(UIComponent component, Vector2 value) => Init();
        private void Init()
        {
            height = Mathf.Floor(Handle.height + ScrollableContent.height + ButtonPanel.height + Padding);
            ScrollableContent.relativePosition = new Vector2(0, Handle.height);
            ButtonPanel.relativePosition = new Vector2(0, Handle.height + ScrollableContent.height + Padding);
        }
        protected virtual void FillContent() { }
        private void AddButtonPanel()
        {
            ButtonPanel = AddUIComponent<UIPanel>();
            ButtonPanel.size = new Vector2(DefaultWidth, ButtonHeight + 10);
        }
        protected UIButton AddButton(int num, int from, Action action)
        {
            var button = ButtonPanel.AddUIComponent<UIButton>();
            button.normalBgSprite = "ButtonMenu";
            button.hoveredTextColor = new Color32(7, 132, 255, 255);
            button.pressedTextColor = new Color32(30, 30, 44, 255);
            button.disabledTextColor = new Color32(7, 7, 7, 255);
            button.horizontalAlignment = UIHorizontalAlignment.Center;
            button.verticalAlignment = UIVerticalAlignment.Middle;
            button.eventClick += (UIComponent component, UIMouseEventParameter eventParam) => action?.Invoke();

            ChangeButton(button, num, from);

            return button;
        }
        private static float Space => 25f;
        public static int DefaultButton { get; set; } = 1;
        protected void ChangeButton(UIButton button, int i, int from, float? positionRatio = null, float? widthRatio = null)
        {
            var width = this.width - (Space * 2 + Space / 2 * (from - 1));
            button.size = new Vector2(width * (widthRatio ?? 1f / from), ButtonHeight);
            button.relativePosition = new Vector2(Space * (0.5f + i / 2f) + width * (positionRatio ?? 1f / from * (i - 1)), 0);
        }
        public void SetButtonsRatio(params int[] ratio)
        {
            var buttons = ButtonPanel.components.OfType<UIButton>().ToArray();
            if (buttons.Length == 0)
                return;

            var sum = 0;
            var resultRatio = new int[buttons.Length];
            for (var i = 0; i < buttons.Length; i += 1)
                sum += resultRatio[i] = (i < ratio.Length ? ratio[i] : 1);

            var before = 0;
            for (var i = 0; i < buttons.Length; i += 1)
            {
                ChangeButton(buttons[i], i + 1, buttons.Length, (float)before / sum, (float)resultRatio[i] / sum);
                before += resultRatio[i];
            }
        }

        protected override void OnKeyDown(UIKeyEventParameter p)
        {
            if (!p.used)
            {
                if (p.keycode == KeyCode.Escape)
                {
                    p.Use();
                    Close();
                }
                else if (p.keycode == KeyCode.Return)
                {
                    if (ButtonPanel.components.OfType<UIButton>().Skip(DefaultButton - 1).FirstOrDefault() is UIButton button)
                        button.SimulateClick();
                    p.Use();
                }
            }
        }

        protected virtual void Close() => HideModal(this);

        public void ToScreenCenter()
        {
            var view = GetUIView();
            relativePosition = new Vector3((view.fixedWidth - width) / 2, (view.fixedHeight - height) / 2);
        }
    }
    public class ScrollableContent : UIScrollablePanel
    {
        protected override void OnSizeChanged()
        {
            base.OnSizeChanged();

            foreach (var item in components)
                item.width = width - 2 * MessageBoxBase.Padding;
        }

        protected override void OnComponentAdded(UIComponent child)
        {
            base.OnComponentAdded(child);

            child.eventVisibilityChanged += OnChildVisibilityChanged;
            child.eventSizeChanged += OnChildSizeChanged;
        }

        protected override void OnComponentRemoved(UIComponent child)
        {
            base.OnComponentRemoved(child);

            child.eventVisibilityChanged -= OnChildVisibilityChanged;
            child.eventSizeChanged -= OnChildSizeChanged;
        }

        private void OnChildVisibilityChanged(UIComponent component, bool value) => FitContentChildren();
        private void OnChildSizeChanged(UIComponent component, Vector2 value) => FitContentChildren();

        private void FitContentChildren()
        {
            FitChildrenVertically();
            width = verticalScrollbar?.isVisible == true ? MessageBoxBase.DefaultWidth - verticalScrollbar.width - 3 : MessageBoxBase.DefaultWidth;
        }
    }
}
