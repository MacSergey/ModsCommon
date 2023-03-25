using ColossalFramework.UI;
using ModsCommon.Utilities;
using System;
using System.Linq;
using UnityEngine;

namespace ModsCommon.UI
{
    public static class ComponentStyle
    {
        public static UIDynamicFont SemiBoldFont { get; } = Resources.FindObjectsOfTypeAll<UIDynamicFont>().FirstOrDefault(f => f.name == "OpenSans-Semibold");
        public static UIDynamicFont RegularFont { get; } = Resources.FindObjectsOfTypeAll<UIDynamicFont>().FirstOrDefault(f => f.name == "OpenSans-Regular");


        public static Color32 DarkPrimaryColor0 => new Color32(0, 0, 0, 255);
        public static Color32 DarkPrimaryColor5 => new Color32(12, 13, 13, 255);
        public static Color32 DarkPrimaryColor10 => new Color32(24, 26, 27, 255);
        public static Color32 DarkPrimaryColor15 => new Color32(36, 39, 40, 255);
        public static Color32 DarkPrimaryColor20 => new Color32(48, 52, 54, 255);
        public static Color32 DarkPrimaryColor25 => new Color32(61, 65, 67, 255);
        public static Color32 DarkPrimaryColor30 => new Color32(73, 78, 80, 255);
        public static Color32 DarkPrimaryColor35 => new Color32(85, 90, 94, 255);
        public static Color32 DarkPrimaryColor40 => new Color32(97, 103, 107, 255);
        public static Color32 DarkPrimaryColor45 => new Color32(109, 116, 120, 255);
        public static Color32 DarkPrimaryColor50 => new Color32(121, 129, 134, 255);
        public static Color32 DarkPrimaryColor55 => new Color32(135, 142, 146, 255);
        public static Color32 DarkPrimaryColor60 => new Color32(148, 154, 158, 255);
        public static Color32 DarkPrimaryColor65 => new Color32(162, 167, 170, 255);
        public static Color32 DarkPrimaryColor70 => new Color32(175, 179, 182, 255);
        public static Color32 DarkPrimaryColor75 => new Color32(188, 192, 194, 255);
        public static Color32 DarkPrimaryColor80 => new Color32(202, 205, 206, 255);
        public static Color32 DarkPrimaryColor85 => new Color32(215, 217, 219, 255);
        public static Color32 DarkPrimaryColor90 => new Color32(228, 230, 231, 255);
        public static Color32 DarkPrimaryColor95 => new Color32(242, 242, 243, 255);
        public static Color32 DarkPrimaryColor100 => new Color32(255, 255, 255, 255);


        public static Color32 SettingsColor0 => new Color32(0, 0, 0, 255);
        public static Color32 SettingsColor5 => new Color32(10, 13, 15, 255);
        public static Color32 SettingsColor10 => new Color32(20, 25, 31, 255);
        public static Color32 SettingsColor15 => new Color32(30, 38, 46, 255);
        public static Color32 SettingsColor20 => new Color32(40, 50, 62, 255);
        public static Color32 SettingsColor25 => new Color32(50, 63, 77, 255);
        public static Color32 SettingsColor30 => new Color32(60, 75, 93, 255);
        public static Color32 SettingsColor35 => new Color32(71, 88, 108, 255);
        public static Color32 SettingsColor40 => new Color32(81, 101, 123, 255);
        public static Color32 SettingsColor45 => new Color32(91, 113, 139, 255);
        public static Color32 SettingsColor50 => new Color32(101, 126, 154, 255);
        public static Color32 SettingsColor55 => new Color32(116, 139, 164, 255);
        public static Color32 SettingsColor60 => new Color32(132, 152, 174, 255);
        public static Color32 SettingsColor65 => new Color32(147, 165, 184, 255);
        public static Color32 SettingsColor70 => new Color32(162, 177, 195, 255);
        public static Color32 SettingsColor75 => new Color32(178, 190, 205, 255);
        public static Color32 SettingsColor80 => new Color32(193, 203, 215, 255);
        public static Color32 SettingsColor85 => new Color32(109, 216, 225, 255);
        public static Color32 SettingsColor90 => new Color32(224, 229, 235, 255);
        public static Color32 SettingsColor95 => new Color32(240, 242, 245, 255);
        public static Color32 SettingsColor100 => new Color32(255, 255, 255, 255);



        public static Color32 ErrorNormalColor => new Color32(246, 84, 85, 255);
        public static Color32 ErrorPressedColor => new Color32(248, 68, 68, 255);
        public static Color32 ErrorFocusedColor => new Color32(225, 62, 62, 255);


        public static Color32 WellColor => new Color32(42, 185, 48, 255);
        public static Color32 WarningColor => new Color32(237, 149, 38, 255);

        public static Color32 NormalBlue => new Color32(51, 153, 255, 255);
        public static Color32 HoveredBlue => new Color32(29, 143, 255, 255);
        public static Color32 PressedBlue => new Color32(7, 132, 255, 255);


        public static Color32 NormalGreen => new Color32(98, 179, 45, 255);
        public static Color32 HoveredGreen => new Color32(115, 205, 55, 255);


        public static Color32 PanelColor => DarkPrimaryColor20;
        public static Color32 HeaderColor => DarkPrimaryColor10;


        #region BUTTON

        public static Color32 ButtonNormalColor => DarkPrimaryColor55;
        public static Color32 ButtonHoveredColor => DarkPrimaryColor45;
        public static Color32 ButtonPressedColor => DarkPrimaryColor35;
        public static Color32 ButtonFocusedColor => NormalBlue;
        public static Color32 ButtonDisabledColor => DarkPrimaryColor15;
        public static Color32 ButtonSelectedNormalColor => NormalBlue;
        public static Color32 ButtonSelectedHoveredColor => HoveredBlue;
        public static Color32 ButtonSelectedPressedColor => PressedBlue;
        public static Color32 ButtonSelectedFocusedColor => NormalBlue;
        public static Color32 ButtonSelectedDisabledColor => DarkPrimaryColor15;


        public static void ButtonMessageBoxStyle(this CustomUIButton button)
        {
            button.atlas = CommonTextures.Atlas;
            button.SetBgSprite(new SpriteSet(CommonTextures.PanelBig));
            button.SetBgColor(new ColorSet(ButtonNormalColor, ButtonHoveredColor, ButtonPressedColor, ButtonNormalColor, ButtonDisabledColor));
            button.SetSelectedBgColor(new ColorSet(ButtonSelectedNormalColor, ButtonSelectedHoveredColor, ButtonSelectedPressedColor, ButtonSelectedFocusedColor, ButtonSelectedDisabledColor));
            button.SetTextColor(new ColorSet(Color.white, Color.white, Color.white, null, null));
            button.Bold = true;

            button.horizontalAlignment = UIHorizontalAlignment.Center;
            button.verticalAlignment = UIVerticalAlignment.Middle;
            button.textHorizontalAlignment = UIHorizontalAlignment.Center;

            button.textPadding.top = 2;
        }
        public static void ButtonSettingsStyle(this CustomUIButton button)
        {
            button.atlas = CommonTextures.Atlas;
            button.SetBgSprite(new SpriteSet(CommonTextures.PanelBig));
            button.SetBgColor(new ColorSet(SettingsColor60, SettingsColor70, SettingsColor70, SettingsColor60, SettingsColor15));
            button.SetTextColor(new ColorSet(Color.white, Color.white, Color.white, null, null));

            button.horizontalAlignment = UIHorizontalAlignment.Center;
            button.verticalAlignment = UIVerticalAlignment.Middle;
            button.textHorizontalAlignment = UIHorizontalAlignment.Center;

            button.textPadding.top = 2;
        }

        [Obsolete]
        public static Color32 ButtonNormal => Color.white;
        [Obsolete]
        public static Color32 ButtonHovered => new Color32(224, 224, 224, 255);
        [Obsolete]
        public static Color32 ButtonPressed => new Color32(192, 192, 192, 255);
        [Obsolete]
        public static Color32 ButtonFocused => new Color32(160, 160, 160, 255);
        [Obsolete]
        public static void SetDefaultStyle(this UIButton button)
        {
            button.atlas = TextureHelper.InGameAtlas;
            button.normalBgSprite = "ButtonWhite";
            button.disabledBgSprite = "ButtonWhite";
            button.hoveredBgSprite = "ButtonWhite";
            button.pressedBgSprite = "ButtonWhite";
            button.color = ButtonNormal;
            button.hoveredColor = ButtonHovered;
            button.pressedColor = ButtonPressed;
            button.disabledColor = ButtonFocused;
            button.textColor = button.hoveredTextColor = button.focusedTextColor = Color.black;
            button.pressedTextColor = button.disabledTextColor = Color.white;
        }

        #endregion

        #region DROPDOWN

        public static void DropDownDefaultStyle<ObjectType, PopupType, EntityType>(this SelectItemDropDown<ObjectType, EntityType, PopupType> dropDown, Vector2? size = null)
            where PopupType : CustomUIPanel, IPopup<ObjectType, EntityType>
            where EntityType : CustomUIButton, IPopupEntity<ObjectType>
        {
            dropDown.atlasBackground = CommonTextures.Atlas;
            dropDown.SetBgSprite(new SpriteSet(CommonTextures.FieldSingle));
            dropDown.SetBgColor(new ColorSet(FieldNormalColor, FieldHoveredColor, FieldHoveredColor, FieldNormalColor, FieldDisabledColor));

            dropDown.atlasForeground = CommonTextures.Atlas;
            dropDown.SetFgSprite(new SpriteSet(CommonTextures.ArrowDown));
            dropDown.SetFgColor(new ColorSet(new Color32(0, 0, 0, 255)));

            dropDown.foregroundSpriteMode = UIForegroundSpriteMode.Scale;
            dropDown.scaleFactor = 0.7f;
            dropDown.horizontalAlignment = UIHorizontalAlignment.Right;
            dropDown.verticalAlignment = UIVerticalAlignment.Middle;
            dropDown.spritePadding = new RectOffset(0, 5, 0, 0);

            dropDown.Entity.Padding = new RectOffset(0, 40, 0, 0);

            dropDown.size = size ?? new Vector2(230, 20);
        }
        public static void PopupDefaultStyle<ObjectType, EntityType>(this ObjectPopup<ObjectType, EntityType> popup, float? entityHeight = null)
            where EntityType : CustomUIButton, IPopupEntity<ObjectType>, IReusable
        {
            popup.Atlas = CommonTextures.Atlas;
            popup.BackgroundSprite = CommonTextures.FieldSingle;
            popup.color = FieldHoveredColor;

            popup.EntityHeight = entityHeight ?? 20f;
            popup.MaximumSize = new Vector2(230f, 700f);
        }
        public static void EntityDefaultStyle<ObjectType, EntityType>(this EntityType entity)
            where EntityType : CustomUIButton, IPopupEntity<ObjectType>
        {
            entity.atlas = CommonTextures.Atlas;
            entity.hoveredBgSprite = CommonTextures.FieldSingle;
            entity.focusedBgSprite = CommonTextures.FieldSingle;

            entity.hoveredBgColor = FieldNormalColor;
            entity.focusedBgColor = FieldFocusedColor;
        }


        public static void DropDownMessageBoxStyle<ObjectType, PopupType, EntityType>(this SelectItemDropDown<ObjectType, EntityType, PopupType> dropDown, Vector2? size = null)
            where PopupType : ObjectPopup<ObjectType, EntityType>
            where EntityType : PopupEntity<ObjectType>
        {
            dropDown.atlasBackground = CommonTextures.Atlas;
            dropDown.SetBgSprite(new SpriteSet(CommonTextures.FieldSingle));
            dropDown.SetBgColor(new ColorSet(DarkPrimaryColor55, DarkPrimaryColor45, DarkPrimaryColor35, DarkPrimaryColor55, DarkPrimaryColor15));

            dropDown.atlasForeground = CommonTextures.Atlas;
            dropDown.SetFgSprite(new SpriteSet(CommonTextures.ArrowDown));
            dropDown.SetFgColor(new ColorSet(SettingsColor15));

            dropDown.foregroundSpriteMode = UIForegroundSpriteMode.Scale;
            dropDown.scaleFactor = 0.7f;
            dropDown.spritePadding.right = 5;

            dropDown.textVerticalAlignment = UIVerticalAlignment.Middle;
            dropDown.textHorizontalAlignment = UIHorizontalAlignment.Left;

            dropDown.horizontalAlignment = UIHorizontalAlignment.Right;
            dropDown.verticalAlignment = UIVerticalAlignment.Middle;

            dropDown.size = size ?? new Vector2(230, 20);
        }
        public static void PopupMessageBoxStyle<ObjectType, EntityType>(this ObjectPopup<ObjectType, EntityType> popup, float? entityHeight = null)
            where EntityType : PopupEntity<ObjectType>
        {
            popup.Atlas = CommonTextures.Atlas;
            popup.BackgroundSprite = CommonTextures.FieldSingle;
            popup.color = DarkPrimaryColor55;

            popup.EntityHeight = entityHeight ?? 20f;
            popup.MaximumSize = new Vector2(230f, 700f);
            popup.ItemsPadding = new RectOffset(4, 4, 4, 4);
        }
        public static void EntityMessageBoxStyle<ObjectType>(this PopupEntity<ObjectType> entity)
        {
            entity.atlas = CommonTextures.Atlas;
            entity.hoveredBgSprite = CommonTextures.FieldSingle;
            entity.focusedBgSprite = CommonTextures.FieldSingle;

            entity.hoveredBgColor = DarkPrimaryColor45;
            entity.focusedBgColor = NormalBlue;
        }


        public static void DropDownSettingsStyle<ObjectType, EntityType, PopupType>(this SelectItemDropDown<ObjectType, EntityType, PopupType> dropDown, Vector2? size = null)
            where PopupType : ObjectPopup<ObjectType, EntityType>
            where EntityType : PopupEntity<ObjectType>
        {
            dropDown.atlasBackground = CommonTextures.Atlas;
            dropDown.SetBgSprite(new SpriteSet(CommonTextures.FieldSingle));
            dropDown.SetBgColor(new ColorSet(SettingsColor60, SettingsColor70, SettingsColor70, SettingsColor60, SettingsColor15));

            dropDown.atlasForeground = CommonTextures.Atlas;
            dropDown.SetFgSprite(new SpriteSet(CommonTextures.ArrowDown));
            dropDown.SetFgColor(new ColorSet(SettingsColor15));

            dropDown.foregroundSpriteMode = UIForegroundSpriteMode.Scale;
            dropDown.scaleFactor = 0.7f;
            dropDown.spritePadding.right = 5;

            dropDown.textVerticalAlignment = UIVerticalAlignment.Middle;
            dropDown.textHorizontalAlignment = UIHorizontalAlignment.Left;

            dropDown.horizontalAlignment = UIHorizontalAlignment.Right;
            dropDown.verticalAlignment = UIVerticalAlignment.Middle;

            dropDown.size = size ?? new Vector2(230, 20);
        }

        public static void PopupSettingsStyle<ObjectType, EntityType, PopupType>(this PopupType popup, float? entityHeight = null)
            where PopupType : CustomUIPanel, IPopup<ObjectType, EntityType>
            where EntityType : CustomUIButton, IPopupEntity<ObjectType>
        {
            popup.Atlas = CommonTextures.Atlas;
            popup.BackgroundSprite = CommonTextures.FieldSingle;
            popup.color = SettingsColor25;

            popup.EntityHeight = entityHeight ?? 20f;
            popup.MaximumSize = new Vector2(230f, 700f);
            popup.ItemsPadding = new RectOffset(4, 4, 4, 4);
        }
        public static void EntitySettingsStyle<ObjectType, EntityType>(this EntityType entity)
            where EntityType : CustomUIButton, IPopupEntity<ObjectType>
        {
            entity.atlas = CommonTextures.Atlas;
            entity.hoveredBgSprite = CommonTextures.FieldSingle;
            entity.focusedBgSprite = CommonTextures.FieldSingle;

            entity.hoveredBgColor = SettingsColor45;
            entity.focusedBgColor = NormalBlue;
        }

        #endregion

        #region TEXT FIELD

        public static Color32 FieldNormalColor => DarkPrimaryColor60;
        public static Color32 FieldHoveredColor => DarkPrimaryColor70;
        public static Color32 FieldDisabledColor => DarkPrimaryColor50;
        public static Color32 FieldFocusedColor => new Color32(151, 202, 222, 255);
        public static Color32 FieldDisabledFocusedColor => new Color32(113, 151, 166, 255);

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
            textField.selectionBackgroundColor = Color.black;

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
        public static void SettingsStyle(this CustomUITextField textField)
        {
            textField.atlas = CommonTextures.Atlas;
            textField.normalBgSprite = CommonTextures.FieldSingle;
            textField.hoveredBgSprite = CommonTextures.FieldSingle;
            textField.focusedBgSprite = CommonTextures.FieldSingle;
            textField.disabledBgSprite = CommonTextures.FieldSingle;
            textField.selectionSprite = CommonTextures.Empty;

            textField.color = SettingsColor60;
            textField.hoveredColor = SettingsColor70;
            textField.focusedColor = NormalBlue;
            textField.disabledColor = SettingsColor15;
        }
        public static void SetStyle(this CustomUITextField textField, TextFieldStyle style)
        {
            textField.color = style.Colors.normal;
            textField.hoveredColor = style.Colors.hovered;
            textField.focusedColor = style.Colors.focused;
            textField.disabledColor = style.Colors.disabled;

            textField.textColor = style.TextColor;
            textField.selectionBackgroundColor = style.SelectionColor;
        }
        #endregion

        #region TAB STRIPE

        public static void DefaultStyle<TabType>(this TabStrip<TabType> tabStrip)
            where TabType : Tab
        {
            tabStrip.Atlas = CommonTextures.Atlas;
            tabStrip.BackgroundSprite = CommonTextures.PanelBig;

            tabStrip.TabSpacingHorizontal = 4;
            tabStrip.TabSpacingVertical = 4;

            tabStrip.TabAtlas = CommonTextures.Atlas;
            tabStrip.TabNormalSprite = CommonTextures.PanelSmall;
            tabStrip.TabHoveredSprite = CommonTextures.PanelSmall;
            tabStrip.TabPressedSprite = CommonTextures.PanelSmall;
            tabStrip.TabFocusedSprite = CommonTextures.PanelSmall;
            tabStrip.TabDisabledSprite = CommonTextures.PanelSmall;
        }
        public static void SettingsStyle<TabType>(this TabStrip<TabType> tabStrip)
            where TabType : Tab
        {
            tabStrip.Atlas = CommonTextures.Atlas;
            tabStrip.BackgroundSprite = CommonTextures.PanelBig;
            tabStrip.color = SettingsColor25;

            tabStrip.TabSpacingHorizontal = 4;
            tabStrip.TabSpacingVertical = 4;

            tabStrip.TabAtlas = CommonTextures.Atlas;
            tabStrip.TabNormalSprite = CommonTextures.PanelSmall;
            tabStrip.TabHoveredSprite = CommonTextures.PanelSmall;
            tabStrip.TabPressedSprite = CommonTextures.PanelSmall;
            tabStrip.TabFocusedSprite = CommonTextures.PanelSmall;
            tabStrip.TabDisabledSprite = CommonTextures.PanelSmall;

            tabStrip.TabColor = SettingsColor25;
            tabStrip.TabHoveredColor = SettingsColor45;
            tabStrip.TabPressedColor = SettingsColor55;
            tabStrip.TabFocusedColor = NormalBlue;
        }

        #endregion

        #region TOGGLE

        public static Color32 ToggleOnNormalColor => NormalGreen;
        public static Color32 ToggleOnHoveredColor => HoveredGreen;
        public static Color32 ToggleOnPressedColor => HoveredGreen;

        public static Color32 ToggleOffNormalColor => DarkPrimaryColor55;
        public static Color32 ToggleOffHoveredColor => DarkPrimaryColor65;
        public static Color32 ToggleOffPressedColor => DarkPrimaryColor65;


        public static void DefaultStyle(this CustomUIToggle toggle)
        {
            toggle.atlas = CommonTextures.Atlas;
            toggle.SetBgSprite(new SpriteSet(CommonTextures.ToggleBackgroundSmall));
            toggle.SetFgSprite(new SpriteSet(CommonTextures.ToggleCircle));

            toggle.OnColor = FieldFocusedColor;
            toggle.OnHoverColor = FieldFocusedColor;
            toggle.OnPressedColor = FieldFocusedColor;
            toggle.OnDisabledColor = FieldDisabledFocusedColor;

            toggle.OffColor = FieldNormalColor;
            toggle.OffHoverColor = FieldHoveredColor;
            toggle.OffPressedColor = FieldHoveredColor;
            toggle.OffDisabledColor = FieldDisabledColor;

            toggle.textScale = 0.8f;
            toggle.CircleScale = 0.7f;
            toggle.ShowMark = true;
            toggle.textPadding = new RectOffset(12, 6, 5, 0);
            toggle.size = new Vector2(42f, 22f);
        }

        public static void SettingsStyle(this CustomUIToggle toggle)
        {
            toggle.atlas = CommonTextures.Atlas;
            toggle.SetBgSprite(new SpriteSet(CommonTextures.ToggleBackground));
            toggle.SetFgSprite(new SpriteSet(CommonTextures.ToggleCircle));

            toggle.OnColor = ToggleOnNormalColor;
            toggle.OnHoverColor = ToggleOnHoveredColor;
            toggle.OnPressedColor = ToggleOnHoveredColor;

            toggle.OffColor = SettingsColor60;
            toggle.OffHoverColor = SettingsColor70;
            toggle.OffPressedColor = SettingsColor70;

            toggle.CircleScale = 0.7f;
            toggle.ShowMark = true;
            toggle.textPadding = new RectOffset(17, 11, 5, 0);
            toggle.size = new Vector2(60f, 30f);
        }

        #endregion

        #region SCROLLBAR

        public static void DefaultStyle(this CustomUIScrollbar scrollbar)
        {
            scrollbar.AtlasTrack = CommonTextures.Atlas;
            scrollbar.TrackSprite = CommonTextures.FieldSingle;
            scrollbar.TrackColor = DarkPrimaryColor30;

            scrollbar.AtlasThumb = CommonTextures.Atlas;
            scrollbar.ThumbSprite = CommonTextures.FieldSingle;
            scrollbar.ThumbColor = DarkPrimaryColor50;

            scrollbar.DefaultValue();
        }

        public static void SettingsStyle(this CustomUIScrollbar scrollbar)
        {
            scrollbar.AtlasTrack = CommonTextures.Atlas;
            scrollbar.TrackSprite = CommonTextures.FieldSingle;
            scrollbar.TrackColor = SettingsColor30;

            scrollbar.AtlasThumb = CommonTextures.Atlas;
            scrollbar.ThumbSprite = CommonTextures.FieldSingle;
            scrollbar.ThumbColor = SettingsColor50;

            scrollbar.DefaultValue();
        }

        private static void DefaultValue(this CustomUIScrollbar scrollbar)
        {
            scrollbar.Increment = 50f;
            scrollbar.AutoHide = true;

            switch (scrollbar.Orientation)
            {
                case UIOrientation.Horizontal:
                    scrollbar.height = 12f;
                    scrollbar.MinThumbSize = new Vector2(20f, 4f);
                    scrollbar.TrackPadding = new RectOffset(4, 4, 0, 4);
                    scrollbar.ThumbPadding = new RectOffset(4, 4, 0, 4);
                    break;
                case UIOrientation.Vertical:
                    scrollbar.width = 12f;
                    scrollbar.MinThumbSize = new Vector2(4f, 20f);
                    scrollbar.TrackPadding = new RectOffset(0, 4, 4, 4);
                    scrollbar.ThumbPadding = new RectOffset(0, 4, 4, 4);
                    break;
            }
        }

        #endregion

        #region CHECKBOX

        public static void SettingsStyle(this CustomUICheckBox checkBox)
        {
            checkBox.Atlas = CommonTextures.Atlas;
            checkBox.MarkSize = new Vector2(14f, 14f);
            checkBox.CheckedSprite = CommonTextures.RadioChecked;
            checkBox.UncheckedSprite = CommonTextures.RadioUnchecked;
            checkBox.CheckedNormalColor = NormalBlue;
            checkBox.TextPadding.left = 10;
        }

        #endregion

    }

    public class ControlStyle
    {
        public static ControlStyle Default { get; } = new ControlStyle()
        {
            TextField = new TextFieldStyle()
            {
                Colors = new ColorSet(ComponentStyle.FieldNormalColor, ComponentStyle.FieldHoveredColor, ComponentStyle.FieldHoveredColor, ComponentStyle.FieldFocusedColor, ComponentStyle.FieldDisabledColor),
                TextColor = ComponentStyle.DarkPrimaryColor100,
                SelectionColor = ComponentStyle.DarkPrimaryColor0,
            },
            Segmented = new ButtonStyle()
            {
                BgColors = new ColorSet(ComponentStyle.FieldNormalColor, ComponentStyle.FieldHoveredColor, ComponentStyle.FieldHoveredColor, ComponentStyle.FieldFocusedColor, ComponentStyle.FieldDisabledColor),
                SelBgColors = new ColorSet(ComponentStyle.FieldFocusedColor, ComponentStyle.FieldFocusedColor, ComponentStyle.FieldFocusedColor, ComponentStyle.FieldFocusedColor, ComponentStyle.FieldDisabledFocusedColor),

                FgColors = new ColorSet(ComponentStyle.DarkPrimaryColor100),
                SelFgColors = new ColorSet(ComponentStyle.DarkPrimaryColor100),

                TextColors = new ColorSet(ComponentStyle.DarkPrimaryColor100),
                SelTextColors = new ColorSet(ComponentStyle.DarkPrimaryColor100),
            },
            Button = new ButtonStyle()
            {
                BgColors = new ColorSet(ComponentStyle.ButtonNormal, ComponentStyle.ButtonHovered, ComponentStyle.ButtonPressed, ComponentStyle.ButtonNormal, ComponentStyle.ButtonFocused),
                SelBgColors = new ColorSet(),

                FgColors = new ColorSet(ComponentStyle.ButtonNormal),
                SelFgColors = new ColorSet(),

                TextColors = new ColorSet(Color.black, Color.black, Color.white, Color.black, Color.white),
                SelTextColors = new ColorSet(),
            },
            DropDown = new DropDownStyle()
            {

            },
            Toggle = new ToggleStyle()
            {
                OnColors = new ColorSet(ComponentStyle.FieldFocusedColor, ComponentStyle.FieldFocusedColor, ComponentStyle.FieldFocusedColor, ComponentStyle.FieldFocusedColor, ComponentStyle.FieldDisabledFocusedColor),
                OffColors = new ColorSet(ComponentStyle.FieldNormalColor, ComponentStyle.FieldHoveredColor, ComponentStyle.FieldHoveredColor, ComponentStyle.FieldNormalColor, ComponentStyle.FieldDisabledColor)
            },
        };

        public TextFieldStyle TextField { get; set; }
        public ButtonStyle Segmented { get; set; }
        public ButtonStyle Button { get; set; }
        public DropDownStyle DropDown { get; set; }
        public ToggleStyle Toggle { get; set; }

        public struct ColorSet
        {
            public Color32 normal;
            public Color32 hovered;
            public Color32 pressed;
            public Color32 focused;
            public Color32 disabled;

            public ColorSet(Color32 normal, Color32 hovered, Color32 pressed, Color32 focused, Color32 disabled)
            {
                this.normal = normal;
                this.hovered = hovered;
                this.pressed = pressed;
                this.focused = focused;
                this.disabled = disabled;
            }
            public ColorSet(Color32 color)
            {
                this.normal = color;
                this.hovered = color;
                this.pressed = color;
                this.focused = color;
                this.disabled = color;
            }

            public static implicit operator UI.ColorSet(ColorSet set) => new UI.ColorSet(set.normal, set.hovered, set.pressed, set.focused, set.disabled);
        }
    }
    public class BaseStyle
    {

    }
    public class TextFieldStyle : BaseStyle
    {
        public ControlStyle.ColorSet Colors { get; set; }
        public Color32 SelectionColor { get; set; }
        public Color32 TextColor { get; set; }
    }
    public class ButtonStyle : BaseStyle
    {
        public ControlStyle.ColorSet BgColors { get; set; }
        public ControlStyle.ColorSet SelBgColors { get; set; }
        public ControlStyle.ColorSet FgColors { get; set; }
        public ControlStyle.ColorSet SelFgColors { get; set; }
        public ControlStyle.ColorSet TextColors { get; set; }
        public ControlStyle.ColorSet SelTextColors { get; set; }
    }
    public class DropDownStyle : BaseStyle
    {
        public ControlStyle.ColorSet BgColors { get; set; }
        public ControlStyle.ColorSet FgColors { get; set; }
        public ControlStyle.ColorSet TextColors { get; set; }

        public Color32 PopupColor { get; set; }

        public Color32 EntityHoveredColor { get; set; }
        public Color32 EntitySelectedColor { get; set; }
    }
    public class ToggleStyle : BaseStyle
    {
        public ControlStyle.ColorSet OnColors { get; set; }
        public ControlStyle.ColorSet OffColors { get; set; }
    }

    public struct SpriteSet
    {
        public string normal;
        public string hovered;
        public string pressed;
        public string focused;
        public string disabled;

        public SpriteSet(string normal, string hovered, string pressed, string focused, string disabled)
        {
            this.normal = normal;
            this.hovered = hovered;
            this.pressed = pressed;
            this.focused = focused;
            this.disabled = disabled;
        }
        public SpriteSet(string sprite)
        {
            this.normal = sprite;
            this.hovered = sprite;
            this.pressed = sprite;
            this.focused = sprite;
            this.disabled = sprite;
        }
    }

    [Obsolete]
    public struct ColorSet
    {
        public Color32? normal;
        public Color32? hovered;
        public Color32? pressed;
        public Color32? focused;
        public Color32? disabled;

        public ColorSet(Color32? normal, Color32? hovered, Color32? pressed, Color32? focused, Color32? disabled)
        {
            this.normal = normal;
            this.hovered = hovered;
            this.pressed = pressed;
            this.focused = focused;
            this.disabled = disabled;
        }
        public ColorSet(Color32? color)
        {
            this.normal = color;
            this.hovered = color;
            this.pressed = color;
            this.focused = color;
            this.disabled = color;
        }
    }

    public struct ItemStyle
    {
        public UITextureAtlas backgroundAtlas;
        public UITextureAtlas foregroundAtlas;

        public SpriteSet spritesBg;
        public SpriteSet selectedSpritesBg;
    }
}
