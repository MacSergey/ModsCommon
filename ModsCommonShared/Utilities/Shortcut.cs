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
        public Shortcut(string fileName, string name, string labelKey, InputKey key, Action action = null)
        {
            LabelKey = labelKey;
            InputKey = new SavedInputKey(name, fileName, key, true);
            Action = action;
        }

        public bool IsKeyUp => InputKey.IsKeyUp();
        public virtual bool Press(Event e)
        {
            if (IsKeyUp)
            {
                Press();
                return true;
            }
            else
                return false;
        }
        public void Press() => Action?.Invoke();

        public override string ToString() => InputKey.ToLocalizedString("KEYNAME");
    }
}
