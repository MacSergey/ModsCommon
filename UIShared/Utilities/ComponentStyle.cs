using ColossalFramework.UI;
using ModsCommon.Utilities;
using System;
using System.Linq;
using UnityEngine;
using static ModsCommon.Utilities.CommonTextures;

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
            button.Atlas = Atlas;
            button.AllBgSprites = PanelBig;
            button.BgColors = new ColorSet(ButtonNormalColor, ButtonHoveredColor, ButtonPressedColor, ButtonNormalColor, ButtonDisabledColor);
            button.SelBgColors = new ColorSet(ButtonSelectedNormalColor, ButtonSelectedHoveredColor, ButtonSelectedPressedColor, ButtonSelectedFocusedColor, ButtonSelectedDisabledColor);
            button.AllTextColors = Color.white;
            button.Bold = true;

            button.HorizontalAlignment = UIHorizontalAlignment.Center;
            button.VerticalAlignment = UIVerticalAlignment.Middle;
            button.TextHorizontalAlignment = UIHorizontalAlignment.Center;

            button.TextPadding.top = 2;
        }
        public static void ButtonSettingsStyle(this CustomUIButton button)
        {
            button.Atlas = Atlas;
            button.AllBgSprites = PanelBig;
            button.AllBgColors = new ColorSet(SettingsColor60, SettingsColor70, SettingsColor70, SettingsColor60, SettingsColor15);
            button.AllTextColors = Color.white;

            button.HorizontalAlignment = UIHorizontalAlignment.Center;
            button.VerticalAlignment = UIVerticalAlignment.Middle;
            button.TextHorizontalAlignment = UIHorizontalAlignment.Center;

            button.TextPadding.top = 2;
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
        public static void SetDefaultStyle(this CustomUIButton button)
        {
            button.Atlas = TextureHelper.InGameAtlas;
            button.BgSprites = "ButtonWhite";
            button.BgColors = new ColorSet(ButtonNormal, ButtonHovered, ButtonPressed, ButtonNormal, Color.black);
            button.TextColors = new ColorSet(Color.black, Color.black, Color.white, Color.black, Color.white);
        }

        #endregion

        #region DROPDOWN

        public static void DropDownDefaultStyle<ObjectType, PopupType, EntityType>(this SelectItemDropDown<ObjectType, EntityType, PopupType> dropDown, Vector2? size = null)
            where PopupType : CustomUIPanel, IPopup<ObjectType, EntityType>
            where EntityType : CustomUIButton, IPopupEntity<ObjectType>
        {
            dropDown.DropDownStyle = Default.DropDown;

            dropDown.IconMode = SpriteMode.Scale;
            dropDown.ScaleFactor = 0.7f;
            dropDown.HorizontalAlignment = UIHorizontalAlignment.Right;
            dropDown.VerticalAlignment = UIVerticalAlignment.Middle;
            dropDown.IconPadding = new RectOffset(0, 5, 0, 0);

            dropDown.Entity.Padding = new RectOffset(0, 40, 0, 0);

            dropDown.size = size ?? new Vector2(230, 20);
        }
        public static void PopupDefaultStyle<ObjectType, EntityType>(this ObjectPopup<ObjectType, EntityType> popup, float? entityHeight = null)
            where EntityType : CustomUIButton, IPopupEntity<ObjectType>, IReusable
        {
            popup.PopupStyle = Default.DropDown;

            popup.EntityHeight = entityHeight ?? 20f;
            popup.MaximumSize = new Vector2(230f, 700f);
        }
        public static void EntityDefaultStyle<ObjectType, EntityType>(this EntityType entity)
            where EntityType : CustomUIButton, IPopupEntity<ObjectType>
        {
            entity.EntityStyle = Default.DropDown;
        }


        public static void DropDownMessageBoxStyle<ObjectType, PopupType, EntityType>(this SelectItemDropDown<ObjectType, EntityType, PopupType> dropDown, Vector2? size = null)
            where PopupType : ObjectPopup<ObjectType, EntityType>
            where EntityType : PopupEntity<ObjectType>
        {
            dropDown.DropDownStyle = MessageBox.DropDown;

            dropDown.IconMode = SpriteMode.Scale;
            dropDown.ScaleFactor = 0.7f;
            dropDown.IconPadding.right = 5;

            dropDown.TextVerticalAlignment = UIVerticalAlignment.Middle;
            dropDown.TextHorizontalAlignment = UIHorizontalAlignment.Left;

            dropDown.HorizontalAlignment = UIHorizontalAlignment.Right;
            dropDown.VerticalAlignment = UIVerticalAlignment.Middle;

            dropDown.size = size ?? new Vector2(230, 20);
        }
        public static void PopupMessageBoxStyle<ObjectType, EntityType>(this ObjectPopup<ObjectType, EntityType> popup, float? entityHeight = null)
            where EntityType : PopupEntity<ObjectType>
        {
            popup.PopupStyle = MessageBox.DropDown;

            popup.EntityHeight = entityHeight ?? 20f;
            popup.MaximumSize = new Vector2(230f, 700f);
            popup.ItemsPadding = new RectOffset(4, 4, 4, 4);
        }
        public static void EntityMessageBoxStyle<ObjectType>(this PopupEntity<ObjectType> entity)
        {
            entity.EntityStyle = MessageBox.DropDown;
        }


        public static void DropDownSettingsStyle<ObjectType, EntityType, PopupType>(this SelectItemDropDown<ObjectType, EntityType, PopupType> dropDown, Vector2? size = null)
            where PopupType : ObjectPopup<ObjectType, EntityType>
            where EntityType : PopupEntity<ObjectType>
        {
            dropDown.DropDownStyle = Settings.DropDown;

            dropDown.IconMode = SpriteMode.Scale;
            dropDown.ScaleFactor = 0.7f;
            dropDown.IconPadding.right = 5;

            dropDown.TextVerticalAlignment = UIVerticalAlignment.Middle;
            dropDown.TextHorizontalAlignment = UIHorizontalAlignment.Left;

            dropDown.HorizontalAlignment = UIHorizontalAlignment.Right;
            dropDown.VerticalAlignment = UIVerticalAlignment.Middle;

            dropDown.size = size ?? new Vector2(230, 20);
        }
        public static void PopupSettingsStyle<ObjectType, EntityType, PopupType>(this PopupType popup, float? entityHeight = null)
            where PopupType : CustomUIPanel, IPopup<ObjectType, EntityType>
            where EntityType : CustomUIButton, IPopupEntity<ObjectType>
        {
            popup.PopupStyle = Settings.DropDown;

            popup.EntityHeight = entityHeight ?? 20f;
            popup.MaximumSize = new Vector2(230f, 700f);
            popup.ItemsPadding = new RectOffset(4, 4, 4, 4);
        }
        public static void EntitySettingsStyle<ObjectType, EntityType>(this EntityType entity)
            where EntityType : CustomUIButton, IPopupEntity<ObjectType>
        {
            entity.EntityStyle = Settings.DropDown;
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
            textField.TextFieldStyle = Default.TextField;

            textField.isInteractive = true;
            textField.builtinKeyNavigation = true;
            textField.CursorWidth = 1;
            textField.CursorBlinkTime = 0.45f;
            textField.SelectOnFocus = true;

            textField.verticalAlignment = UIVerticalAlignment.Middle;
            textField.Padding = new RectOffset(0, 0, 6, 0);
        }
        public static void SettingsStyle(this CustomUITextField textField)
        {
            textField.TextFieldStyle = Settings.TextField;
        }
        #endregion

        #region TAB STRIPE

        public static void DefaultStyle<TabType>(this TabStrip<TabType> tabStrip)
            where TabType : Tab
        {
            tabStrip.Atlas = Atlas;
            tabStrip.BackgroundSprite = PanelBig;

            tabStrip.TabSpacingHorizontal = 4;
            tabStrip.TabSpacingVertical = 4;

            tabStrip.TabAtlas = Atlas;
            tabStrip.TabNormalSprite = PanelSmall;
            tabStrip.TabHoveredSprite = PanelSmall;
            tabStrip.TabPressedSprite = PanelSmall;
            tabStrip.TabFocusedSprite = PanelSmall;
            tabStrip.TabDisabledSprite = PanelSmall;
        }
        public static void SettingsStyle<TabType>(this TabStrip<TabType> tabStrip)
            where TabType : Tab
        {
            tabStrip.Atlas = Atlas;
            tabStrip.BackgroundSprite = PanelBig;
            tabStrip.BgColors = SettingsColor25;

            tabStrip.TabSpacingHorizontal = 4;
            tabStrip.TabSpacingVertical = 4;

            tabStrip.TabAtlas = Atlas;
            tabStrip.TabNormalSprite = PanelSmall;
            tabStrip.TabHoveredSprite = PanelSmall;
            tabStrip.TabPressedSprite = PanelSmall;
            tabStrip.TabFocusedSprite = PanelSmall;
            tabStrip.TabDisabledSprite = PanelSmall;

            tabStrip.TabColor = SettingsColor25;
            tabStrip.TabHoveredColor = SettingsColor45;
            tabStrip.TabPressedColor = SettingsColor55;
            tabStrip.TabFocusedColor = NormalBlue;
        }

        #endregion

        #region TOGGLE

        public static void DefaultStyle(this CustomUIToggle toggle)
        {
            toggle.ToggleStyle = Default.Toggle;

            toggle.textScale = 0.8f;
            toggle.CircleScale = 0.7f;
            toggle.ShowMark = true;
            toggle.TextPadding = new RectOffset(12, 6, 5, 0);
            toggle.size = new Vector2(42f, 22f);
        }

        public static void SettingsStyle(this CustomUIToggle toggle)
        {
            toggle.ToggleStyle = Settings.Toggle;

            toggle.CircleScale = 0.7f;
            toggle.ShowMark = true;
            toggle.TextPadding = new RectOffset(17, 11, 5, 0);
            toggle.size = new Vector2(60f, 30f);
        }

        #endregion

        #region SCROLLBAR

        public static void DefaultStyle(this CustomUIScrollbar scrollbar)
        {
            scrollbar.AtlasTrack = Atlas;
            scrollbar.TrackSprite = FieldSingle;
            scrollbar.TrackColor = DarkPrimaryColor30;

            scrollbar.AtlasThumb = Atlas;
            scrollbar.ThumbSprite = FieldSingle;
            scrollbar.ThumbColor = DarkPrimaryColor50;

            scrollbar.DefaultValue();
        }

        public static void SettingsStyle(this CustomUIScrollbar scrollbar)
        {
            scrollbar.AtlasTrack = Atlas;
            scrollbar.TrackSprite = FieldSingle;
            scrollbar.TrackColor = SettingsColor30;

            scrollbar.AtlasThumb = Atlas;
            scrollbar.ThumbSprite = FieldSingle;
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
            checkBox.Atlas = Atlas;
            checkBox.MarkSize = new Vector2(14f, 14f);
            checkBox.CheckedSprite = RadioChecked;
            checkBox.UncheckedSprite = RadioUnchecked;
            checkBox.CheckedNormalColor = NormalBlue;
            checkBox.TextPadding.left = 10;
        }

        #endregion


        public static ControlStyle Default { get; } = new ControlStyle()
        {
            TextField = new TextFieldStyle()
            {
                BgAtlas = Atlas,
                FgAtlas = Atlas,

                BgSprites = FieldSingle,
                BgColors = new ColorSet(FieldNormalColor, FieldHoveredColor, FieldHoveredColor, FieldFocusedColor, FieldDisabledColor),

                TextColors = DarkPrimaryColor100,

                SelectionSprite = Empty,
                SelectionColor = DarkPrimaryColor0,
            },
            Segmented = new SegmentedStyle()
            {
                Single = new ButtonStyle()
                {
                    BgAtlas = Atlas,
                    IconAtlas = Atlas,

                    AllBgSprites = FieldSingle,
                    BgColors = new ColorSet(FieldNormalColor, FieldHoveredColor, FieldHoveredColor, FieldFocusedColor, FieldDisabledColor),
                    SelBgColors = new ColorSet(FieldFocusedColor, FieldFocusedColor, FieldFocusedColor, FieldFocusedColor, FieldDisabledFocusedColor),

                    IconColors = new ColorSet(Color.white, Color.white, Color.white, Color.white, Color.black),
                    SelIconColors = new ColorSet(Color.white),

                    AllTextColors = new ColorSet(DarkPrimaryColor100),
                },
                Left = new ButtonStyle()
                {
                    BgAtlas = Atlas,
                    IconAtlas = Atlas,

                    AllBgSprites = FieldLeft,
                    BgColors = new ColorSet(FieldNormalColor, FieldHoveredColor, FieldHoveredColor, FieldFocusedColor, FieldDisabledColor),
                    SelBgColors = new ColorSet(FieldFocusedColor, FieldFocusedColor, FieldFocusedColor, FieldFocusedColor, FieldDisabledFocusedColor),

                    AllIconColors = new ColorSet(DarkPrimaryColor100),
                    AllTextColors = new ColorSet(DarkPrimaryColor100),
                },
                Middle = new ButtonStyle()
                {
                    BgAtlas = Atlas,
                    IconAtlas = Atlas,

                    AllBgSprites = FieldMiddle,
                    BgColors = new ColorSet(FieldNormalColor, FieldHoveredColor, FieldHoveredColor, FieldFocusedColor, FieldDisabledColor),
                    SelBgColors = new ColorSet(FieldFocusedColor, FieldFocusedColor, FieldFocusedColor, FieldFocusedColor, FieldDisabledFocusedColor),

                    AllIconColors = new ColorSet(DarkPrimaryColor100),
                    AllTextColors = new ColorSet(DarkPrimaryColor100),
                },
                Right = new ButtonStyle()
                {
                    BgAtlas = Atlas,
                    IconAtlas = Atlas,

                    AllBgSprites = FieldRight,
                    BgColors = new ColorSet(FieldNormalColor, FieldHoveredColor, FieldHoveredColor, FieldFocusedColor, FieldDisabledColor),
                    SelBgColors = new ColorSet(FieldFocusedColor, FieldFocusedColor, FieldFocusedColor, FieldFocusedColor, FieldDisabledFocusedColor),

                    AllIconColors = new ColorSet(DarkPrimaryColor100),
                    AllTextColors = new ColorSet(DarkPrimaryColor100),
                }
            },
            SmallButton = new ButtonStyle()
            {
                BgColors = new ColorSet(ButtonNormal, ButtonHovered, ButtonPressed, ButtonNormal, ButtonFocused),
                SelBgColors = new ColorSet(),

                IconColors = new ColorSet(ButtonNormal),
                SelIconColors = new ColorSet(),

                TextColors = new ColorSet(Color.black, Color.black, Color.white, Color.black, Color.white),
                SelTextColors = new ColorSet(),
            },
            LargeButton = new ButtonStyle()
            {
                BgColors = new ColorSet(ButtonNormal, ButtonHovered, ButtonPressed, ButtonNormal, ButtonFocused),
                SelBgColors = new ColorSet(),

                IconColors = new ColorSet(ButtonNormal),
                SelIconColors = new ColorSet(),

                TextColors = new ColorSet(Color.black, Color.black, Color.white, Color.black, Color.white),
                SelTextColors = new ColorSet(),
            },
            DropDown = new DropDownStyle()
            {
                BgAtlas = Atlas,
                IconAtlas = Atlas,

                BgSprites = FieldSingle,
                IconSprites = ArrowDown,

                BgColors = new ColorSet(FieldNormalColor, FieldHoveredColor, FieldHoveredColor, FieldNormalColor, FieldDisabledColor),
                IconColors = Color.black,


                PopupAtlas = Atlas,
                PopupSprite = FieldSingle,
                PopupColor = FieldHoveredColor,


                EntityAtlas = Atlas,

                EntitySprites = new SpriteSet(default, FieldSingle, default, default, default),
                EntitySelSprites = FieldSingle,

                EntityColors = FieldNormalColor,
                EntitySelColors = FieldFocusedColor,
            },
            Toggle = new ToggleStyle()
            {
                BgAtlas = Atlas,
                MarkAtlas = Atlas,

                OnBgSprites = ToggleBackgroundSmall,
                OffBgSprites = ToggleBackgroundSmall,

                OnMarkSprites = ToggleCircle,
                OffMarkSprites = ToggleCircle,

                OnBgColors = new ColorSet(FieldFocusedColor, FieldFocusedColor, FieldFocusedColor, FieldFocusedColor, FieldDisabledFocusedColor),
                OffBgColors = new ColorSet(FieldNormalColor, FieldHoveredColor, FieldHoveredColor, FieldNormalColor, FieldDisabledColor),

                OnMarkColors = Color.white,
                OffMarkColors = Color.white,
            },
            ColorPicker = new ColorPickerStyle()
            {
                BgAtlas = Atlas,
                FgAtlas = Atlas,

                BgSprites = PanelSmall,
                BgColors = new ColorSet(FieldNormalColor, FieldHoveredColor, FieldHoveredColor, FieldNormalColor, FieldDisabledColor),

                FgSprites = PanelSmall,
            },
            Label = new LabelStyle()
            {
                NormalTextColor = Color.white,
                DisabledTextColor = Color.white,
            },
            PropertyPanel = new PropertyPanelStyle()
            {
                BgAtlas = Atlas,
                BgSprites = PanelLarge,
                BgColors = new Color32(82, 101, 117, 255),
                MaskSprite = OpacitySliderMask,
            },
            HeaderContent = new HeaderStyle()
            {
                MainBgColors = new ColorSet(default, DarkPrimaryColor45, DarkPrimaryColor55, default, default),
                MainIconColors = new ColorSet(Color.white, Color.white, Color.white, Color.white, DarkPrimaryColor55),
                AdditionalBgColors = new ColorSet(default, DarkPrimaryColor45, DarkPrimaryColor45, default, default),
                AdditionalIconColors = new ColorSet(Color.white, Color.white, Color.white, Color.white, DarkPrimaryColor55),
            }
        };

        public static ControlStyle MessageBox { get; } = new ControlStyle()
        {
            DropDown = new DropDownStyle()
            {
                BgAtlas = Atlas,
                IconAtlas = Atlas,

                BgSprites = FieldSingle,
                IconSprites = ArrowDown,

                BgColors = new ColorSet(DarkPrimaryColor55, DarkPrimaryColor45, DarkPrimaryColor35, DarkPrimaryColor55, DarkPrimaryColor15),
                IconColors = SettingsColor15,


                PopupAtlas = Atlas,
                PopupSprite = FieldSingle,
                PopupColor = DarkPrimaryColor55,


                EntityAtlas = Atlas,

                EntitySprites = new SpriteSet(default, FieldSingle, default, default, default),
                EntitySelSprites = FieldSingle,

                EntityColors = DarkPrimaryColor45,
                EntitySelColors = NormalBlue,
            },
        };

        public static ControlStyle Settings { get; } = new ControlStyle()
        {
            TextField = new TextFieldStyle()
            {
                BgAtlas = Atlas,
                FgAtlas = Atlas,

                BgSprites = FieldSingle,
                BgColors = new ColorSet(SettingsColor60, SettingsColor70, default, NormalBlue, SettingsColor15),

                //FgSprites = new SpriteSet(default, default, default, BorderSmall, default),
                //FgColors = NormalBlue,

                TextColors = DarkPrimaryColor100,

                SelectionSprite = Empty,
                SelectionColor = DarkPrimaryColor0,
            },
            Toggle = new ToggleStyle()
            {
                BgAtlas = Atlas,
                MarkAtlas = Atlas,

                OnBgSprites = ToggleBackground,
                OffBgSprites = ToggleBackground,

                OnMarkSprites = ToggleCircle,
                OffMarkSprites = ToggleCircle,

                OnBgColors = new ColorSet(NormalGreen, HoveredGreen, HoveredGreen, NormalGreen, default),
                OffBgColors = new ColorSet(SettingsColor60, SettingsColor70, SettingsColor70, SettingsColor60, default),

                OnMarkColors = Color.white,
                OffMarkColors = Color.white,

                AllTextColors = Color.white,
            },
            DropDown = new DropDownStyle()
            {
                BgAtlas = Atlas,
                IconAtlas = Atlas,

                BgSprites = FieldSingle,
                IconSprites = ArrowDown,

                BgColors = new ColorSet(SettingsColor60, SettingsColor70, SettingsColor70, SettingsColor60, SettingsColor15),
                IconColors = SettingsColor15,

                AllTextColors = Color.white,


                PopupAtlas = Atlas,
                PopupSprite = FieldSingle,
                PopupColor = SettingsColor25,


                EntityAtlas = Atlas,

                EntitySprites = new SpriteSet(default, FieldSingle, default, default, default),
                EntitySelSprites = FieldSingle,

                EntityColors = SettingsColor45,
                EntitySelColors = NormalBlue,
            },
        };
    }

    public class ControlStyle
    {
        public TextFieldStyle TextField { get; set; }
        public SegmentedStyle Segmented { get; set; }
        public ButtonStyle SmallButton { get; set; }
        public ButtonStyle LargeButton { get; set; }
        public DropDownStyle DropDown { get; set; }
        public ToggleStyle Toggle { get; set; }
        public ColorPickerStyle ColorPicker { get; set; }
        public LabelStyle Label { get; set; }
        public PropertyPanelStyle PropertyPanel { get; set; }
        public HeaderStyle HeaderContent { get; set; }
    }
    public class ItemStyle { }
    public class InteractiveStyle : ItemStyle
    {
        public UITextureAtlas BgAtlas { get; set; }
        public UITextureAtlas FgAtlas { get; set; }
        public UITextureAtlas IconAtlas { get; set; }

        public SpriteSet BgSprites { get; set; }
        public SpriteSet SelBgSprites { get; set; }
        public SpriteSet AllBgSprites
        {
            set
            {
                BgSprites = value;
                SelBgSprites = value;
            }
        }

        public SpriteSet FgSprites { get; set; }
        public SpriteSet SelFgSprites { get; set; }
        public SpriteSet AllFgSprites
        {
            set
            {
                FgSprites = value;
                SelFgSprites = value;
            }
        }

        public SpriteSet IconSprites { get; set; }
        public SpriteSet SelIconSprites { get; set; }
        public SpriteSet AllIconSprites
        {
            set
            {
                IconSprites = value;
                SelIconSprites = value;
            }
        }

        public ColorSet BgColors { get; set; }
        public ColorSet SelBgColors { get; set; }
        public ColorSet AllBgColors
        {
            set
            {
                BgColors = value;
                SelBgColors = value;
            }
        }

        public ColorSet FgColors { get; set; }
        public ColorSet SelFgColors { get; set; }
        public ColorSet AllFgColors
        {
            set
            {
                FgColors = value;
                SelFgColors = value;
            }
        }

        public ColorSet IconColors { get; set; }
        public ColorSet SelIconColors { get; set; }
        public ColorSet AllIconColors
        {
            set
            {
                IconColors = value;
                SelIconColors = value;
            }
        }

        public ColorSet TextColors { get; set; }
        public ColorSet SelTextColors { get; set; }
        public ColorSet AllTextColors
        {
            set
            {
                TextColors = value;
                SelTextColors = value;
            }
        }
    }
    public class TextFieldStyle : InteractiveStyle
    {
        public string SelectionSprite { get; set; }
        public Color32 SelectionColor { get; set; }
    }
    public class ButtonStyle : InteractiveStyle
    {

    }
    public class SegmentedStyle : InteractiveStyle
    {
        public ButtonStyle Single { get; set; }
        public ButtonStyle Left { get; set; }
        public ButtonStyle Middle { get; set; }
        public ButtonStyle Right { get; set; }
    }
    public class DropDownStyle : ButtonStyle
    {
        public UITextureAtlas PopupAtlas { get; set; }
        public string PopupSprite { get; set; }
        public Color32 PopupColor { get; set; }
        public RectOffset PopupItemsPadding { get; set; }


        public UITextureAtlas EntityAtlas { get; set; }

        public SpriteSet EntitySprites { get; set; }
        public SpriteSet EntitySelSprites { get; set; }

        public ColorSet EntityColors { get; set; }
        public ColorSet EntitySelColors { get; set; }
    }
    public class ToggleStyle : ButtonStyle
    {
        public UITextureAtlas MarkAtlas
        {
            get => IconAtlas;
            set => IconAtlas = value;
        }

        public SpriteSet OnBgSprites
        {
            get => SelBgSprites;
            set => SelBgSprites = value;
        }
        public SpriteSet OffBgSprites
        {
            get => BgSprites;
            set => BgSprites = value;
        }

        public SpriteSet OnFgSprites
        {
            get => SelFgSprites;
            set => SelFgSprites = value;
        }
        public SpriteSet OffFgSprites
        {
            get => FgSprites;
            set => FgSprites = value;
        }

        public SpriteSet OnMarkSprites
        {
            get => SelIconSprites;
            set => SelIconSprites = value;
        }
        public SpriteSet OffMarkSprites
        {
            get => IconSprites;
            set => IconSprites = value;
        }

        public ColorSet OnBgColors
        {
            get => SelBgColors;
            set => SelBgColors = value;
        }
        public ColorSet OffBgColors
        {
            get => BgColors;
            set => BgColors = value;
        }

        public ColorSet OnFgColors
        {
            get => SelFgColors;
            set => SelFgColors = value;
        }
        public ColorSet OffFgColors
        {
            get => FgColors;
            set => FgColors = value;
        }

        public ColorSet OnMarkColors
        {
            get => SelIconColors;
            set => SelIconColors = value;
        }
        public ColorSet OffMarkColors
        {
            get => IconColors;
            set => IconColors = value;
        }

        public ColorSet OnTextColors
        {
            get => SelTextColors;
            set => SelTextColors = value;
        }
        public ColorSet OffTextColors
        {
            get => TextColors;
            set => TextColors = value;
        }
    }
    public class CheckBoxStyle : InteractiveStyle
    {

    }
    public class ColorPickerStyle : InteractiveStyle
    {
        public TextFieldStyle TextField { get; set; }
        public ButtonStyle Button { get; set; }
    }
    public class LabelStyle : ItemStyle
    {
        public Color32 NormalTextColor { get; set; }
        public Color32 DisabledTextColor { get; set; }
    }
    public class PropertyPanelStyle : ItemStyle
    {
        public UITextureAtlas BgAtlas { get; set; }
        public SpriteSet BgSprites { get; set; }
        public ColorSet BgColors { get; set; }
        public string MaskSprite { get; set; }
    }
    public class HeaderStyle : ItemStyle
    {
        public ColorSet MainBgColors { get; set; }
        public ColorSet MainIconColors { get; set; }
        public ColorSet AdditionalBgColors { get; set; }
        public ColorSet AdditionalIconColors { get; set; }
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

        public static implicit operator SpriteSet(string sprite) => new SpriteSet(sprite);
    }
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

        public static implicit operator ColorSet(Color32 color) => new ColorSet(color);
        public static implicit operator ColorSet(Color color) => new ColorSet(color);
    }
}
