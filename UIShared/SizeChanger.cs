using ColossalFramework.UI;
using ModsCommon.Utilities;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace ModsCommon.UI
{
    public class SizeChanger : CustomUIPanel
    {
        private bool InProgress { get; set; }

        private UIComponent _target;
        public UIComponent Target
        {
            get => _target;
            set
            {
                if (_target != null)
                    _target.eventSizeChanged -= TargetSizeChanged;

                _target = value;

                if (_target != null)
                    _target.eventSizeChanged += TargetSizeChanged;
            }
        }

        private void TargetSizeChanged(UIComponent component, Vector2 value) => SetPosition();

        public SizeChanger()
        {
            size = new Vector2(9, 9);
            atlas = CommonTextures.Atlas;
            backgroundSprite = CommonTextures.ResizeSprite;
            color = new Color32(255, 255, 255, 160);

            var handle = AddUIComponent<CustomUIDragHandle>();
            handle.size = size;
            handle.relativePosition = Vector2.zero;
            handle.target = this;
        }
        public override void Start()
        {
            base.Start();
            Target ??= parent;
        }
        private void SetPosition() => relativePosition = Target.size - size;
        protected override void OnPositionChanged()
        {
            if (!InProgress)
            {
                InProgress = true;
                base.OnPositionChanged();
                Target.size = (Vector2)relativePosition + size;
                SetPosition();
                InProgress = false;
            }
        }
    }
}
