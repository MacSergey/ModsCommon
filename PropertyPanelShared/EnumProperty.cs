using ColossalFramework.UI;
using IMT.Utilities;
using ModsCommon.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ModsCommon.UI
{
    public interface IEnumSelector<EnumType>
    {
        void Init(Func<EnumType, bool> selector = null);
    }

    public abstract class EnumSinglePropertyPanel<EnumType, SelectorType, RefType> : ListSinglePropertyPanel<EnumType, SelectorType, RefType>
        where EnumType : Enum
        where SelectorType : UIComponent, RefType, IEnumSelector<EnumType>
        where RefType : ISingleSelector<EnumType>
    {
        protected override bool AllowNull => false;

        public override void Init() => Init(null);
        public virtual void Init(Func<EnumType, bool> selector)
        {
            base.Init(null);
            InitSelector(selector);
        }
        protected virtual void InitSelector(Func<EnumType, bool> selector)
        {
            Selector.Init(selector);
        }
        public virtual void Clear()
        {
            Selector.PauseLayout(Selector.Clear);
        }
    }
    public abstract class EnumSingleSegmentedPropertyPanel<EnumType, SegmentedType, RefType> : EnumSinglePropertyPanel<EnumType, SegmentedType, RefType>
        where EnumType : Enum
        where SegmentedType : UIComponent, RefType, IEnumSelector<EnumType>
        where RefType : ISingleSegmented<EnumType>
    {
        public override void SetStyle(ControlStyle style)
        {
            Selector.SegmentedStyle = style.Segmented;
        }
    }

    public abstract class EnumSingleDropDownPropertyPanel<EnumType, DropDownType, RefType> : EnumSinglePropertyPanel<EnumType, DropDownType, RefType>
        where EnumType : Enum
        where DropDownType : UIComponent, RefType, IEnumSelector<EnumType>
        where RefType : ISingleDropDown<EnumType>
    {
        public override void SetStyle(ControlStyle style)
        {
            Selector.DropDownStyle = style.DropDown;
        }
    }


    public abstract class EnumMultiPropertyPanel<EnumType, SelectorType, RefType> : ListMultiPropertyPanel<EnumType, SelectorType, RefType>
        where EnumType : Enum
        where SelectorType : UIComponent, RefType
        where RefType : IMultiSelector<EnumType>, IEnumSelector<EnumType>
    {
        public event Action<EnumType> OnSelectObjectChanged;

        protected override bool AllowNull => false;
        public EnumType SelectedObject
        {
            get => Selector.SelectedObjects.GetEnum();
            set => Selector.SelectedObjects = value.GetEnumValues().ToList();
        }

        public override void Init() => Init(null);
        public void Init(Func<EnumType, bool> selector)
        {
            base.Init(null);
            Selector.Init(selector);
        }
        public virtual void Clear()
        {
            Selector.PauseLayout(Selector.Clear);
        }

        protected override void SelectorValueChanged(List<EnumType> value)
        {
            base.SelectorValueChanged(value);
            OnSelectObjectChanged?.Invoke(value.GetEnum());
        }
    }

    public class BoolListPropertyPanel : ListSinglePropertyPanel<bool, BoolSegmented, ISingleSelector<bool>>
    {
        protected override bool AllowNull => false;
        protected override bool IsEqual(bool first, bool second) => first == second;

        public override void Init() => Init(CommonLocalize.MessageBox_No, CommonLocalize.MessageBox_Yes);
        public void Init(string falseLabel, string trueLabel, bool invert = true)
        {
            base.Init(null);

            Selector.PauseLayout(() =>
            {
                if (invert)
                {
                    Selector.AddItem(true, new OptionData(trueLabel));
                    Selector.AddItem(false, new OptionData(falseLabel));
                }
                else
                {
                    Selector.AddItem(false, new OptionData(falseLabel));
                    Selector.AddItem(true, new OptionData(trueLabel));
                }
            });
        }
        public override void SetStyle(ControlStyle style)
        {
            Selector.SegmentedStyle = style.Segmented;
        }
    }
    public class IntListPropertyPanel : ListPropertyPanel<int, IntSegmented, ISingleSelector<int>>
    {
        protected override bool AllowNull => false;
        protected override bool IsEqual(int first, int second) => first == second;

        public override void Init() => Init(2);
        public void Init(int count)
        {
            base.Init(null);

            Selector.PauseLayout(() =>
            {
                for (var i = 1; i <= count; i += 1)
                    Selector.AddItem(i, new OptionData(i.ToString()));
            });
        }
        public override void SetStyle(ControlStyle style)
        {
            Selector.SegmentedStyle = style.Segmented;
        }
    }
}
