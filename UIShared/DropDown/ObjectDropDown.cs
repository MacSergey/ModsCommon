﻿using ColossalFramework.UI;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ModsCommon.UI
{
    public delegate void PopupStyleDelegate<ObjectType, EntityType, PopupType>(PopupType popup, ref bool overridden)
        where PopupType : CustomUIPanel, IPopup<ObjectType, EntityType>
        where EntityType : CustomUIButton, IPopupEntity<ObjectType>;

    public delegate void EntityStyleDelegate<ObjectType, EntityType>(EntityType entity, ref bool overridden)
        where EntityType : CustomUIButton, IPopupEntity<ObjectType>;

    public interface IPopup { }
    public interface IPopup<ObjectType, EntityType> : IPopup
        where EntityType : CustomUIButton, IPopupEntity<ObjectType>
    {
        event Action<ObjectType> OnSelect;
        event EntityStyleDelegate<ObjectType, EntityType> OnSetEntityStyle;

        DropDownStyle PopupStyle { set; }

        ObjectType SelectedObject { get; set; }
        Func<ObjectType, ObjectType, bool> IsEqualDelegate { set; }
        bool AutoClose { get; }

        Vector2 MaximumSize { get; set; }
        float EntityHeight { get; set; }
        RectOffset ItemsPadding { get; set; }

        void Init(IEnumerable<ObjectType> values, Func<ObjectType, bool> selector, Func<ObjectType, ObjectType, int> sorter);
        public void PauseRefreshing(Action action);
    }
    public interface IPopupEntity<ObjectType>
    {
        event Action<int, ObjectType> OnSelected;

        DropDownStyle EntityStyle { set; }

        ObjectType EditObject { get; }
        bool IsSelected { get; set; }
        int Index { get; set; }
        RectOffset Padding { get; set; }

        void SetObject(int index, ObjectType value, bool selected);
        void PerformAutoWidth();
    }

    public abstract class ObjectDropDown<ObjectType, EntityType, PopupType> : BaseDropDown<PopupType>
        where EntityType : CustomUIButton, IPopupEntity<ObjectType>
        where PopupType : CustomUIPanel, IPopup<ObjectType, EntityType>
    {
        #region PROPERTIES

        public event Action<ObjectType> OnSelectObject;
        public event PopupStyleDelegate<ObjectType, EntityType, PopupType> OnSetPopupStyle;
        public event EntityStyleDelegate<ObjectType, EntityType> OnSetEntityStyle;

        public Func<ObjectType, ObjectType, bool> IsEqualDelegate { get; set; }

        #endregion

        protected virtual void SelectObject(ObjectType value) => SelectObjectEvent(value);
        protected virtual void SelectObjectEvent(ObjectType value) => OnSelectObject?.Invoke(value);

        #region POPUP

        public override bool AutoClose
        {
            get => base.AutoClose && (Popup == null || Popup.AutoClose);
            set => base.AutoClose = value;
        }
        protected abstract IEnumerable<ObjectType> Objects { get; }
        protected abstract Func<ObjectType, bool> Selector { get; }
        protected abstract Func<ObjectType, ObjectType, int> Sorter { get; }

        protected virtual void SetPopupStyle() { }
        protected virtual void SetEntityStyle(EntityType entity, ref bool overridden) => OnSetEntityStyle?.Invoke(entity, ref overridden);

        protected virtual void InitPopup() => Popup.Init(Objects, Selector, Sorter);

        protected override void WhilePopupOpening()
        {
            base.WhilePopupOpening();

            Popup.PauseRefreshing(() =>
            {
                var overridden = false;
                OnSetPopupStyle?.Invoke(Popup, ref overridden);
                if (!overridden)
                    SetPopupStyle();

                InitPopup();
            });
        }
        protected override void SetPopupProperties()
        {
            base.SetPopupProperties();
            Popup.IsEqualDelegate = IsEqualDelegate;
        }
        protected override void AddPopupEventHandlers()
        {
            base.AddPopupEventHandlers();
            Popup.OnSelect += Select;
            Popup.OnSetEntityStyle += SetEntityStyle;
        }

        private void Select(ObjectType value)
        {
            SelectObject(value);
            ClosePopup();
        }

        private DropDownStyle style;
        public virtual DropDownStyle DropDownStyle
        {
            get => style;
            set
            {
                style = value;

                bgAtlas = value.BgAtlas;
                fgAtlas = value.FgAtlas;
                IconAtlas = value.IconAtlas;

                bgSprites = value.BgSprites;
                fgSprites = value.FgSprites;
                IconSprites = value.IconSprites;

                bgColors = value.BgColors;
                fgColors = value.FgColors;
                IconColors = value.IconColors;

                textColors = value.TextColors;

                Invalidate();
            }
        }

        #endregion
    }
}
