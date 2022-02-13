using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.UI;
using ModsCommon.Utilities;
using System;
using System.Linq;
using UnityEngine;

namespace ModsCommon.UI
{
    public class KeymappingsPanel : UICustomControl
    {
        public event Action<Shortcut> BindingChanged;

        private static Shortcut EditShortcut { get; set; }

        public void AddKeymapping(Shortcut shortcut)
        {
            var panel = component.AttachUIComponent(UITemplateManager.GetAsGameObject("KeyBindingTemplate")) as UIPanel;

            if (component.components.Count % 2 == 0)
                panel.backgroundSprite = null;

            if (panel.Find<UILabel>("Name") is UILabel label)
                label.text = shortcut.Label;

            if (panel.Find<UIButton>("Binding") is UIButton button)
            {
                button.eventKeyDown += OnBindingKeyDown;
                button.eventMouseDown += OnBindingMouseDown;
                button.text = shortcut.ToString();
                button.objectUserData = shortcut;
            }
        }

        private void OnBindingKeyDown(UIComponent comp, UIKeyEventParameter p)
        {
            if (EditShortcut != null && !IsModifierKey(p.keycode))
            {
                p.Use();
                UIView.PopModal();

                if (p.keycode == KeyCode.Backspace)
                    EditShortcut.InputKey.value = SavedInputKey.Empty;
                else if (p.keycode != KeyCode.Escape)
                {
                    if (EditShortcut.IgnoreModifiers)
                        EditShortcut.InputKey.value = SavedInputKey.Encode(p.keycode, false, false, false);
                    else
                        EditShortcut.InputKey.value = SavedInputKey.Encode(p.keycode, p.control, p.shift, p.alt);
                }

                (p.source as UITextComponent).text = EditShortcut.InputKey.GetLocale();

                BindingChanged?.Invoke(EditShortcut);
                EditShortcut = null;
            }
        }
        private void OnBindingMouseDown(UIComponent comp, UIMouseEventParameter p)
        {
            if (EditShortcut == null)
            {
                p.Use();
                EditShortcut = (Shortcut)p.source.objectUserData;
                var button = p.source as UIButton;
                button.buttonsMask = UIMouseButton.Left | UIMouseButton.Right | UIMouseButton.Middle | UIMouseButton.Special0 | UIMouseButton.Special1 | UIMouseButton.Special2 | UIMouseButton.Special3;
                button.text = CommonLocalize.Settings_PressAnyKey;
                p.source.Focus();
                UIView.PushModal(p.source);
            }
            else if (!IsUnbindableMouseButton(p.buttons))
            {
                p.Use();
                UIView.PopModal();

                if (EditShortcut.IgnoreModifiers)
                    EditShortcut.InputKey.value = SavedInputKey.Encode(ButtonToKeycode(p.buttons), false, false, false);
                else
                    EditShortcut.InputKey.value = SavedInputKey.Encode(ButtonToKeycode(p.buttons), Utility.CtrlIsPressed, Utility.ShiftIsPressed, Utility.AltIsPressed);

                var button = p.source as UIButton;
                button.text = EditShortcut.InputKey.GetLocale();
                button.buttonsMask = UIMouseButton.Left;

                BindingChanged?.Invoke(EditShortcut);
                EditShortcut = null;
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
