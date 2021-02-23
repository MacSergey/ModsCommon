using ColossalFramework.UI;
using ModsCommon.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace ModsCommon.UI
{
    public abstract class EnumPropertyPanel<EnumType, UISelector> : ListPropertyPanel<EnumType, UISelector>
        where EnumType : Enum
        where UISelector : UIComponent, IUISelector<EnumType>
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

    public class BoolListPropertyPanel : ListPropertyPanel<bool, BoolListPropertyPanel.BoolSegmented>
    {
        protected override bool AllowNull => false;
        protected override bool IsEqual(bool first, bool second) => first == second;
        public void Init(string falseLabel, string trueLabel)
        {
            base.Init();
            Selector.AddItem(true, trueLabel);
            Selector.AddItem(false, falseLabel);
        }

        public class BoolSegmented : UISegmented<bool> { }
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

        public class IntSegmented : UISegmented<int> { }
    }
}
