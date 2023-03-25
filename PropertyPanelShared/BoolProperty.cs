using ColossalFramework.UI;
using System;
using UnityEngine;

namespace ModsCommon.UI
{
    public class BoolPropertyPanel : EditorPropertyPanel, IReusable
    {
        private CustomUIToggle Toggle { get; set; }
        public event Action<bool> OnValueChanged;

        public bool Value 
        { 
            get => Toggle.State; 
            set => Toggle.State = value; 
        }

        public BoolPropertyPanel()
        {

        }
        protected override void FillContent()
        {
            Toggle = Content.AddUIComponent<CustomUIToggle>();
            Toggle.name = nameof(Toggle);
            Toggle.DefaultStyle();
            Toggle.OnStateChanged += ToggleStateChanged;
        }
        public override void DeInit()
        {
            base.DeInit();
            OnValueChanged = null;
        }

        private void ToggleStateChanged(bool value) => OnValueChanged?.Invoke(value);

        public override void SetStyle(ControlStyle style)
        {
            Toggle.SetStyle(style.Toggle);
        }
    }
}
