using ModsCommon.Utilities;
using System;
using ColossalFramework.UI;
using UnityEngine;
using ColossalFramework;
using IMT.Utilities;

namespace ModsCommon.UI
{
    public class KeymappingSettingsItem : ControlSettingsItem<CustomUIButton>
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

            Control.eventKeyDown += OnBindingKeyDown;
            Control.eventMouseDown += OnBindingMouseDown;
        }

        private void OnBindingKeyDown(UIComponent comp, UIKeyEventParameter p)
        {
            if (Shortcut is Shortcut shortcut && !IsModifierKey(p.keycode))
            {
                p.Use();
                MessageBox.Hide(Warning);
                Warning = null;

                if (p.keycode == KeyCode.Backspace)
                    shortcut.InputKey.value = SavedInputKey.Empty;
                else if (p.keycode != KeyCode.Escape)
                {
                    if (shortcut.IgnoreModifiers)
                        shortcut.InputKey.value = SavedInputKey.Encode(p.keycode, false, false, false);
                    else
                        shortcut.InputKey.value = SavedInputKey.Encode(p.keycode, p.control, p.shift, p.alt);
                }

                Control.text = shortcut.InputKey.GetLocale();

                BindingChanged?.Invoke(Shortcut);
            }
        }
        private void OnBindingMouseDown(UIComponent comp, UIMouseEventParameter p)
        {
            if (!InProgress)
            {
                Control.ButtonsMask = UIMouseButton.Left | UIMouseButton.Right | UIMouseButton.Middle | UIMouseButton.Special0 | UIMouseButton.Special1 | UIMouseButton.Special2 | UIMouseButton.Special3;
                Control.text = CommonLocalize.Settings_PressAnyKey;

                p.Use();
                Warning = MessageBox.Show<WarningLabel>();
                Warning.Shortcut = Shortcut;
                Warning.Focus();
            }
            else if (!IsUnbindableMouseButton(p.buttons))
            {
                p.Use();
                MessageBox.Hide(Warning);
                Warning = null;

                if (Shortcut.IgnoreModifiers)
                    Shortcut.InputKey.value = SavedInputKey.Encode(ButtonToKeycode(p.buttons), false, false, false);
                else
                    Shortcut.InputKey.value = SavedInputKey.Encode(ButtonToKeycode(p.buttons), Utility.CtrlIsPressed, Utility.ShiftIsPressed, Utility.AltIsPressed);

                Control.text = Shortcut.InputKey.GetLocale();
                Control.ButtonsMask = UIMouseButton.Left;

                BindingChanged?.Invoke(Shortcut);
            }
        }

        private KeyCode ButtonToKeycode(UIMouseButton button) => button switch
        {
            UIMouseButton.Left => KeyCode.Mouse0,
            UIMouseButton.Right => KeyCode.Mouse1,
            UIMouseButton.Middle => KeyCode.Mouse2,
            UIMouseButton.Special0 => KeyCode.Mouse3,
            UIMouseButton.Special1 => KeyCode.Mouse4,
            UIMouseButton.Special2 => KeyCode.Mouse5,
            UIMouseButton.Special3 => KeyCode.Mouse6,
            _ => KeyCode.None,
        };

        private bool IsModifierKey(KeyCode code) => code == KeyCode.LeftControl || code == KeyCode.RightControl || code == KeyCode.LeftShift || code == KeyCode.RightShift || code == KeyCode.LeftAlt || code == KeyCode.RightAlt;
        private bool IsUnbindableMouseButton(UIMouseButton code) => code == UIMouseButton.Left || code == UIMouseButton.Right;

        private class WarningLabel : CustomUIPanel
        {
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
                ShortcutTitle = AddUIComponent<CustomUILabel>();
                ShortcutTitle.Atlas = CommonTextures.Atlas;
                ShortcutTitle.BackgroundSprite = CommonTextures.PanelBig;
                ShortcutTitle.color = ComponentStyle.SettingsColor50;
                ShortcutTitle.Padding = new RectOffset(30, 30, 0, 5);
                ShortcutTitle.autoSize = true;
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
                color = ComponentStyle.SettingsColor25;
                AutoChildrenVertically = AutoLayoutChildren.Fit;
                AutoChildrenHorizontally = AutoLayoutChildren.Fit;
                AutoLayoutStart = LayoutStart.TopCentre;
                Padding = new RectOffset(50, 50, 10, 10);
                AutoLayout = AutoLayout.Vertical;

                var res = GetUIView().GetScreenResolution();
                relativePosition = new Vector3((res.x - width) * 0.5f, (res.y - height) * 0.5f);
            }
        }
    }
}
