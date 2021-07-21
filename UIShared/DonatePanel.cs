using ColossalFramework.UI;
using ModsCommon.Utilities;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace ModsCommon.UI
{
    public class DonatePanel : CustomUIPanel
    {
        private CustomUILabel Label { get; }
        private CustomUIButton Patreon { get; }
        private CustomUIButton PayPal { get; }
        private Vector2 ButtonSize { get; set; } = new Vector2(300f, 75f);

        public DonatePanel()
        {
            Label = AddUIComponent<CustomUILabel>();
            Label.autoHeight = true;
            Label.wordWrap = true;
            Label.textAlignment = UIHorizontalAlignment.Center;
            Label.relativePosition = new Vector3(0f, 5f);

            Patreon = AddButton(CommonTextures.Patreon, Utility.OpenPatreon);
            PayPal = AddButton(CommonTextures.PayPal, Utility.OpenPayPal);

            eventSizeChanged += OnSizeChanged;

            SetHeight();
            SetPositions();
        }

        private CustomUIButton AddButton(string sprite, Action onClick)
        {
            var button = AddUIComponent<CustomUIButton>();
            button.size = new Vector2(200f, 50f);
            button.atlas = CommonTextures.Atlas;
            button.normalBgSprite = sprite;
            button.hoveredColor = new Color32(224, 224, 224, 255);

            button.eventClicked += (_, _) => onClick?.Invoke();
            return button;
        }
        private void SetHeight()
        {
            height = 5f + Label.height + 10f + ButtonSize.y + ButtonSize.y / 5f;
        }
        private void SetPositions()
        {
            var height = 5f + Label.height + 10f;
            Patreon.relativePosition = new Vector2(width / 2f - ButtonSize.x - 5f, height);
            PayPal.relativePosition = new Vector2(width / 2f + 5f, height);
        }
        private void OnSizeChanged(UIComponent component, Vector2 value)
        {
            eventSizeChanged -= OnSizeChanged;

            Label.width = width;
            SetHeight();
            SetPositions();

            eventSizeChanged += OnSizeChanged;
        }

        public void Init(string modName, Vector2 buttonSize, float textScale = 1f)
        {
            Label.textScale = textScale;
            Label.text = string.Format(CommonLocalize.Setting_Donate, modName);
            ButtonSize = buttonSize;
            Patreon.size = ButtonSize;
            PayPal.size = ButtonSize;

            SetHeight();
            SetPositions();
        }
    }
}
