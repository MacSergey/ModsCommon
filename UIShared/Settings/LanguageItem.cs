using ModsCommon.Utilities;
using System;
using ColossalFramework.UI;
using UnityEngine;

namespace ModsCommon.UI
{
    public class LanguageSettingsItem : ContentSettingsItem
    {
        protected override RectOffset ItemsPadding => new RectOffset(0, 0, 5, 5);
        public LanguageDropDown DropDown { get; }
        public LanguageSettingsItem()
        {
            DropDown = Content.AddUIComponent<LanguageDropDown>();
        }
    }

    public class LanguageDropDown : SelectItemDropDown<LanguageDropDown.Language, LanguageDropDown.LanguageEntity, LanguageDropDown.LanguagePopup>
    {
        protected override Func<Language, bool> Selector => null;
        protected override Func<Language, Language, int> Sorter => null;

        public new string SelectedObject
        {
            get => base.SelectedObject.locale;
            set => base.SelectedObject = new Language(value, string.Empty, string.Empty);
        }
        public LanguageDropDown()
        {
            ComponentStyle.DropDownSettingsStyle(this, new Vector2(250f, 34f));
        }
        protected override void SetPopupStyle() => Popup.PopupSettingsStyle<Language, LanguageEntity, LanguagePopup>(34f);
        protected override void InitPopup()
        {
            Popup.AutoWidth = true;
            base.InitPopup();
        }

        public readonly struct Language
        {
            public readonly string locale;
            public readonly string label;
            public readonly string sprite;

            public Language(string locale, string label, string sprite)
            {
                this.locale = locale;
                this.label = label;
                this.sprite = sprite;
            }

            public override bool Equals(object obj)
            {
                if (obj is Language language)
                    return language.locale == locale;
                else
                    return false;
            }
            public override int GetHashCode() => locale.GetHashCode();
            public override string ToString() => $"{locale}: {label}";
        }

        public class LanguageEntity : PopupEntity<Language>
        {
            public LanguageEntity()
            {
                Atlas = CommonTextures.Atlas;
                IconMode = SpriteMode.Scale;

                HorizontalAlignment = UIHorizontalAlignment.Left;
                VerticalAlignment = UIVerticalAlignment.Middle;

                TextVerticalAlignment = UIVerticalAlignment.Middle;
                TextHorizontalAlignment = UIHorizontalAlignment.Left;
                textScale = 0.9f;
            }

            public override void SetObject(int index, Language language, bool selected)
            {
                base.SetObject(index, language, selected);

                text = language.label;
                IconSprites = language.sprite;
            }
            public override void DeInit()
            {
                base.DeInit();

                text = string.Empty;
                IconSprites = string.Empty;
            }

            protected override void OnSizeChanged()
            {
                TextPadding = new RectOffset(Mathf.CeilToInt(height) + 4, 8, 3, 0);
                IconPadding = new RectOffset(5, 0, 0, 0);
                ScaleFactor = 24f / height;
            }
        }
        public class LanguagePopup : ObjectPopup<Language, LanguageEntity>
        {
            protected override float DefaultEntityHeight => 34f;
            protected override void SetEntityStyle(LanguageEntity entity) => entity.EntitySettingsStyle<Language, LanguageEntity>();
        }
    }
}
