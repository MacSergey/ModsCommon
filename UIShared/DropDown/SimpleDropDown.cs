using ColossalFramework.UI;
using IMT.Utilities;
using ModsCommon.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ModsCommon.UI
{
    public interface IDropDownRef : ISelectorRef { }
    public interface IDropDown<ValueType>
    {
        ValueType SelectedObject { get; set; }
        bool UseWheel { get; set; }
        bool WheelTip { set; }
    }

    public abstract class SimpleDropDown<ValueType, EntityType, PopupType, RefType> : SelectItemDropDown<DropDownItem<ValueType>, EntityType, PopupType>, ISingleSelector<ValueType, RefType>, IDropDown<ValueType>
        where EntityType : SimpleEntity<ValueType>
        where PopupType : SimplePopup<ValueType, EntityType>
        where RefType : IDropDownRef, IDropDown<ValueType>
    {
        public RefType Ref { get; }
        bool IReusable.InCache { get; set; }
        Transform IReusable.CachedTransform { get => m_CachedTransform; set => m_CachedTransform = value; }


        public new event Action<ValueType> OnSelectObject;

        protected override Func<DropDownItem<ValueType>, bool> Selector => null;
        protected override IComparer<DropDownItem<ValueType>> Comparer => null;

        public new ValueType SelectedObject
        {
            get => base.SelectedObject.value;
            set => base.SelectedObject = new DropDownItem<ValueType>(value, default);
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
        Func<ValueType, ValueType, bool> ISelector<ValueType, RefType>.IsEqualDelegate
        {
            set => IsEqualDelegate = (x, y) => value(x.value, y.value);
        }

        public SimpleDropDown() : base()
        {
            Ref = CreateRef();
            Entity.textScale = EntityTextScale;
        }
        protected abstract RefType CreateRef();

        public virtual void AddItem(ValueType item) => AddItem(new DropDownItem<ValueType>(item, (OptionData)item.ToString()));
        public virtual void AddItem(ValueType item, string label) => AddItem(new DropDownItem<ValueType>(item, (OptionData)label));
        public virtual void AddItem(ValueType item, OptionData optionData) => AddItem(new DropDownItem<ValueType>(item, optionData));
        protected override void SelectObjectEvent(DropDownItem<ValueType> item) => OnSelectObject?.Invoke(item.value);
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
        public override void DeInit()
        {
            base.DeInit();

            Clear();
            entityTextScale = 0.7f;
        }

        public void SetDefaultStyle(Vector2? size = null)
        {
            this.DropDownDefaultStyle(size);
        }

        public bool IsLayoutSuspended => false;
        public Vector2 ItemSize => size;
        public RectOffset LayoutPadding => new();

        public void StopLayout() { }
        public void StartLayout(bool layoutNow = true, bool force = false) { }
        public void PauseLayout(Action action, bool layoutNow = true, bool force = false) => action?.Invoke();
        public void Ignore(UIComponent item, bool ignore) { }
    }
    public class SimpleDropDownRef<ValueType, DropDownType> : IDropDownRef, IDropDown<ValueType>
        where DropDownType : IDropDown<ValueType>
    {
        protected DropDownType DropDown { get; }

        public SimpleDropDownRef(DropDownType dropDown)
        {
            DropDown = dropDown;
        }

        public ValueType SelectedObject 
        { 
            get => DropDown.SelectedObject; 
            set => DropDown.SelectedObject = value; 
        }
        public bool UseWheel 
        { 
            get => DropDown.UseWheel; 
            set => DropDown.UseWheel = value; 
        }
        public bool WheelTip 
        { 
            set => DropDown.WheelTip = value; 
        }
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
    public readonly struct DropDownItem<ValueType> : IComparable<DropDownItem<ValueType>>
    {
        public readonly ValueType value;
        public readonly OptionData optionData;

        public DropDownItem(ValueType value, OptionData optionData)
        {
            this.value = value;
            this.optionData = optionData;
        }
        public int CompareTo(DropDownItem<ValueType> other)
        {
            return Comparer<ValueType>.Default.Compare(value, other.value);
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

    public class StringDropDown : SimpleDropDown<string, StringDropDown.StringEntity, StringDropDown.StringPopup, StringDropDown.StringDropDownRef>
    {
        protected override StringDropDownRef CreateRef() => new(this);

        public class StringEntity : SimpleEntity<string> { }
        public class StringPopup : SimplePopup<string, StringEntity> { }
        public class StringDropDownRef : SimpleDropDownRef<string, StringDropDown>
        {
            public StringDropDownRef(StringDropDown dropDown) : base(dropDown) { }
        }
    }
    public abstract class EnumDropDown<EnumType, EntityType, PopupType, RefType> : SimpleDropDown<EnumType, EntityType, PopupType, RefType>, IEnumSelector<EnumType>
        where EnumType : Enum
        where EntityType : SimpleEntity<EnumType>
        where PopupType : SimplePopup<EnumType, EntityType>
        where RefType : IDropDownRef, IDropDown<EnumType>
    {
        public void Init(Func<EnumType, bool> selector = null)
        {
            PauseLayout(() =>
            {
                foreach (var value in GetValues())
                {
                    if (selector == null || selector(value))
                    {
                        var label = value.Description();
                        var atlas = value.Atlas();
                        var sprite = value.Sprite();
                        AddItem(value, new OptionData(label, atlas, sprite));
                    }
                }
            });
        }
        protected virtual IEnumerable<EnumType> GetValues() => EnumExtension.GetEnumValues<EnumType>().IsVisible();
    }
}
