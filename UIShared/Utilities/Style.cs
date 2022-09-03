using ColossalFramework.UI;
using ModsCommon.Utilities;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static ColossalFramework.UI.UIDropDown;

namespace ModsCommon.UI
{
    public static class ComponentStyle
    {
        public static void DefaultStyle(this UIButton button)
        {
            button.normalBgSprite = "ButtonMenu";
            button.hoveredTextColor = new Color32(7, 132, 255, 255);
            button.pressedTextColor = new Color32(30, 30, 44, 255);
            button.disabledTextColor = new Color32(7, 7, 7, 255);
            button.horizontalAlignment = UIHorizontalAlignment.Center;
            button.verticalAlignment = UIVerticalAlignment.Middle;
            button.textHorizontalAlignment = UIHorizontalAlignment.Center;
        }
        public static void CustomStyle(this UIButton button)
        {
            button.normalBgSprite = "TextFieldPanel";
            button.color = new Color32(224, 224, 224, 255);
            button.hoveredColor = new Color32(192, 192, 192, 255);
            button.pressedColor = new Color32(7, 132, 255, 255);
            button.textColor = Color.white;
            button.hoveredTextColor = Color.white;
            button.pressedTextColor = Color.white;
            button.horizontalAlignment = UIHorizontalAlignment.Center;
            button.verticalAlignment = UIVerticalAlignment.Middle;
            button.textHorizontalAlignment = UIHorizontalAlignment.Center;
        }

        public static void DefaultStyle(this UIDropDown dropDown, Vector2? size = null)
        {
            dropDown.atlas = CommonTextures.Atlas;
            dropDown.listBackground = CommonTextures.FieldHovered;
            dropDown.itemHeight = 20;
            dropDown.itemHover = CommonTextures.FieldNormal;
            dropDown.itemHighlight = CommonTextures.FieldFocused;
            dropDown.normalBgSprite = CommonTextures.FieldNormal;
            dropDown.hoveredBgSprite = CommonTextures.FieldHovered;
            dropDown.disabledBgSprite = CommonTextures.FieldDisabled;
            dropDown.listHeight = 700;
            dropDown.listPosition = PopupListPosition.Below;
            dropDown.clampListToScreen = true;
            dropDown.foregroundSpriteMode = UIForegroundSpriteMode.Stretch;
            dropDown.textScale = 0.7f;
            dropDown.textFieldPadding = new RectOffset(8, 0, 6, 0);
            dropDown.popupColor = Color.white;
            dropDown.popupTextColor = Color.black;
            dropDown.verticalAlignment = UIVerticalAlignment.Middle;
            dropDown.horizontalAlignment = UIHorizontalAlignment.Left;
            dropDown.itemPadding = new RectOffset(14, 0, 5, 0);

            var button = dropDown.Find<CustomUIButton>("Trigger");
            if(button == null)
            {
                button = dropDown.AddUIComponent<CustomUIButton>();
                button.name = "Trigger";
            }
            button.atlas = TextureHelper.InGameAtlas;
            button.text = string.Empty;
            button.relativePosition = new Vector3(0f, 0f);
            button.textVerticalAlignment = UIVerticalAlignment.Middle;
            button.textHorizontalAlignment = UIHorizontalAlignment.Left;
            button.normalFgSprite = "IconDownArrow";
            button.hoveredFgSprite = "IconDownArrowHovered";
            button.pressedFgSprite = "IconDownArrowPressed";
            button.focusedFgSprite = "IconDownArrow";
            button.disabledFgSprite = "IconDownArrowDisabled";
            button.foregroundSpriteMode = UIForegroundSpriteMode.Scale;
            button.horizontalAlignment = UIHorizontalAlignment.Right;
            button.verticalAlignment = UIVerticalAlignment.Middle;
            button.textScale = 0.8f;

            dropDown.triggerButton = button;

            dropDown.size = size ?? new Vector2(230, 20);
        }

        public static void DefaultSettingsStyle(this UIDropDown dropDown, Vector2? size = null)
        {
            dropDown.atlas = TextureHelper.InGameAtlas;
            dropDown.listBackground = "OptionsDropboxListbox";
            dropDown.itemHeight = 24;
            dropDown.itemHover = "ListItemHover";
            dropDown.itemHighlight = "ListItemHighlight";
            dropDown.normalBgSprite = "OptionsDropbox";
            dropDown.hoveredBgSprite = "OptionsDropboxHovered";
            dropDown.focusedBgSprite = "OptionsDropboxFocused";
            dropDown.autoListWidth = true;
            dropDown.listHeight = 700;
            dropDown.listPosition = PopupListPosition.Below;
            dropDown.clampListToScreen = false;
            dropDown.foregroundSpriteMode = UIForegroundSpriteMode.Stretch;
            dropDown.popupColor = Color.white;
            dropDown.popupTextColor = new Color32(170, 170, 170, 255);
            dropDown.textScale = 1f;
            dropDown.textFieldPadding = new RectOffset(14, 40, 7, 0);
            dropDown.verticalAlignment = UIVerticalAlignment.Middle;
            dropDown.horizontalAlignment = UIHorizontalAlignment.Left;
            dropDown.itemPadding = new RectOffset(14, 14, 4, 0);
            dropDown.triggerButton = dropDown;

            dropDown.size = size ?? new Vector2(400, 31);
        }

        public static void CustomSettingsStyle(this CustomUIDropDown dropDown, Vector2? size = null)
        {
            dropDown.atlasBackground = CommonTextures.Atlas;
            dropDown.normalBgSprite = CommonTextures.FieldNormal;
            dropDown.hoveredBgSprite = CommonTextures.FieldNormal;

            dropDown.atlasForeground = TextureHelper.InGameAtlas;
            dropDown.normalFgSprite = "IconDownArrow";
            dropDown.hoveredFgSprite = "IconDownArrowHovered";
            //dropDown.pressedFgSprite = "IconDownArrowPressed";
            dropDown.focusedFgSprite = "IconDownArrow";
            dropDown.disabledFgSprite = "IconDownArrowDisabled";

            dropDown.atlas = CommonTextures.Atlas;
            dropDown.listBackground = CommonTextures.FieldHovered;
            dropDown.itemHover = CommonTextures.FieldNormal;
            dropDown.itemHighlight = CommonTextures.FieldFocused;

            dropDown.itemHeight = 24;
            dropDown.listHeight = 700;
            dropDown.autoListWidth = true;
            dropDown.listPosition = PopupListPosition.Below;
            dropDown.itemPadding = new RectOffset(14, 14, 4, 0);
            dropDown.clampListToScreen = false;
            dropDown.foregroundSpriteMode = UIForegroundSpriteMode.Stretch;

            dropDown.color = new Color32(101, 101, 101, 255);
            dropDown.hoveredBgColor = new Color32(172, 172, 172, 255);
            dropDown.normalFgColor = Color.black;
            dropDown.hoveredFgColor = new Color32(64, 64, 64, 255);
            dropDown.popupColor = new Color32(101, 101, 101, 255);
            dropDown.popupTextColor = Color.white;

            dropDown.textScale = 1f;
            dropDown.textFieldPadding = new RectOffset(14, 40, 7, 0);
            dropDown.verticalAlignment = UIVerticalAlignment.Middle;
            dropDown.horizontalAlignment = UIHorizontalAlignment.Left;
            dropDown.foregroundSpriteMode = UIForegroundSpriteMode.Scale;
            dropDown.horizontalAlignment = UIHorizontalAlignment.Right;
            dropDown.verticalAlignment = UIVerticalAlignment.Middle;

            dropDown.triggerButton = dropDown;

            dropDown.size = size ?? new Vector2(230, 20);
        }

        public static void CustomSettingsStyle(this UITextField textField)
        {
            textField.atlas = CommonTextures.Atlas;
            textField.normalBgSprite = CommonTextures.FieldNormal;
            textField.selectionSprite = CommonTextures.Empty;
            textField.color = new Color32(101, 101, 101, 255);
        }
    }
}
