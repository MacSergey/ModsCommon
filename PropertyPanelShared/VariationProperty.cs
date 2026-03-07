using System;
using UnityEngine;

namespace ModsCommon.UI
{
    public abstract class VariationProperty<ItemType, SegmentedType, RefType> : EditorPropertyPanel, IReusable
        where SegmentedType : UISingleSegmented<ItemType>, RefType
        where RefType : ISingleSegmented<ItemType>
    {
        bool IReusable.InCache { get; set; }
        Transform IReusable.CachedTransform { get => m_CachedTransform; set => m_CachedTransform = value; }

        public event Action<ItemType> OnSelectorChanged;

        protected SegmentedType Selector { get; private set; }
        public RefType SelectorRef => Selector;

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

        protected virtual void SelectorChanged(ItemType selectedItem)
        {
            OnSelectorChanged?.Invoke(selectedItem);
        }

        public override void SetStyle(ControlStyle style)
        {
            Selector.SegmentedStyle = style.Segmented;
        }
    }
}
