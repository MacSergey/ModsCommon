using ColossalFramework.UI;
using ModsCommon.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ModsCommon.UI
{
    public abstract class EnumOncePropertyPanel<EnumType, UISelector> : ListOncePropertyPanel<EnumType, UISelector>
        where EnumType : Enum
        where UISelector : UIComponent, IUIOnceSelector<EnumType>
    {
        protected override bool AllowNull => false;

        public override void Init() => Init(null);
        public void Init(Func<EnumType, bool> selector)
        {
            base.Init(null);
            FillItems(selector);
        }
        protected virtual void FillItems(Func<EnumType, bool> selector)
        {
            Selector.StopLayout();
            foreach (var value in EnumExtension.GetEnumValues<EnumType>())
            {
                if (selector?.Invoke(value) != false)
                    Selector.AddItem(value, GetDescription(value));
            }
            Selector.StartLayout();
        }
        public virtual void Clear()
        {
            Selector.StopLayout();
            Selector.Clear();
            Selector.StartLayout();
        }
        protected abstract string GetDescription(EnumType value);
    }
    public abstract class EnumMultyPropertyPanel<EnumType, UISelector> : ListMultyPropertyPanel<EnumType, UISelector>
        where EnumType : Enum
        where UISelector : UIComponent, IUIMultySelector<EnumType>
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
            Selector.StopLayout();
            foreach (var value in EnumExtension.GetEnumValues<EnumType>())
            {
                if (selector?.Invoke(value) != false)
                    Selector.AddItem(value, GetDescription(value));
            }
            Selector.StartLayout();
        }
        public virtual void Clear()
        {
            Selector.StopLayout();
            Selector.Clear();
            Selector.StartLayout();
        }
        protected abstract string GetDescription(EnumType value);

        protected override void SelectorValueChanged(List<EnumType> value)
        {
            base.SelectorValueChanged(value);
            OnSelectObjectChanged?.Invoke(value.GetEnum());
        }
    }

    public class BoolListPropertyPanel : ListOncePropertyPanel<bool, BoolListPropertyPanel.BoolSegmented>
    {
        protected override bool AllowNull => false;
        protected override bool IsEqual(bool first, bool second) => first == second;

        public override void Init() => Init(CommonLocalize.MessageBox_No, CommonLocalize.MessageBox_Yes);
        public void Init(string falseLabel, string trueLabel, bool invert = true)
        {
            base.Init(null);

            Selector.StopLayout();
            if (invert)
            {
                Selector.AddItem(true, trueLabel);
                Selector.AddItem(false, falseLabel);
            }
            else
            {
                Selector.AddItem(false, falseLabel);
                Selector.AddItem(true, trueLabel);
            }
            Selector.StartLayout();
        }

        public class BoolSegmented : UIOnceSegmented<bool> { }
    }
    public class IntListPropertyPanel : ListPropertyPanel<int, IntListPropertyPanel.IntSegmented>
    {
        protected override bool AllowNull => false;
        protected override bool IsEqual(int first, int second) => first == second;

        public override void Init() => Init(2);
        public void Init(int count)
        {
            base.Init(null);

            Selector.StopLayout();
            for (var i = 1; i <= count; i += 1)
                Selector.AddItem(i, i.ToString());
            Selector.StartLayout();
        }

        public class IntSegmented : UIOnceSegmented<int> { }
    }
}
