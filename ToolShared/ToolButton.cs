using ColossalFramework.UI;
using UnityEngine;

namespace ModsCommon.UI
{
    public abstract class NetToolButton<TypeTool> : CustomUIButton
        where TypeTool : ITool
    {
        private static int ButtonSize => 31;
        protected abstract Vector2 ButtonPosition { get; }
        protected abstract UITextureAtlas DefaultAtlas { get; }
        protected virtual SpriteSet DefaultBgSprite { get; }
        protected virtual SpriteSet DefaultFgSprite { get; }

        public override void Start()
        {
            Atlas = DefaultAtlas;

            BgSprites = DefaultBgSprite;
            FgSprites = DefaultFgSprite;

            relativePosition = ButtonPosition;
            size = new Vector2(ButtonSize, ButtonSize);
        }
        public override void Update()
        {
            base.Update();

            var enable = SingletonTool<TypeTool>.Instance?.enabled == true;

            if (enable && (State == UIButton.ButtonState.Normal || State == UIButton.ButtonState.Hovered))
                State = UIButton.ButtonState.Focused;
            else if (!enable && State == UIButton.ButtonState.Focused)
                State = UIButton.ButtonState.Normal;
        }

        protected override void OnClick(UIMouseEventParameter p)
        {
            SingletonTool<TypeTool>.Logger.Debug($"On button click");

            base.OnClick(p);
            SingletonTool<TypeTool>.Instance.Toggle();
        }
        protected override void OnTooltipEnter(UIMouseEventParameter p)
        {
            tooltip = SingletonTool<TypeTool>.Instance.ToolTip;
            base.OnTooltipEnter(p);
        }
    }
}
