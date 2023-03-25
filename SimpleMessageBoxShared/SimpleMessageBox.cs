using ColossalFramework.UI;
using ModsCommon.Utilities;
using System;
using UnityEngine;

namespace ModsCommon.UI
{
    public enum MessageType
    {
        None,
        Regular,
        Warning
    }

    public abstract class SimpleMessageBox : MessageBoxBase
    {
        protected CustomUILabel Message { get; set; }

        public string MessageText { set => Message.text = value; }
        public float MessageScale { set => Message.textScale = value; }
        public UIHorizontalAlignment TextAlignment { set => Message.textAlignment = value; }

        private MessageType type;
        public MessageType Type
        {
            get => type;
            set
            {
                if(value != type)
                {
                    type = value;

                    Message.backgroundSprite = value == MessageType.None ? string.Empty : CommonTextures.PanelBig;
                    Message.color = value switch
                    {
                        MessageType.Regular => ComponentStyle.DarkPrimaryColor15,
                        MessageType.Warning => ComponentStyle.WarningColor,
                        _ => ComponentStyle.DarkPrimaryColor0,
                    };
                }
            }
        }

        public SimpleMessageBox()
        {
            Content.PauseLayout(() =>
            {
                Content.AutoLayoutSpace = 15;

                Message = Content.AddUIComponent<CustomUILabel>();

                Message.Bold = true;
                Message.textAlignment = UIHorizontalAlignment.Center;
                Message.verticalAlignment = UIVerticalAlignment.Middle;
                Message.textScale = 1.1f;
                Message.wordWrap = true;
                Message.autoHeight = true;
                Message.padding = new RectOffset(10, 10, 10, 10);
                Message.atlas = CommonTextures.Atlas;
                Message.minimumSize = new Vector2(0, 79);

                Type = MessageType.Warning;
            });
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
        protected CustomUIButton Button1 { get => Button; set => Button = value; }
        protected CustomUIButton Button2 { get; set; }
        public Func<bool> OnButton1Click { get => OnButtonClick; set => OnButtonClick = value; }
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
    public abstract class ErrorMessageBox : TwoButtonMessageBox
    {
        public virtual void Init<TypeMod>() where TypeMod : BaseMod<TypeMod>
        {
            CaptionText = SingletonMod<TypeMod>.Instance.NameRaw;
            Button1Text = CommonLocalize.MessageBox_OK;
        }
    }
    public class ErrorSupportMessageBox : ErrorMessageBox
    {
        public override void Init<TypeMod>()
        {
            base.Init<TypeMod>();

            Button2Text = CommonLocalize.Mod_Support;
            OnButton2Click = SingletonMod<TypeMod>.Instance.OpenSupport;
        }
    }
    public class ErrorLoadMessageBox : ThreeButtonMessageBox
    {
        public virtual void Init<TypeMod>() where TypeMod : BaseMod<TypeMod>
        {
            CaptionText = SingletonMod<TypeMod>.Instance.NameRaw;
            Button1Text = CommonLocalize.MessageBox_OK;

            MessageText = CommonLocalize.Mod_LoadedWithErrors;
            Button2Text = CommonLocalize.Dependency_Disable;
            OnButton2Click = () =>
            {
                if (SingletonMod<TypeMod>.Instance.ThisSearcher.GetPlugin() is ColossalFramework.Plugins.PluginManager.PluginInfo plugin)
                    plugin.SetState(false);

                return true;
            };
            Button3Text = CommonLocalize.Mod_Support;
            OnButton3Click = SingletonMod<TypeMod>.Instance.OpenSupport;
        }
    }
}
