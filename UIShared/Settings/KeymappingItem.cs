using ModsCommon.Utilities;
using System;
using ColossalFramework.UI;
using UnityEngine;
using ColossalFramework;

namespace ModsCommon.UI
{
    public class KeymappingSettingsItem : ControlSettingsItem<CustomUIButton>
    {
        public event Action<Shortcut> BindingChanged;

        private Shortcut shortcut;
        public Shortcut Shortcut 
        {
            get => shortcut;
            set
            {
                if(shortcut != value)
                {
                    shortcut = value;
                    Label = shortcut.Label;
                    Control.text = shortcut.ToString();
                }
            }
        }
        private bool InProgress { get; set; }

        protected override void InitControl()
        {
            base.InitControl();

            Control.CustomSettingsStyle();
            Control.size = new Vector2(278f, 31f);

            Control.eventKeyDown += OnBindingKeyDown;
            Control.eventMouseDown += OnBindingMouseDown;
        }

        private void OnBindingKeyDown(UIComponent comp, UIKeyEventParameter p)
        {
            if (Shortcut is Shortcut shortcut && !IsModifierKey(p.keycode))
            {
                p.Use();
                InProgress = false;
                UIView.PopModal();

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
                Control.buttonsMask = UIMouseButton.Left | UIMouseButton.Right | UIMouseButton.Middle | UIMouseButton.Special0 | UIMouseButton.Special1 | UIMouseButton.Special2 | UIMouseButton.Special3;
                Control.text = CommonLocalize.Settings_PressAnyKey;
                Control.Focus();

                p.Use();
                InProgress = true;
                UIView.PushModal(Control);
            }
            else if (!IsUnbindableMouseButton(p.buttons))
            {
                p.Use();
                InProgress = false;
                UIView.PopModal();

                if (Shortcut.IgnoreModifiers)
                    Shortcut.InputKey.value = SavedInputKey.Encode(ButtonToKeycode(p.buttons), false, false, false);
                else
                    Shortcut.InputKey.value = SavedInputKey.Encode(ButtonToKeycode(p.buttons), Utility.CtrlIsPressed, Utility.ShiftIsPressed, Utility.AltIsPressed);

                Control.text = Shortcut.InputKey.GetLocale();
                Control.buttonsMask = UIMouseButton.Left;

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
    }
}
