using ColossalFramework;
using System;
using UnityEngine;

namespace ModsCommon.Utilities
{
    public class Shortcut
    {
        public virtual string Label => LabelKey;
        protected string LabelKey { get; }
        public SavedInputKey InputKey { get; }
        private Action Action { get; }
        public bool CanRepeat { get; set; } = false;
        public bool IgnoreModifiers { get; set; } = false;

        public bool NotSet => InputKey.Key == KeyCode.None;
        private DateTime LastPress { get; set; }
        private bool DelayIsOut => (DateTime.UtcNow - LastPress).TotalMilliseconds > 150f;

        public bool IsPressed
        {
            get
            {
                var value = InputKey.value;
                var keyCode = (KeyCode)(value & 0xFFFFFFF);

                //only modifier press?
                //if (keyCode == KeyCode.None)
                //    return false;

                if (CanRepeat ? !DelayIsOut || !Input.GetKey(keyCode) : (!Input.GetKeyUp(keyCode)))
                    return false;

                if (!IgnoreModifiers)
                {
                    if ((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) != ((value & 0x40000000) != 0))
                        return false;

                    if ((Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) != ((value & 0x20000000) != 0))
                        return false;

                    if ((Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt) || Input.GetKey(KeyCode.AltGr)) != ((value & 0x10000000) != 0))
                        return false;
                }

                return true;
            }
        }

        public Shortcut(string fileName, string name, string labelKey, InputKey key, Action action = null)
        {
            LabelKey = labelKey;
            InputKey = new SavedInputKey(name, fileName, key, true);
            Action = action;
        }

        public virtual bool Press(Event e)
        {
            if (e.type != EventType.Used && IsPressed)
            {
                Press();
                e.Use();
                LastPress = DateTime.UtcNow;
                return true;
            }
            else
                return false;
        }
        public void Press() => Action?.Invoke();

        public override string ToString() => InputKey.GetLocale();
    }

    public abstract class ModShortcut<TypeMod> : Shortcut
        where TypeMod : BaseMod<TypeMod>
    {
        public override string Label => SingletonMod<TypeMod>.Instance.GetLocalizeString(LabelKey);
        public ModShortcut(string name, string labelKey, InputKey key, Action action = null) : base(BaseSettings<TypeMod>.SettingsFile, name, labelKey, key, action) { }
    }
}
