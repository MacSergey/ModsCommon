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

        public virtual void AddItem(ValueType item) => AddItem(new DropDownItem<ValueType>(item, (OptionData)item.ToString()));
        public virtual void AddItem(ValueType item, string label) => AddItem(new DropDownItem<ValueType>(item, (OptionData)label));
        public virtual void AddItem(ValueType item, OptionData optionData) => AddItem(new DropDownItem<ValueType>(item, optionData));
        protected override void SelectObject(DropDownItem<ValueType> item) => OnSelectObject?.Invoke(item.value);
        protected override void SetPopupStyle() => Popup.PopupDefaultStyle();
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

        public int Level => 0;
        public void StopLayout() { }
        public void StartLayout(bool layoutNow = true) { }
        public void PauseLayout(Action action) => action?.Invoke();
    }
    public abstract class SimpleEntity<ValueType> : PopupEntity<DropDownItem<ValueType>>
    {
        public SimpleEntity()
        {
            textHorizontalAlignment = UIHorizontalAlignment.Left;
            textPadding = new RectOffset(8, 40, 3, 0);
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
            entity.EntityStyle<DropDownItem<ValueType>, EntityType>();
            entity.textScale = EntityTextScale;
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
            if (obj is DropDownItem<ValueType> item)
                return value.Equals(item.value);
            else
                return false;
        }
        public override int GetHashCode() => value.GetHashCode();
    }

    public class StringDropDown : SimpleDropDown<string, StringDropDown.StringEntity, StringDropDown.StringPopup>
    {
        public class StringEntity : SimpleEntity<string> { }
        public class StringPopup : SimplePopup<string, StringEntity> { }
    }
}
