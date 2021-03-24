using ColossalFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ModsCommon.Utils
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

        public virtual bool IsPressed(Event e)
        {
            if (InputKey.IsPressed(e))
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
