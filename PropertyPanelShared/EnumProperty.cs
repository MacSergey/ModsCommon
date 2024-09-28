using ColossalFramework.UI;
using IMT.Utilities;
using ModsCommon.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ModsCommon.UI
{

    public abstract class EnumSinglePropertyPanel<EnumType, SelectorType, RefType> : ListSinglePropertyPanel<EnumType, SelectorType, RefType>
        where EnumType : Enum
        where SelectorType : UIComponent, ISingleSelector<EnumType, RefType>
        where RefType : ISelectorRef
    {
        protected override bool AllowNull => false;

        public override void Init() => Init(null);
        public void Init(Func<EnumType, bool> selector)
        {
            base.Init(null);
            FillItems(selector);
        }
        protected virtual IEnumerable<EnumType> GetValues() => EnumExtension.GetEnumValues<EnumType>().IsVisible();
        protected virtual void FillItems(Func<EnumType, bool> selector)
        {
            Selector.PauseLayout(() =>
            {
                foreach (var value in GetValues())
                {
                    if (selector == null || selector(value))
                    {
                        var label = value.Description();
                        var atlas = value.Atlas();
                        var sprite = value.Sprite();
                        Selector.AddItem(value, new OptionData(label, atlas, sprite));
                    }
                }
            });
        }
        public virtual void Clear()
        {
            Selector.PauseLayout(Selector.Clear);
        }
    }
    public abstract class AutoEnumSinglePropertyPanel<EnumType, SegmentedType, RefType> : ListSinglePropertyPanel<EnumType, SegmentedType, RefType>
        where EnumType : Enum
        where SegmentedType : UISingleEnumSegmented<EnumType, RefType>, ISingleSelector<EnumType, RefType>
        where RefType : ISegmentedRef, ISegmented<EnumType>
    {
        protected override bool AllowNull => false;
        public RefType SelectorRef => Selector.Ref;

        protected override void AddSelector()
        {
            base.AddSelector();
            Selector.Init();
        }
        protected override void ClearSelector()
        {
            //dont clear
        }

        public override void SetStyle(ControlStyle style)
        {
            Selector.SegmentedStyle = style.Segmented;
        }
    }

    public abstract class EnumMultiPropertyPanel<EnumType, SelectorType, RefType> : ListMultiPropertyPanel<EnumType, SelectorType, RefType>
        where EnumType : Enum
        where SelectorType : UIComponent, IMultiSelector<EnumType, RefType>
        where RefType : ISelectorRef
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
            FillItems(selector);
        }
        protected virtual void FillItems(Func<EnumType, bool> selector)
        {
            Selector.PauseLayout(() =>
            {
                foreach (var value in EnumExtension.GetEnumValues<EnumType>().IsVisible())
                {
                    if (selector == null || selector(value))
                    {
                        var label = value.Description();
                        var atlas = value.Atlas();
                        var sprite = value.Sprite();
                        Selector.AddItem(value, new OptionData(label, atlas, sprite));
                    }
                }
            });
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
    public abstract class AutoEnumMultiPropertyPanel<EnumType, SegmentedType, RefType> : ListMultiPropertyPanel<EnumType, SegmentedType, RefType>
        where EnumType : Enum
        where SegmentedType : UIMultyEnumSegmented<EnumType, RefType>, IMultiSelector<EnumType, RefType>
        where RefType : ISegmentedRef, ISegmented<EnumType>
    {
        protected override bool AllowNull => false;
        public RefType SelectorRef => Selector.Ref;

        protected override void AddSelector()
        {
            base.AddSelector();
            Selector.Init();
        }
        protected override void ClearSelector()
        {
            //dont clear
        }

        public override void SetStyle(ControlStyle style)
        {
            Selector.SegmentedStyle = style.Segmented;
        }
    }

    public class BoolListPropertyPanel : ListSinglePropertyPanel<bool, BoolSegmented, BoolSegmented.BoolSegmentedRef>
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
    public class IntListPropertyPanel : ListPropertyPanel<int, IntSegmented, IntSegmented.IntSegmentedRef>
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
