using ColossalFramework.UI;
using ModsCommon.Utilities;
using UnityEngine;

namespace ModsCommon.UI
{
    public abstract class NetToolButton<TypeTool> : CustomUIButton
        where TypeTool : ITool
    {
        private static int ButtonSize => 31;
        protected abstract Vector2 ButtonPosition { get; }
        protected abstract UITextureAtlas Atlas { get; }
        protected virtual string NormalBgSprite { get; }
        protected virtual string HoveredBgSprite { get; }
        protected virtual string PressedBgSprite { get; }
        protected virtual string FocusedBgSprite { get; }
        protected virtual string NormalFgSprite { get; }
        protected virtual string HoveredFgSprite { get; }
        protected virtual string PressedFgSprite { get; }
        protected virtual string FocusedFgSprite { get; }

        public override void Start()
        {
            atlas = Atlas;

            normalBgSprite = NormalBgSprite;
            hoveredBgSprite = HoveredBgSprite;
            pressedBgSprite = PressedBgSprite;
            focusedBgSprite = FocusedBgSprite;

            normalFgSprite = NormalFgSprite;
            hoveredFgSprite = HoveredFgSprite;
            pressedFgSprite = PressedFgSprite;
            focusedFgSprite = FocusedFgSprite;

            relativePosition = ButtonPosition;
            size = new Vector2(ButtonSize, ButtonSize);
        }
        public override void Update()
        {
            base.Update();

            var enable = SingletonTool<TypeTool>.Instance?.enabled == true;

            if (enable && (state == ButtonState.Normal || state == ButtonState.Hovered))
                state = ButtonState.Focused;
            else if (!enable && state == ButtonState.Focused)
                state = ButtonState.Normal;
        }

        protected override void OnClick(UIMouseEventParameter p)
        {
            SingletonTool<TypeTool>.Logger.Debug($"On button click");

            base.OnClick(p);
            SingletonTool<TypeTool>.Instance.Toggle();
        }
        protected override void OnTooltipEnter(UIMouseEventParameter p)
        {
            tooltip = $"{SingletonTool<TypeTool>.Instance.ModInstance.NameRaw} ({SingletonTool<TypeTool>.Activation})";
            base.OnTooltipEnter(p);
        }
    }
}
