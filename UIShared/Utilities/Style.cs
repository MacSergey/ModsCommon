using ColossalFramework.UI;
using ModsCommon.Utilities;
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
            button.hoveredColor = new Color32(160, 160, 160, 255);
            button.pressedColor = new Color32(7, 132, 255, 255);
            button.focusedColor = new Color32(128, 192, 255, 255);
            button.textColor = Color.white;
            button.hoveredTextColor = Color.white;
            button.pressedTextColor = Color.white;
            button.horizontalAlignment = UIHorizontalAlignment.Center;
            button.verticalAlignment = UIVerticalAlignment.Middle;
            button.textHorizontalAlignment = UIHorizontalAlignment.Center;
        }

        public static void DefaultStyle(this CustomUIDropDown dropDown, Vector2? size = null)
        {
            dropDown.atlasBackground = CommonTextures.Atlas;
            dropDown.normalBgSprite = CommonTextures.FieldNormal;
            dropDown.hoveredBgSprite = CommonTextures.FieldHovered;
            dropDown.disabledBgSprite = CommonTextures.FieldDisabled;

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

            dropDown.itemHeight = 20;
            dropDown.listHeight = 700;
            dropDown.listPosition = PopupListPosition.Below;
            dropDown.clampListToScreen = true;
            dropDown.foregroundSpriteMode = UIForegroundSpriteMode.Stretch;
            dropDown.itemPadding = new RectOffset(14, 0, 5, 0);

            dropDown.popupColor = Color.white;
            dropDown.popupTextColor = Color.black;

            dropDown.textScale = 0.7f;
            dropDown.textFieldPadding = new RectOffset(8, 0, 6, 0);
            dropDown.verticalAlignment = UIVerticalAlignment.Middle;
            dropDown.horizontalAlignment = UIHorizontalAlignment.Left;
            dropDown.foregroundSpriteMode = UIForegroundSpriteMode.Scale;
            dropDown.horizontalAlignment = UIHorizontalAlignment.Right;
            dropDown.verticalAlignment = UIVerticalAlignment.Middle;

            dropDown.triggerButton = dropDown;

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

        public static void CustomSettingsStyle<ObjectType, PopupType, EntityType>(this AdvancedDropDown<ObjectType, PopupType, EntityType> dropDown, Vector2? size = null)
            where PopupType : Popup<ObjectType, EntityType>
            where EntityType : PopupEntity<ObjectType>
        {
            dropDown.atlasBackground = CommonTextures.Atlas;
            dropDown.normalBgSprite = CommonTextures.FieldNormal;
            dropDown.hoveredBgSprite = CommonTextures.FieldNormal;

            dropDown.atlasForeground = TextureHelper.InGameAtlas;
            dropDown.normalFgSprite = "IconDownArrow";
            dropDown.hoveredFgSprite = "IconDownArrowHovered";
            dropDown.pressedFgSprite = "IconDownArrowPressed";
            dropDown.focusedFgSprite = "IconDownArrow";
            dropDown.disabledFgSprite = "IconDownArrowDisabled";
            dropDown.foregroundSpriteMode = UIForegroundSpriteMode.Scale;

            dropDown.color = new Color32(101, 101, 101, 255);
            dropDown.hoveredBgColor = new Color32(172, 172, 172, 255);
            dropDown.normalFgColor = Color.black;
            dropDown.hoveredFgColor = new Color32(64, 64, 64, 255);

            dropDown.textVerticalAlignment = UIVerticalAlignment.Middle;
            dropDown.textHorizontalAlignment = UIHorizontalAlignment.Left;

            dropDown.horizontalAlignment = UIHorizontalAlignment.Right;
            dropDown.verticalAlignment = UIVerticalAlignment.Middle;

            dropDown.size = size ?? new Vector2(230, 20);
        }
        public static void CustomSettingsStyle<ObjectType, EntityType>(this Popup<ObjectType, EntityType> popup, float? entityHeight = null)
            where EntityType : PopupEntity<ObjectType>
        {
            popup.atlas = CommonTextures.Atlas;
            popup.backgroundSprite = CommonTextures.FieldHovered;
            popup.ItemHover = CommonTextures.FieldNormal;
            popup.ItemSelected = CommonTextures.FieldNormal;

            popup.color = new Color32(101, 101, 101, 255);
            popup.ColorHover = new Color32(128, 192, 255, 255);

            popup.EntityHeight = entityHeight ?? 20f;
            popup.MaximumSize = new Vector2(230f, 700f);
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
