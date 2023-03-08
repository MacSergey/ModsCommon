using ColossalFramework.UI;
using ModsCommon.Utilities;
using System;
using UnityEngine;
using static ColossalFramework.UI.UIDropDown;

namespace ModsCommon.UI
{
    public static class ComponentStyle
    {
        public static Color32 NormalBlue => new Color32(51, 153, 255, 255);
        public static Color32 HoveredBlue => new Color32(29, 143, 255, 255);
        public static Color32 PressedBlue => new Color32(7, 132, 255, 255);

        public static Color32 NormalGray => new Color32(129, 141, 145, 255);
        public static Color32 HoveredGray => new Color32(104, 116, 120, 255);
        public static Color32 PressedGray => new Color32(81, 90, 93, 255);
        public static Color32 DisabledGray => new Color32(35, 39, 41, 255);


        public static Color32 ButtonNormalColor => NormalGray;
        public static Color32 ButtonHoveredColor => HoveredGray;
        public static Color32 ButtonPressedColor => PressedGray;
        public static Color32 ButtonFocusedColor => NormalBlue;
        public static Color32 ButtonDisabledColor => DisabledGray;
        public static Color32 ButtonSelectedNormalColor => NormalBlue;
        public static Color32 ButtonSelectedHoveredColor => HoveredBlue;
        public static Color32 ButtonSelectedPressedColor => PressedBlue;
        public static Color32 ButtonSelectedFocusedColor => NormalBlue;
        public static Color32 ButtonSelectedDisabledColor => DisabledGray;


        public static Color32 FieldNormalColor => new Color32(148, 161, 166, 255);
        public static Color32 FieldHoveredColor => new Color32(177, 189, 193, 255);
        public static Color32 FieldDisabledColor => new Color32(104, 110, 113, 255);
        public static Color32 FieldFocusedColor => new Color32(151, 202, 222, 255);
        public static Color32 FieldDisabledFocusedColor => new Color32(113, 151, 166, 255);

        #region BUTTON

        public static void CustomStyle(this CustomUIButton button)
        {
            button.atlas = CommonTextures.Atlas;
            button.SetBgSprite(new SpriteSet(CommonTextures.PanelBig));
            button.SetBgColor(new ColorSet(ButtonNormalColor, ButtonHoveredColor, ButtonPressedColor, ButtonFocusedColor, ButtonDisabledColor));
            button.SetSelectedBgColor(new ColorSet(ButtonSelectedNormalColor, ButtonSelectedHoveredColor, ButtonSelectedPressedColor, ButtonSelectedFocusedColor, ButtonSelectedDisabledColor));
            button.SetTextColor(new ColorSet(Color.white, Color.white, Color.white, null, null));

            button.horizontalAlignment = UIHorizontalAlignment.Center;
            button.verticalAlignment = UIVerticalAlignment.Middle;
            button.textHorizontalAlignment = UIHorizontalAlignment.Center;
        }

        #endregion

        #region DROPDOWN

        [Obsolete]
        public static void DefaultStyle(this CustomUIDropDown dropDown, Vector2? size = null)
        {
            dropDown.atlasBackground = CommonTextures.Atlas;
            dropDown.normalBgSprite = CommonTextures.FieldSingle;
            dropDown.hoveredBgSprite = CommonTextures.FieldSingle;
            dropDown.disabledBgSprite = CommonTextures.FieldSingle;
            dropDown.color = FieldNormalColor;
            dropDown.hoveredBgColor = FieldHoveredColor;
            dropDown.disabledBgColor = FieldDisabledColor;

            dropDown.atlasForeground = TextureHelper.InGameAtlas;
            dropDown.normalFgSprite = "IconDownArrow";
            dropDown.hoveredFgSprite = "IconDownArrowHovered";
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
            dropDown.normalBgSprite = CommonTextures.FieldSingle;
            dropDown.hoveredBgSprite = CommonTextures.FieldSingle;
            dropDown.disabledBgSprite = CommonTextures.FieldSingle;

            dropDown.normalBgColor = FieldNormalColor;
            dropDown.hoveredBgColor = FieldHoveredColor;
            dropDown.disabledBgColor = FieldDisabledColor;

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
            popup.backgroundSprite = CommonTextures.FieldSingle;
            popup.ItemHover = CommonTextures.FieldSingle;
            popup.ItemSelected = CommonTextures.FieldSingle;

            popup.color = FieldHoveredColor;
            popup.ColorHover = FieldNormalColor;
            popup.ColorSelected = FieldFocusedColor;

            popup.EntityHeight = entityHeight ?? 20f;
            popup.MaximumSize = new Vector2(230f, 700f);
        }


        public static void CustomSettingsStyle<ObjectType, PopupType, EntityType>(this AdvancedDropDown<ObjectType, PopupType, EntityType> dropDown, Vector2? size = null)
            where PopupType : Popup<ObjectType, EntityType>
            where EntityType : PopupEntity<ObjectType>
        {
            dropDown.atlasBackground = CommonTextures.Atlas;
            dropDown.normalBgSprite = CommonTextures.FieldNormal;
            dropDown.hoveredBgSprite = CommonTextures.FieldNormal;

            dropDown.normalBgColor = new Color32(101, 101, 101, 255);
            dropDown.hoveredBgColor = new Color32(172, 172, 172, 255);

            dropDown.atlasForeground = TextureHelper.InGameAtlas;
            dropDown.normalFgSprite = "IconDownArrow";
            dropDown.hoveredFgSprite = "IconDownArrowHovered";
            dropDown.pressedFgSprite = "IconDownArrowPressed";
            dropDown.focusedFgSprite = "IconDownArrow";
            dropDown.disabledFgSprite = "IconDownArrowDisabled";
            dropDown.foregroundSpriteMode = UIForegroundSpriteMode.Scale;

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

        #endregion

        #region TEXT FIELD

        public static void DefaultStyle(this CustomUITextField textField)
        {
            textField.atlas = CommonTextures.Atlas;
            textField.normalBgSprite = CommonTextures.FieldSingle;
            textField.hoveredBgSprite = CommonTextures.FieldSingle;
            textField.focusedBgSprite = CommonTextures.FieldSingle;
            textField.disabledBgSprite = CommonTextures.FieldSingle;
            textField.selectionSprite = CommonTextures.Empty;

            textField.color = FieldNormalColor;
            textField.hoveredColor = FieldHoveredColor;
            textField.focusedColor = FieldFocusedColor;
            textField.disabledColor = FieldDisabledColor;

            textField.allowFloats = true;
            textField.isInteractive = true;
            textField.enabled = true;
            textField.readOnly = false;
            textField.builtinKeyNavigation = true;
            textField.cursorWidth = 1;
            textField.cursorBlinkTime = 0.45f;
            textField.selectOnFocus = true;

            textField.verticalAlignment = UIVerticalAlignment.Middle;
            textField.padding = new RectOffset(0, 0, 6, 0);
        }
        public static void CustomSettingsStyle(this UITextField textField)
        {
            textField.atlas = CommonTextures.Atlas;
            textField.normalBgSprite = CommonTextures.FieldNormal;
            textField.selectionSprite = CommonTextures.Empty;
            textField.color = new Color32(101, 101, 101, 255);
        }

        #endregion

        #region TAB STRIPE

        public static void CustomStyle<TabType>(this TabStrip<TabType> tabStrip)
            where TabType : Tab
        {
            tabStrip.atlas = CommonTextures.Atlas;
            tabStrip.backgroundSprite = CommonTextures.PanelBig;

            tabStrip.TabSpacingHorizontal = 4;
            tabStrip.TabSpacingVertical = 4;

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

        #endregion
    }
}
