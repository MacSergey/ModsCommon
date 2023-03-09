using ColossalFramework.UI;
using System;
using UnityEngine;

namespace ModsCommon.UI
{
    public class BoolPropertyPanel : EditorPropertyPanel, IReusable
    {
        bool IReusable.InCache { get; set; }

        private CustomUIToggle Toggle { get; set; }
        public event Action<bool> OnValueChanged;

        public bool Value 
        { 
            get => Toggle.State; 
            set => Toggle.State = value; 
        }

        public BoolPropertyPanel()
        {
            Toggle = Content.AddUIComponent<CustomUIToggle>();
            Toggle.CustomStyle();
            Toggle.OnStateChanged += ToggleStateChanged;
        }
        public override void DeInit()
        {
            base.DeInit();
            OnValueChanged = null;
        }

        private void ToggleStateChanged(bool value) => OnValueChanged?.Invoke(value);
    }
}
