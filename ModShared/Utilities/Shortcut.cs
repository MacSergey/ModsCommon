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
        public bool NotSet => InputKey.Key == KeyCode.None;
        private DateTime LastPress { get; set; }

        public Shortcut(string fileName, string name, string labelKey, InputKey key, Action action = null)
        {
            LabelKey = labelKey;
            InputKey = new SavedInputKey(name, fileName, key, true);
            Action = action;
        }

        public bool IsKeyUp => CanRepeat ? (InputKey.IsPressed() && (DateTime.UtcNow - LastPress).TotalMilliseconds > 150f) : InputKey.IsKeyUp();
        public virtual bool Press(Event e)
        {
            if (e.type != EventType.Used && IsKeyUp)
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

        public override string ToString() => InputKey.ToLocalizedString("KEYNAME");
    }

    public abstract class ModShortcut<TypeMod> : Shortcut
        where TypeMod : BaseMod<TypeMod>
    {
        public override string Label => SingletonMod<TypeMod>.Instance.GetLocalizeString(LabelKey);
        public ModShortcut(string name, string labelKey, InputKey key, Action action = null) : base(BaseSettings<TypeMod>.SettingsFile, name, labelKey, key, action) { }
    }
}
