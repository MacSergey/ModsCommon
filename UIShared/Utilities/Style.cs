using ColossalFramework.UI;
using ModsCommon.Utilities;
using UnityEngine;
using static ColossalFramework.UI.UIDropDown;

namespace ModsCommon.UI
{
    public static class ComponentStyle
    {
        public static Color32 PressedBlue => new Color32(7, 132, 255, 255);
        public static Color32 FocusedBlue => new Color32(51, 153, 255, 255);

        public static Color32 NormalGray => new Color32(129, 141, 145, 255);
        public static Color32 HoveredGray => new Color32(104, 116, 120, 255);
        public static Color32 PressedGray => new Color32(81, 90, 93, 255);


        public static Color32 ButtonNormal => NormalGray;
        public static Color32 ButtonHovered => HoveredGray;
        public static Color32 ButtonPressed => PressedBlue;
        public static Color32 ButtonFocused => FocusedBlue;

        public static void CustomStyle(this UIButton button)
        {
            button.atlas = CommonTextures.Atlas;
            button.normalBgSprite = CommonTextures.PanelBig;

            button.color = NormalGray;
            button.hoveredColor = HoveredGray;
            button.pressedColor = PressedBlue;
            button.focusedColor = FocusedBlue;

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

        public static void DefaultStyle<ObjectType, PopupType, EntityType>(this AdvancedDropDown<ObjectType, PopupType, EntityType> dropDown, Vector2? size = null)
            where PopupType : Popup<ObjectType, EntityType>
            where EntityType : PopupEntity<ObjectType>
        {
            dropDown.atlasBackground = CommonTextures.Atlas;
            dropDown.normalBgSprite = CommonTextures.FieldNormal;
            dropDown.hoveredBgSprite = CommonTextures.FieldHovered;
            dropDown.disabledBgSprite = CommonTextures.FieldDisabled;

            dropDown.atlasForeground = TextureHelper.InGameAtlas;
            dropDown.normalFgSprite = "IconDownArrow";
            dropDown.hoveredFgSprite = "IconDownArrowHovered";
            dropDown.focusedFgSprite = "IconDownArrow";
            dropDown.disabledFgSprite = "IconDownArrowDisabled";
            dropDown.foregroundSpriteMode = UIForegroundSpriteMode.Scale;
            dropDown.horizontalAlignment = UIHorizontalAlignment.Right;
            dropDown.verticalAlignment = UIVerticalAlignment.Middle;
            dropDown.spritePadding = new RectOffset(0, 5, 0, 0);

            dropDown.Entity.Padding = new RectOffset(0, 40, 0, 0);

            dropDown.size = size ?? new Vector2(230, 20);
        }
        public static void DefaultStyle<ObjectType, EntityType>(this Popup<ObjectType, EntityType> popup, float? entityHeight = null)
            where EntityType : PopupEntity<ObjectType>
        {
            popup.atlas = CommonTextures.Atlas;
            popup.backgroundSprite = CommonTextures.FieldHovered;
            popup.ItemHover = CommonTextures.FieldNormal;
            popup.ItemSelected = CommonTextures.FieldFocused;

            popup.EntityHeight = entityHeight ?? 20f;
            popup.MaximumSize = new Vector2(230f, 700f);
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

        public static void CustomStyle<TabType>(this TabStrip<TabType> tabStrip)
            where TabType : Tab
        {
            tabStrip.atlas = CommonTextures.Atlas;
            tabStrip.backgroundSprite = CommonTextures.PanelBig;

            tabStrip.TabSpacingHorizontal = 2;
            tabStrip.TabSpacingVertical = 2;

            tabStrip.TabAtlas = CommonTextures.Atlas;
            tabStrip.TabNormalSprite = CommonTextures.PanelSmall;
            tabStrip.TabHoveredSprite = CommonTextures.PanelSmall;
            tabStrip.TabPressedSprite = CommonTextures.PanelSmall;
            tabStrip.TabFocusedSprite = CommonTextures.PanelSmall;
            tabStrip.TabDisabledSprite = CommonTextures.PanelSmall;
        }
        public static void CustomSettingsStyle<TabType>(this TabStrip<TabType> tabStrip)
            where TabType : Tab
        {
            tabStrip.atlas = CommonTextures.Atlas;
            tabStrip.backgroundSprite = CommonTextures.PanelBig;
            tabStrip.color = new Color32(40, 50, 76, 255);

            tabStrip.TabSpacingHorizontal = 4;
            tabStrip.TabSpacingVertical = 4;

            tabStrip.TabAtlas = CommonTextures.Atlas;
            tabStrip.TabNormalSprite = CommonTextures.PanelSmall;
            tabStrip.TabHoveredSprite = CommonTextures.PanelSmall;
            tabStrip.TabPressedSprite = CommonTextures.PanelSmall;
            tabStrip.TabFocusedSprite = CommonTextures.PanelSmall;
            tabStrip.TabDisabledSprite = CommonTextures.PanelSmall;

            tabStrip.TabColor = tabStrip.color;
            tabStrip.TabHoveredColor = HoveredGray;
            tabStrip.TabPressedColor = new Color32(120, 120, 120, 255);
            tabStrip.TabFocusedColor = new Color32(0, 128, 255, 255);
        }
    }
}
