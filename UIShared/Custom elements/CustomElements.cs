using ColossalFramework.UI;
using UnityEngine;
using static ColossalFramework.UI.UIButton;

namespace ModsCommon.UI
{
    public class CustomUIPanel : UIPanel
    {
        private Vector3 positionBefore;
        public override void ResetLayout() => positionBefore = relativePosition;
        public override void PerformLayout()
        {
            if ((relativePosition - positionBefore).sqrMagnitude > 0.001)
                relativePosition = positionBefore;
        }
    }
    public class CustomUIScrollablePanel : UIScrollablePanel
    {
        private Vector3 positionBefore;
        public override void ResetLayout() => positionBefore = relativePosition;
        public override void PerformLayout()
        {
            if ((relativePosition - positionBefore).sqrMagnitude > 0.001)
                relativePosition = positionBefore;
        }
    }
    public class CustomUILabel : UILabel
    {
        private Vector3 positionBefore;
        public override void ResetLayout() => positionBefore = relativePosition;
        public override void PerformLayout()
        {
            if ((relativePosition - positionBefore).sqrMagnitude > 0.001)
                relativePosition = positionBefore;
        }
    }
    public class CustomUITextField : UITextField
    {
        private Vector3 positionBefore;
        public override void ResetLayout() => positionBefore = relativePosition;
        public override void PerformLayout()
        {
            if ((relativePosition - positionBefore).sqrMagnitude > 0.001)
                relativePosition = positionBefore;
        }
    }
    public class CustomUIButton : UIButton
    {
        public Vector2 MinimumAutoSize
        {
            get
            {
                var size = minimumSize;

                if (m_Font != null && m_Font.isValid && !string.IsNullOrEmpty(m_Text))
                {
                    using (UIFontRenderer uIFontRenderer = ObtainTextRenderer())
                    {
                        Vector2 vector = uIFontRenderer.MeasureString(m_Text);
                        size = new Vector2(vector.x + textPadding.horizontal, vector.y + textPadding.vertical);
                    }
                }

                return size;
            }
        }

        private Vector3 positionBefore;
        public override void ResetLayout() => positionBefore = relativePosition;
        public override void PerformLayout()
        {
            if ((relativePosition - positionBefore).sqrMagnitude > 0.001)
                relativePosition = positionBefore;
        }

        private UIFontRenderer ObtainTextRenderer()
        {
            var uIFontRenderer = font.ObtainRenderer();
            uIFontRenderer.wordWrap = wordWrap;
            uIFontRenderer.multiLine = true;
            uIFontRenderer.maxSize = size - new Vector2(textPadding.horizontal, textPadding.vertical);
            uIFontRenderer.pixelRatio = PixelsToUnits();
            uIFontRenderer.textScale = textScale;
            uIFontRenderer.vectorOffset = (pivot.TransformToUpperLeft(size, arbitraryPivotOffset) + new Vector3(textPadding.left, -textPadding.top)) * PixelsToUnits();
            uIFontRenderer.textAlign = textHorizontalAlignment;
            uIFontRenderer.processMarkup = processMarkup;
            uIFontRenderer.overrideMarkupColors = false;
            uIFontRenderer.opacity = CalculateOpacity();
            uIFontRenderer.shadow = useDropShadow;
            uIFontRenderer.shadowOffset = dropShadowOffset;
            uIFontRenderer.outline = useOutline;
            uIFontRenderer.outlineSize = outlineSize;

            return uIFontRenderer;
        }
    }
    public class CustomUIDragHandle : UIDragHandle
    {
        private Vector3 positionBefore;

        public override void ResetLayout() => positionBefore = relativePosition;
        public override void PerformLayout()
        {
            if ((relativePosition - positionBefore).sqrMagnitude > 0.001)
                relativePosition = positionBefore;
        }
    }

    public class CustomUIScrollbar : UIScrollbar
    {
        private Vector3 positionBefore;
        public override void ResetLayout() => positionBefore = relativePosition;
        public override void PerformLayout()
        {
            if ((relativePosition - positionBefore).sqrMagnitude > 0.001)
                relativePosition = positionBefore;
        }
    }
    public class CustomUIProgressBar : UIProgressBar
    {
        private Vector3 positionBefore;
        public override void ResetLayout() => positionBefore = relativePosition;
        public override void PerformLayout()
        {
            if ((relativePosition - positionBefore).sqrMagnitude > 0.001)
                relativePosition = positionBefore;
        }
    }
}
