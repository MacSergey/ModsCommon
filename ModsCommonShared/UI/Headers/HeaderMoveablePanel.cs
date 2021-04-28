using ColossalFramework.UI;
using UnityEngine;

namespace ModsCommon.UI
{
    public abstract class HeaderMoveablePanel<TypeContent> : BaseHeaderPanel<TypeContent>
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

        private CustomUILabel Caption { get; set; }
        public UIComponent Target { get; set; }
        private Vector3 LastPosition { get; set; }

        public string Text
        {
            get => Caption.text;
            set => Caption.text = value;
        }

        public HeaderMoveablePanel()
        {
            CreateCaption();
        }
        public override void Init()
        {
            base.Init();
            CaptionSizeChanged();
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

        private void CreateCaption()
        {
            Caption = AddUIComponent<CustomUILabel>();
            Caption.zOrder = 0;
            Caption.autoSize = false;
            Caption.autoHeight = true;
            Caption.textAlignment = UIHorizontalAlignment.Center;
            Caption.verticalAlignment = UIVerticalAlignment.Middle;
            Caption.eventSizeChanged += (_, _) => CaptionSizeChanged();
        }

        private void CaptionSizeChanged() => Caption.relativePosition = new Vector2(10, (height - Caption.height) / 2);

        protected override void OnSizeChanged()
        {
            base.OnSizeChanged();

            Content.autoLayout = true;
            Content.autoLayout = false;
            Content.FitChildrenHorizontally();
            Content.height = height;

            foreach (var item in Content.components)
                item.relativePosition = new Vector2(item.relativePosition.x, (Content.height - item.height) / 2);

            Caption.width = width - Content.width - 20;
            Content.relativePosition = new Vector2(Caption.width - 5 + 20, (height - Content.height) / 2);
        }

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
