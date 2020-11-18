﻿using ColossalFramework.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ModsCommon.UI
{
    public abstract class SimpleMessageBox : MessageBoxBase
    {
        private UILabel Message { get; set; }

        public string MessageText { set => Message.text = value; }
        public float MessageScale { set => Message.textScale = value; }
        public UIHorizontalAlignment TextAlignment { set => Message.textAlignment = value; }

        protected override void FillContent()
        {
            AddMessage();
        }
        private void AddMessage()
        {
            Message = ScrollableContent.AddUIComponent<UILabel>();
            Message.textAlignment = UIHorizontalAlignment.Center;
            Message.verticalAlignment = UIVerticalAlignment.Middle;
            Message.textScale = 1.1f;
            Message.wordWrap = true;
            Message.autoHeight = true;
            Message.minimumSize = new Vector2(Width - 2 * Padding, 78);
            Message.size = new Vector2(Width - 2 * Padding, 78);
            Message.relativePosition = new Vector3(17, 7);
            Message.anchor = UIAnchorStyle.CenterHorizontal | UIAnchorStyle.CenterVertical;
            Message.eventTextChanged += (UIComponent component, string value) => Message.PerformLayout();
        }
    }

    public class OneButtonMessageBox : SimpleMessageBox
    {
        private UIButton Button { get; set; }
        public Func<bool> OnButtonClick { get; set; }
        public string ButtonText { set => Button.text = value; }

        public OneButtonMessageBox()
        {
            Button = AddButton(1, 1, ButtonClick);
        }
        protected virtual void ButtonClick()
        {
            if (OnButtonClick?.Invoke() != false)
                Close();
        }
    }
    public class TwoButtonMessageBox : SimpleMessageBox
    {
        private UIButton Button1 { get; set; }
        private UIButton Button2 { get; set; }
        public Func<bool> OnButton1Click { get; set; }
        public Func<bool> OnButton2Click { get; set; }
        public string Button1Text { set => Button1.text = value; }
        public string Button2Text { set => Button2.text = value; }
        public TwoButtonMessageBox()
        {
            Button1 = AddButton(1, 2, Button1Click);
            Button2 = AddButton(2, 2, Button2Click);
        }
        protected virtual void Button1Click()
        {
            if (OnButton1Click?.Invoke() != false)
                Close();
        }
        protected virtual void Button2Click()
        {
            if (OnButton2Click?.Invoke() != false)
                Close();
        }
    }
    public class ThreeButtonMessageBox : SimpleMessageBox
    {
        private UIButton Button1 { get; set; }
        private UIButton Button2 { get; set; }
        private UIButton Button3 { get; set; }
        public Func<bool> OnButton1Click { get; set; }
        public Func<bool> OnButton2Click { get; set; }
        public Func<bool> OnButton3Click { get; set; }
        public string Button1Text { set => Button1.text = value; }
        public string Button2Text { set => Button2.text = value; }
        public string Button3Text { set => Button3.text = value; }
        public ThreeButtonMessageBox()
        {
            Button1 = AddButton(1, 3, Button1Click);
            Button2 = AddButton(2, 3, Button2Click);
            Button3 = AddButton(3, 3, Button3Click);
        }
        protected virtual void Button1Click()
        {
            if (OnButton1Click?.Invoke() != false)
                Close();
        }
        protected virtual void Button2Click()
        {
            if (OnButton2Click?.Invoke() != false)
                Close();
        }
        protected virtual void Button3Click()
        {
            if (OnButton3Click?.Invoke() != false)
                Close();
        }
    }
}
