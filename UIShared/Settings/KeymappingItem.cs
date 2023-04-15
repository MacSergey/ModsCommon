using ModsCommon.Utilities;
using System;
using ColossalFramework.UI;
using UnityEngine;
using ColossalFramework;

namespace ModsCommon.UI
{
    public class KeymappingUIButton : CustomUIButton
    {
        public Action StartBinding;
        protected override void OnMouseDown(UIMouseEventParameter p)
        {
            base.OnMouseDown(p);
            StartBinding?.Invoke();
        }
    }
    public class KeymappingSettingsItem : ControlSettingsItem<KeymappingUIButton>
    {
        public event Action<Shortcut> BindingChanged;

        protected override RectOffset ItemsPadding => new RectOffset(20, 20, 10, 10);
        private static WarningLabel Warning { get; set; }
        private static bool InProgress => Warning != null;

        private Shortcut shortcut;
        public Shortcut Shortcut
        {
            get => shortcut;
            set
            {
                if (shortcut != value)
                {
                    shortcut = value;
                    Label = shortcut.Label;
                    Control.text = shortcut.ToString();
                }
            }
        }

        protected override void InitControl()
        {
            base.InitControl();

            Control.ButtonSettingsStyle();
            Control.size = new Vector2(278f, 31f);
            Control.StartBinding += OnStartBinding;
        }
        private void OnStartBinding()
        {
            if (!InProgress)
            {
                Control.text = CommonLocalize.Settings_PressAnyKey;

                Warning = MessageBox.Show<WarningLabel>();
                Warning.Shortcut = Shortcut;
                Warning.OnBinding += OnBinding;
            }
        }
        private void OnBinding(KeyCode key, bool ctrl, bool shift, bool alt)
        {
            if (Shortcut is Shortcut shortcut)
            {
                MessageBox.Hide(Warning);
                Warning = null;

                if (key == KeyCode.Backspace)
                    shortcut.InputKey.value = SavedInputKey.Empty;
                else if (key != KeyCode.Escape)
                {
                    if (shortcut.IgnoreModifiers)
                        shortcut.InputKey.value = SavedInputKey.Encode(key, false, false, false);
                    else
                        shortcut.InputKey.value = SavedInputKey.Encode(key, ctrl, shift, alt);
                }

                Control.text = shortcut.InputKey.GetLocale();

                BindingChanged?.Invoke(Shortcut);
            }
        }

        private class WarningLabel : CustomUIPanel
        {
            public event Action<KeyCode, bool, bool, bool> OnBinding;

            private CustomUILabel ShortcutTitle { get; set; }
            public Shortcut Shortcut
            {
                set
                {
                    ShortcutTitle.text = value.Label;
                    ShortcutTitle.AutoSize = AutoSize.All;
                }
            }

            public WarningLabel()
            {
                canFocus = true;

                ShortcutTitle = AddUIComponent<CustomUILabel>();
                ShortcutTitle.Atlas = CommonTextures.Atlas;
                ShortcutTitle.BackgroundSprite = CommonTextures.PanelBig;
                ShortcutTitle.color = ComponentStyle.SettingsColor50;
                ShortcutTitle.Padding = new RectOffset(30, 30, 0, 5);
                ShortcutTitle.AutoSize = AutoSize.All;
                ShortcutTitle.textScale = 3f;

                var pressAnyKey = AddUIComponent<CustomUILabel>();
                pressAnyKey.Bold = true;
                pressAnyKey.text = CommonLocalize.Settings_PressAnyKey.ToUpper();
                pressAnyKey.textScale = 7f;
                pressAnyKey.Padding = new RectOffset(0, 0, 15, 30);
                pressAnyKey.AutoSize = AutoSize.All;

                var pressEsc = AddUIComponent<CustomUILabel>();
                pressEsc.text = $"{string.Format(CommonLocalize.Settings_DiscardShortcut, LocalizeExtension.Esc.ToUpper())}\n{string.Format(CommonLocalize.Settings_ResetShortcut, LocalizeExtension.Backspace.ToUpper())}";
                pressEsc.Atlas = CommonTextures.Atlas;
                pressEsc.BackgroundSprite = CommonTextures.PanelBig;
                pressEsc.color = ComponentStyle.SettingsColor20;
                pressEsc.textColor = ComponentStyle.SettingsColor90;
                pressEsc.textScale = 1f;
                pressEsc.Padding = new RectOffset(15, 15, 7, 7);
                pressEsc.HorizontalAlignment = UIHorizontalAlignment.Center;
                pressEsc.AutoSize = AutoSize.All;

                Atlas = CommonTextures.Atlas;
                BackgroundSprite = CommonTextures.PanelLarge;
                BgColors = ComponentStyle.SettingsColor25;
                AutoChildrenVertically = AutoLayoutChildren.Fit;
                AutoChildrenHorizontally = AutoLayoutChildren.Fit;
                AutoLayoutStart = LayoutStart.TopCentre;
                Padding = new RectOffset(50, 50, 10, 10);
                AutoLayout = AutoLayout.Vertical;

                var res = GetUIView().GetScreenResolution();
                relativePosition = new Vector3((res.x - width) * 0.5f, (res.y - height) * 0.5f);
            }

            public override void Update()
            {
                base.Update();

                if (OnBinding != null)
                {
                    var e = Event.current;

                    if (Input.GetMouseButtonDown(3))
                        OnBinding(KeyCode.Mouse3, e.control, e.shift, e.alt);
                    else if (Input.GetMouseButtonDown(4))
                        OnBinding(KeyCode.Mouse4, e.control, e.shift, e.alt);
                    else if (Input.GetMouseButtonDown(5))
                        OnBinding(KeyCode.Mouse5, e.control, e.shift, e.alt);
                    else if (Input.GetMouseButtonDown(6))
                        OnBinding(KeyCode.Mouse6, e.control, e.shift, e.alt);
                }
            }
            private void OnGUI()
            {
                var e = Event.current;

                if (e.type == EventType.KeyDown && OnBinding != null)
                {
                    if (e.keyCode != KeyCode.LeftControl && e.keyCode != KeyCode.RightControl && e.keyCode != KeyCode.LeftShift && e.keyCode != KeyCode.RightShift && e.keyCode != KeyCode.LeftAlt && e.keyCode != KeyCode.RightAlt)
                        OnBinding(e.keyCode, e.control, e.shift, e.alt);
                }
            }
        }
    }
}
