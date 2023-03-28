using ColossalFramework.UI;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ModsCommon.UI
{
    public abstract class SimpleDropDown<ValueType, EntityType, PopupType> : SelectItemDropDown<DropDownItem<ValueType>, EntityType, PopupType>, IUIOnceSelector<ValueType>
        where EntityType : SimpleEntity<ValueType>
        where PopupType : SimplePopup<ValueType, EntityType>
    {
        bool IReusable.InCache { get; set; }

        public new event Action<ValueType> OnSelectObject;

        protected override Func<DropDownItem<ValueType>, bool> Selector => null;
        protected override Func<DropDownItem<ValueType>, DropDownItem<ValueType>, int> Sorter => null;

        public new ValueType SelectedObject
        {
            get => base.SelectedObject.value;
            set => base.SelectedObject = new DropDownItem<ValueType>(value, default);
        }

        public bool UseWheel { get; set; }
        public bool WheelTip
        {
            set => tooltip = value ? CommonLocalize.ListPanel_ScrollWheel : string.Empty;
        }

        private float entityTextScale = 0.7f;
        public float EntityTextScale
        {
            get => entityTextScale;
            set
            {
                if (value != entityTextScale)
                {
                    entityTextScale = value;
                    Entity.textScale = entityTextScale;
                }
            }
        }
        Func<ValueType, ValueType, bool> IUISelector<ValueType>.IsEqualDelegate
        {
            set => IsEqualDelegate = (x, y) => value(x.value, y.value);
        }

        public SimpleDropDown() : base()
        {
            Entity.textScale = EntityTextScale;
        }

        public virtual void AddItem(ValueType item) => AddItem(new DropDownItem<ValueType>(item, (OptionData)item.ToString()));
        public virtual void AddItem(ValueType item, string label) => AddItem(new DropDownItem<ValueType>(item, (OptionData)label));
        public virtual void AddItem(ValueType item, OptionData optionData) => AddItem(new DropDownItem<ValueType>(item, optionData));
        protected override void SelectObject(DropDownItem<ValueType> item)
        {
            base.SelectObject(item);
            OnSelectObject?.Invoke(item.value);
        }
        protected override void SetPopupStyle()
        {
            Popup.PopupDefaultStyle();
            if (DropDownStyle != null)
                Popup.PopupStyle = DropDownStyle;
        }
        protected override void InitPopup()
        {
            Popup.MaximumSize = new Vector2(width, 700f);
            Popup.EntityHeight = height;
            Popup.width = width;
            Popup.MaxVisibleItems = 0;
            Popup.EntityTextScale = EntityTextScale;
            base.InitPopup();
        }
        public void DeInit()
        {
            Clear();
            UseWheel = false;
            WheelTip = false;
            entityTextScale = 0.7f;
        }

        public void SetDefaultStyle(Vector2? size = null)
        {
            this.DropDownDefaultStyle(size);
        }

        public bool IsLayoutSuspended => false;
        public Vector2 ItemSize => size;
        public RectOffset LayoutPadding => new RectOffset();

        public void StopLayout() { }
        public void StartLayout(bool layoutNow = true, bool force = false) { }
        public void PauseLayout(Action action, bool layoutNow = true, bool force = false) => action?.Invoke();
        public void Ignore(UIComponent item, bool ignore) { }
    }
    public abstract class SimpleEntity<ValueType> : PopupEntity<DropDownItem<ValueType>>
    {
        public SimpleEntity()
        {
            TextHorizontalAlignment = UIHorizontalAlignment.Left;
            TextPadding = new RectOffset(8, 40, 3, 0);
        }

        public override void SetObject(int index, DropDownItem<ValueType> value, bool selected)
        {
            base.SetObject(index, value, selected);
            text = value.optionData.label;
        }
    }
    public abstract class SimplePopup<ValueType, EntityType> : ObjectPopup<DropDownItem<ValueType>, EntityType>
        where EntityType : SimpleEntity<ValueType>
    {
        public float EntityTextScale { get; set; } = 0.7f;
        protected override void SetEntityStyle(EntityType entity)
        {
            entity.EntityDefaultStyle<DropDownItem<ValueType>, EntityType>();
            entity.textScale = EntityTextScale;

            if (PopupStyle != null)
                entity.EntityStyle = PopupStyle;
        }
        public override void DeInit()
        {
            base.DeInit();
            EntityTextScale = 0.7f;
        }
    }
    public readonly struct DropDownItem<ValueType>
    {
        public readonly ValueType value;
        public readonly OptionData optionData;

        public DropDownItem(ValueType value, OptionData optionData)
        {
            this.value = value;
            this.optionData = optionData;
        }

        public override bool Equals(object obj)
        {
            if (obj is not DropDownItem<ValueType> item)
                return false;
            else if (value == null)
                return item.value == null;
            else
                return value.Equals(item.value);
        }
        public override int GetHashCode() => value.GetHashCode();
    }

    public class StringDropDown : SimpleDropDown<string, StringDropDown.StringEntity, StringDropDown.StringPopup>
    {
        public class StringEntity : SimpleEntity<string> { }
        public class StringPopup : SimplePopup<string, StringEntity> { }
    }
}
