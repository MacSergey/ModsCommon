using ColossalFramework.UI;
using System;
using UnityEngine;

namespace ModsCommon.UI
{
    public abstract class SimpleMessageBox : MessageBoxBase
    {
        protected CustomUILabel Message { get; set; }

        public string MessageText { set => Message.text = value; }
        public float MessageScale { set => Message.textScale = value; }
        public UIHorizontalAlignment TextAlignment { set => Message.textAlignment = value; }

        public SimpleMessageBox()
        {
            Panel.StopLayout();

            Message = AddLabel();
            Message.minimumSize = new Vector2(0, 79);

            Panel.StartLayout();
        }

        protected CustomUILabel AddLabel()
        {
            var label = Panel.Content.AddUIComponent<CustomUILabel>();

            label.textAlignment = UIHorizontalAlignment.Center;
            label.verticalAlignment = UIVerticalAlignment.Middle;
            label.textScale = 1.1f;
            label.wordWrap = true;
            label.autoHeight = true;
            label.width = DefaultWidth - 2 * Padding;

            return label;
        }
    }

    public class OneButtonMessageBox : SimpleMessageBox
    {
        protected CustomUIButton Button { get; set; }
        public Func<bool> OnButtonClick { get; set; }
        public string ButtonText { set => Button.text = value; }

        public OneButtonMessageBox()
        {
            Button = AddButton(ButtonClick);
        }
        protected virtual void ButtonClick()
        {
            if (OnButtonClick?.Invoke() != false)
                Close();
        }
    }
    public class TwoButtonMessageBox : OneButtonMessageBox
    {
        private new CustomUIButton Button { get; set; }
        private new Func<bool> OnButtonClick { get; set; }

        protected CustomUIButton Button1 { get => base.Button; set => base.Button = value; }
        protected CustomUIButton Button2 { get; set; }
        public Func<bool> OnButton1Click { get => base.OnButtonClick; set => base.OnButtonClick = value; }
        public Func<bool> OnButton2Click { get; set; }
        public string Button1Text { set => Button1.text = value; }
        public string Button2Text { set => Button2.text = value; }
        public TwoButtonMessageBox()
        {
            Button2 = AddButton(Button2Click);
        }
        protected virtual void Button2Click()
        {
            if (OnButton2Click?.Invoke() != false)
                Close();
        }
    }
    public class ThreeButtonMessageBox : TwoButtonMessageBox
    {
        protected CustomUIButton Button3 { get; set; }
        public Func<bool> OnButton3Click { get; set; }
        public string Button3Text { set => Button3.text = value; }
        public ThreeButtonMessageBox()
        {
            Button3 = AddButton(Button3Click);
        }
        protected virtual void Button3Click()
        {
            if (OnButton3Click?.Invoke() != false)
                Close();
        }
    }
    public class OkMessageBox : OneButtonMessageBox
    {
        public OkMessageBox()
        {
            ButtonText = CommonLocalize.MessageBox_OK;
        }
    }
    public class YesNoMessageBox : TwoButtonMessageBox
    {
        public YesNoMessageBox()
        {
            Button1Text = CommonLocalize.MessageBox_Yes;
            Button2Text = CommonLocalize.MessageBox_No;
        }
    }
}
