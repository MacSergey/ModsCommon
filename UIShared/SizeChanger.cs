using ColossalFramework.UI;
using ModsCommon.Utilities;
using UnityEngine;

namespace ModsCommon.UI
{
    public class SizeChanger : CustomUIPanel
    {
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
        public bool HasTarget => _target != null;

        private Vector2 LastPosition { get; set; }
        private Vector2 LastSize { get; set; }
        private Vector2 CurrentPosition
        {
            get
            {
                var uiView = UIView.GetAView();
                return uiView.ScreenPointToGUI(Input.mousePosition / uiView.inputScale);
            }
        }

        private void TargetSizeChanged(UIComponent component, Vector2 value) => SetPosition();

        public SizeChanger()
        {
            size = new Vector2(9, 9);
            Atlas = CommonTextures.Atlas;
            BackgroundSprite = CommonTextures.Resize;
            BgColors = new Color32(255, 255, 255, 160);
        }
        public override void Start()
        {
            base.Start();
            Target ??= parent;
        }
        private void SetPosition() => relativePosition = Target.size - size;

        protected override void OnMouseDown(UIMouseEventParameter p)
        {
            p.Use();
            LastPosition = CurrentPosition;

            if (Target is UIComponent target)
            {
                target.BringToFront();
                LastSize = target.size;
            }
            else
            {
                GetRootContainer().BringToFront();
                LastSize = Vector2.zero;
            }

            base.OnMouseDown(p);
        }
        protected override void OnMouseUp(UIMouseEventParameter p)
        {
            base.OnMouseUp(p);
            Target?.MakePixelPerfect();
            SetPosition();
        }
        protected override void OnMouseMove(UIMouseEventParameter p)
        {
            p.Use();
            if (p.buttons.IsFlagSet(UIMouseButton.Left) && Target is UIComponent target)
            {
                var delta = CurrentPosition - LastPosition;
                target.size = LastSize + delta;
            }
            base.OnMouseMove(p);
        }
    }
}
