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
        protected float LastPress { get; private set; }
        protected int LastPressFrame { get; private set; }
        protected bool DelayIsOut => Time.time - LastPress > 0.150f && LastPressFrame != Time.frameCount;

        public bool IsPressed
        {
            get
            {
                var value = InputKey.value;
                var keyCode = (KeyCode)(value & 0xFFFFFFF);

                if (!DelayIsOut || (CanRepeat ? !Input.GetKey(keyCode) : !Input.GetKeyUp(keyCode)))
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
            if ((CanRepeat ? e.type == EventType.keyDown : e.type == EventType.KeyUp) && IsPressed)
            {
                Press();
                e.Use();
                LastPress = Time.time;
                LastPressFrame = Time.frameCount;
                return true;
            }
            else
                return false;
        }
        public void Press() => Action?.Invoke();

        public override string ToString() => InputKey.GetLocale();
        public static implicit operator string (Shortcut shortcut) => shortcut.ToString();
    }

    public abstract class ModShortcut<TypeMod> : Shortcut
        where TypeMod : BaseMod<TypeMod>
    {
        public override string Label => SingletonMod<TypeMod>.Instance.GetLocalizeString(LabelKey);
        public ModShortcut(string name, string labelKey, InputKey key, Action action = null) : base(BaseSettings<TypeMod>.SettingsFile, name, labelKey, key, action) { }
    }
}
