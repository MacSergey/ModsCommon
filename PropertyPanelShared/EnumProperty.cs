﻿using ColossalFramework.UI;
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
        protected virtual IEnumerable<EnumType> GetValues() => EnumExtension.GetEnumValues<EnumType>().IsVisible();
        protected virtual void FillItems(Func<EnumType, bool> selector)
        {
            Selector.PauseLayout(() =>
            {
                foreach (var value in GetValues())
                {
                    if (selector?.Invoke(value) != false)
                        Selector.AddItem(value, new OptionData(GetDescription(value)));
                }
            });
        }
        public virtual void Clear()
        {
            Selector.PauseLayout(Selector.Clear);
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
            Selector.PauseLayout(() =>
            {
                foreach (var value in EnumExtension.GetEnumValues<EnumType>().IsVisible())
                {
                    if (selector?.Invoke(value) != false)
                        Selector.AddItem(value, new OptionData(GetDescription(value)));
                }
            });
        }
        public virtual void Clear()
        {
            Selector.PauseLayout(Selector.Clear);
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

        public class IntSegmented : UIOnceSegmented<int> { }
    }
}
