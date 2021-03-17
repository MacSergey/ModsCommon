using ColossalFramework.UI;
using ModsCommon.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace ModsCommon.UI
{
    public abstract class EnumPropertyPanel<EnumType, UISelector> : ListOncePropertyPanel<EnumType, UISelector>
        where EnumType : Enum
        where UISelector : UIComponent, IUIOnceSelector<EnumType>
    {
        protected override bool AllowNull => false;
        public override void Init()
        {
            base.Init();
            FillItems();
        }
        protected virtual void FillItems()
        {
            foreach (var value in EnumExtension.GetEnumValues<EnumType>())
                Selector.AddItem(value, GetDescription(value));
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
        public override void Init()
        {
            base.Init();
            FillItems();
        }
        protected virtual void FillItems()
        {
            foreach (var value in EnumExtension.GetEnumValues<EnumType>())
                Selector.AddItem(value, GetDescription(value));
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
        public void Init(string falseLabel, string trueLabel, bool invert = true)
        {
            base.Init();

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
        }

        public class BoolSegmented : UIOnceSegmented<bool> { }
    }
    public class IntListPropertyPanel : ListPropertyPanel<int, IntListPropertyPanel.IntSegmented>
    {
        protected override bool AllowNull => false;
        protected override bool IsEqual(int first, int second) => first == second;
        public void Init(int count)
        {
            base.Init();
            for (var i = 1; i <= count; i += 1)
                Selector.AddItem(i, i.ToString());
        }

        public class IntSegmented : UIOnceSegmented<int> { }
    }
}
