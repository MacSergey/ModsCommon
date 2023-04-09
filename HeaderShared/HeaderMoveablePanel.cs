using ColossalFramework.UI;
using UnityEngine;

namespace ModsCommon.UI
{
    public abstract class HeaderMoveablePanel<TypeContent> : CustomUIPanel
        where TypeContent : BaseHeaderContent
    {
        protected bool CanMove
        {
            get
            {
                var view = GetUIView();
                var mousePosition = view.ScreenPointToGUI(Input.mousePosition / view.inputScale);
                return !new Rect(Content.absolutePosition, Content.size).Contains(mousePosition);
            }
        }
        private bool Move { get; set; }

        protected TypeContent Content { get; set; }
        protected CustomUILabel Caption { get; private set; }
        public UIComponent Target { get; set; }
        private Vector3 LastPosition { get; set; }

        public string Text
        {
            get => Caption.text;
            set => Caption.text = value;
        }
        public HeaderStyle ContentStyle { set => Content.HeaderStyle = value; }

        public HeaderMoveablePanel() : base()
        {
            PauseLayout(() =>
            {
                AutoLayout = AutoLayout.Horizontal;
                AutoLayoutStart = LayoutStart.MiddleLeft;

                PlaceItems();
            });
        }
        protected virtual void PlaceItems()
        {
            Caption = AddUIComponent<CustomUILabel>();
            Caption.AutoSize = AutoSize.Height;
            Caption.Padding.top = 5;
            Caption.HorizontalAlignment = UIHorizontalAlignment.Center;
            Caption.VerticalAlignment = UIVerticalAlignment.Middle;

            Content = AddUIComponent<TypeContent>();
            Content.AutoChildrenVertically = AutoLayoutChildren.Fit;
            Content.AutoChildrenHorizontally = AutoLayoutChildren.Fit;
            Content.Padding.right = 5;
            Content.PauseLayout(FillContent);
            Content.eventSizeChanged += ContentSizeChanged;
        }
        protected abstract void FillContent();
        protected virtual void Init(float? height)
        {
            size = new Vector2(parent.width, height ?? 42);
            Refresh();
        }
        public override void Start()
        {
            base.Start();

            Target ??= parent;

            if (size.magnitude <= float.Epsilon)
            {
                if (parent != null)
                {
                    size = new Vector2(Target.width, 30f);
                    anchor = UIAnchorStyle.Top | UIAnchorStyle.Left | UIAnchorStyle.Right;
                    relativePosition = Vector2.zero;
                }
                else
                    size = new Vector2(200f, 25f);
            }
        }
        protected override void OnSizeChanged()
        {
            base.OnSizeChanged();
            SetCaption();
        }
        private void ContentSizeChanged(UIComponent component, Vector2 value) => SetCaption();
        private void SetCaption() => Caption.width = width - Content.width;

        public virtual void Refresh() => Content.Refresh();

        protected override void OnMouseDown(UIMouseEventParameter p)
        {
            Move = CanMove;

            (Target ?? GetRootContainer()).BringToFront();

            p.Use();
            var plane = new Plane(Target.transform.TransformDirection(Vector3.back), Target.transform.position);
            plane.Raycast(p.ray, out var enter);
            LastPosition = p.ray.origin + p.ray.direction * enter;
            base.OnMouseDown(p);
        }
        protected override void OnMouseMove(UIMouseEventParameter p)
        {
            p.Use();

            if (Move && p.buttons.IsFlagSet(UIMouseButton.Left))
            {
                var view = GetUIView();
                var pixelsToUnits = view.PixelsToUnits();

                var inNormal = view.uiCamera.transform.TransformDirection(Vector3.back);
                new Plane(inNormal, LastPosition).Raycast(p.ray, out var enter);
                var vector = (p.ray.origin + p.ray.direction * enter).Quantize(pixelsToUnits);
                var corners = GetUIView().GetCorners();
                var vector3 = (Target.transform.position + vector - LastPosition).Quantize(pixelsToUnits);
                var vector4 = Target.pivot.TransformToUpperLeft(Target.size, Target.arbitraryPivotOffset);
                var vector5 = vector4 + new Vector3(Target.size.x, 0f - Target.size.y);
                vector4 *= pixelsToUnits;
                vector5 *= pixelsToUnits;

                if (vector3.x + vector4.x < corners[0].x)
                    vector3.x = corners[0].x - vector4.x;

                if (vector3.x + vector5.x > corners[1].x)
                    vector3.x = corners[1].x - vector5.x;

                if (vector3.y + vector4.y > corners[0].y)
                    vector3.y = corners[0].y - vector4.y;

                if (vector3.y + vector5.y < corners[2].y)
                    vector3.y = corners[2].y - vector5.y;

                Target.transform.position = vector3;
                LastPosition = vector;
            }
            base.OnMouseMove(p);
        }
        protected override void OnMouseUp(UIMouseEventParameter p)
        {
            base.OnMouseUp(p);
            Target.MakePixelPerfect();
        }
    }
}
