using ColossalFramework.UI;
using System;
using UnityEngine;

namespace ModsCommon.UI
{
    public abstract class VariationProperty<ItemType, SegmentedType> : EditorPropertyPanel, IReusable
        where SegmentedType : UIOnceSegmented<ItemType>
    {
        bool IReusable.InCache { get; set; }
        Transform IReusable.CachedTransform { get => m_CachedTransform; set => m_CachedTransform = value; }

        public event Action<ItemType> OnSelectorChanged;

        protected SegmentedType Selector { get; private set; }
        protected ItemType SelectedObject
        {
            get => Selector.SelectedObject;
            set => Selector.SelectedObject = value;
        }

        protected override void FillContent()
        {
            Selector = Content.AddUIComponent<SegmentedType>();
            Selector.name = nameof(Selector);
            Selector.SetDefaultStyle();
        }
        public override void Init()
        {
            Selector.AutoButtonSize = false;
            Selector.ButtonWidth = 30f;
            Selector.SetDefaultStyle();
            Selector.PauseLayout(AddSelectorItems);
            Selector.OnSelectObject += SelectorChanged;

            base.Init();
        }

        public override void DeInit()
        {
            base.DeInit();

            Selector.DeInit();
            OnSelectorChanged = null;
        }

        protected abstract void AddSelectorItems();
        protected void AddItem(ItemType item, OptionData data) => Selector.AddItem(item, data);

        private void SelectorChanged(ItemType selctedItem)
        {
            Refresh();
            SelectorChangedImpl(selctedItem);
        }
        protected virtual void SelectorChangedImpl(ItemType selctedItem) => OnSelectorChanged?.Invoke(selctedItem);
        protected abstract void Refresh();

        public override void SetStyle(ControlStyle style)
        {
            Selector.SegmentedStyle = style.Segmented;
        }
    }

    public abstract class TwoVariationProperty<ItemType, SegmentedType1, SegmentedType2> : EditorPropertyPanel, IReusable
        where SegmentedType1 : UIOnceSegmented<ItemType>
        where SegmentedType2 : UIOnceSegmented<ItemType>
    {
        bool IReusable.InCache { get; set; }
        Transform IReusable.CachedTransform { get => m_CachedTransform; set => m_CachedTransform = value; }

        public event Action<ItemType> OnSelector1Changed;
        public event Action<ItemType> OnSelector2Changed;

        protected SegmentedType1 Selector1 { get; private set; }
        protected SegmentedType2 Selector2 { get; private set; }

        protected ItemType SelectedObject1
        {
            get => Selector1.SelectedObject;
            set => Selector1.SelectedObject = value;
        }
        protected ItemType SelectedObject2
        {
            get => Selector2.SelectedObject;
            set => Selector2.SelectedObject = value;
        }

        protected override void FillContent()
        {
            Selector1 = Content.AddUIComponent<SegmentedType1>();
            Selector1.name = nameof(Selector1);
            Selector1.SetDefaultStyle();

            Selector2 = Content.AddUIComponent<SegmentedType2>();
            Selector2.name = nameof(Selector2);
            Selector2.SetDefaultStyle();
        }
        public override void Init()
        {
            Selector1.AutoButtonSize = false;
            Selector1.ButtonWidth = 30f;
            Selector1.SetDefaultStyle();
            Selector1.PauseLayout(AddSelector1Items);
            Selector1.OnSelectObject += Selector1Changed;

            Selector2.AutoButtonSize = false;
            Selector2.ButtonWidth = 30f;
            Selector2.SetDefaultStyle();
            Selector2.PauseLayout(AddSelector2Items);
            Selector2.OnSelectObject += Selector2Changed;

            base.Init();
        }

        public override void DeInit()
        {
            base.DeInit();

            Selector1.DeInit();
            Selector2.DeInit();

            OnSelector1Changed = null;
            OnSelector2Changed = null;
        }

        protected abstract void AddSelector1Items();
        protected abstract void AddSelector2Items();
        protected void AddItem1(ItemType item, OptionData data) => Selector1.AddItem(item, data);
        protected void AddItem2(ItemType item, OptionData data) => Selector2.AddItem(item, data);

        private void Selector1Changed(ItemType selctedItem)
        {
            Refresh();
            Selector1ChangedImpl(selctedItem);
        }
        private void Selector2Changed(ItemType selctedItem)
        {
            Refresh();
            Selector2ChangedImpl(selctedItem);
        }
        protected virtual void Selector1ChangedImpl(ItemType selctedItem) => OnSelector1Changed?.Invoke(selctedItem);
        protected virtual void Selector2ChangedImpl(ItemType selctedItem) => OnSelector2Changed?.Invoke(selctedItem);
        protected abstract void Refresh();

        public override void SetStyle(ControlStyle style)
        {
            Selector1.SegmentedStyle = style.Segmented;
            Selector2.SegmentedStyle = style.Segmented;
        }
    }
}
