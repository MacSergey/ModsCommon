using ColossalFramework.UI;
using UnityEngine;

namespace ModsCommon.UI
{
    public class CustomUIListBox : UIListBox
    {
        private Vector3 positionBefore;
        public override void ResetLayout() => positionBefore = relativePosition;
        public override void PerformLayout()
        {
            if ((relativePosition - positionBefore).sqrMagnitude > 0.001)
                relativePosition = positionBefore;
        }

        //protected override void OnRebuildRenderData()
        //{
        //	if (atlas != null && font != null && font.isValid)
        //	{
        //		if (textRenderData != null)
        //			textRenderData.Clear();
        //		else
        //		{
        //			UIRenderData item = UIRenderData.Obtain();
        //			m_RenderData.Add(item);
        //		}

        //		renderData.material = atlas.material;
        //		textRenderData.material = atlas.material;
        //		RenderBackground();
        //		int count = renderData.vertices.Count;
        //		RenderHover();
        //		RenderSelection();
        //		RenderItems();
        //		ClipQuads(renderData, count);
        //		ClipQuads(textRenderData, 0);
        //	}
        //}

        //private void RenderHover()
        //{
        //	if (!Application.isPlaying || atlas == null || !isEnabled || m_HoverIndex < 0 || m_HoverIndex > items.Length - 1 || string.IsNullOrEmpty(itemHover) || IsFilteredItem(m_HoverIndex))
        //		return;

        //	UITextureAtlas.SpriteInfo spriteInfo = atlas[itemHover];
        //	if (spriteInfo == null)
        //		return;

        //	Vector3 vector = pivot.TransformToUpperLeft(size, arbitraryPivotOffset);
        //	Vector3 offset = new Vector3(vector.x + listPadding.left, vector.y - listPadding.top + scrollPosition, 0f);
        //	float num = PixelsToUnits();
        //	int num2 = m_HoverIndex * itemHeight;
        //	if (animateHover)
        //	{
        //		float num3 = Mathf.Abs(m_HoverTweenLocation - (float)num2);
        //		float num4 = (size.y - listPadding.vertical) * 0.5f;
        //		if (num3 > num4)
        //		{
        //			m_HoverTweenLocation = (float)num2 + Mathf.Sign(m_HoverTweenLocation - (float)num2) * num4;
        //			num3 = num4;
        //		}
        //		float maxDelta = Time.deltaTime / num * 2f;
        //		m_HoverTweenLocation = Mathf.MoveTowards(m_HoverTweenLocation, num2, maxDelta);
        //	}
        //	else
        //		m_HoverTweenLocation = num2;

        //	offset.y -= m_HoverTweenLocation.Quantize(num);

        //	RenderOptions renderOptions = new RenderOptions()
        //	{
        //		_atlas = base.atlas,
        //		_color = ApplyOpacity(base.color),
        //		_fillAmount = 1f,
        //		_flip = UISpriteFlip.None,
        //		_pixelsToUnits = PixelsToUnits(),
        //		_size = new Vector3(base.size.x - (float)listPadding.horizontal, itemHeight),
        //		_spriteInfo = spriteInfo,
        //		_offset = offset,
        //	};

        //	if (spriteInfo.isSliced)
        //		Render.RenderSlicedSprite(renderData, renderOptions);
        //	else
        //		Render.RenderSprite(renderData, renderOptions);

        //	if ((float)num2 != m_HoverTweenLocation)
        //		Invalidate();
        //}
    }
}
