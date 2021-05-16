using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.UI;
using ModsCommon.Utilities;
using UnityEngine;

namespace ModsCommon.UI
{
    public class KeymappingsPanel : UICustomControl
    {
        private SavedInputKey EditBinding { get; set; }
        private int count;

        public void AddKeymapping(Shortcut shortcut)
        {
            var panel = component.AttachUIComponent(UITemplateManager.GetAsGameObject("KeyBindingTemplate")) as UIPanel;

            if (count % 2 == 1)
                panel.backgroundSprite = null;

            count += 1;

            var label = panel.Find<UILabel>("Name");
            label.text = shortcut.Label;

            var button = panel.Find<UIButton>("Binding");
            button.eventKeyDown += OnBindingKeyDown;
            button.eventMouseDown += OnBindingMouseDown;
            button.text = shortcut.ToString();
            button.objectUserData = shortcut.InputKey;
        }

        private void OnBindingKeyDown(UIComponent comp, UIKeyEventParameter p)
        {
            if (EditBinding != null && !IsModifierKey(p.keycode))
            {
                p.Use();
                UIView.PopModal();

                if (p.keycode == KeyCode.Backspace)
                    EditBinding.value = SavedInputKey.Empty;
                else if (p.keycode != KeyCode.Escape)
                    EditBinding.value = SavedInputKey.Encode(p.keycode, p.control, p.shift, p.alt);

                (p.source as UITextComponent).text = EditBinding.ToLocalizedString("KEYNAME");
                EditBinding = null;
            }
        }
        private void OnBindingMouseDown(UIComponent comp, UIMouseEventParameter p)
        {
            if (EditBinding == null)
            {
                p.Use();
                EditBinding = (SavedInputKey)p.source.objectUserData;
                var button = p.source as UIButton;
                button.buttonsMask = UIMouseButton.Left | UIMouseButton.Right | UIMouseButton.Middle | UIMouseButton.Special0 | UIMouseButton.Special1 | UIMouseButton.Special2 | UIMouseButton.Special3;
                button.text = Locale.Get("KEYMAPPING_PRESSANYKEY");
                p.source.Focus();
                UIView.PushModal(p.source);
            }
            else if (!IsUnbindableMouseButton(p.buttons))
            {
                p.Use();
                UIView.PopModal();
                EditBinding.value = SavedInputKey.Encode(ButtonToKeycode(p.buttons), Utilites.CtrlIsPressed, Utilites.ShiftIsPressed, Utilites.AltIsPressed);
                var button = p.source as UIButton;
                button.text = EditBinding.ToLocalizedString("KEYNAME");
                button.buttonsMask = UIMouseButton.Left;
                EditBinding = null;
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
