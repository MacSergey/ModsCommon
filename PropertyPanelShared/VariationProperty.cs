using ColossalFramework.UI;
using UnityEngine;

namespace ModsCommon.UI
{
    public abstract class VariationProperty<ItemType, SegmentedType> : EditorPropertyPanel, IReusable
        where SegmentedType : UIOnceSegmented<ItemType>
    {
        bool IReusable.InCache { get; set; }
        Transform IReusable.CachedTransform { get => m_CachedTransform; set => m_CachedTransform = value; }

        private SegmentedType Selector { get; set; }
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
        }

        protected abstract void AddSelectorItems();
        protected void AddItem(ItemType item, OptionData data) => Selector.AddItem(item, data);

        private void SelectorChanged(ItemType selctedItem)
        {
            Refresh();
            SelectorChangedImpl(selctedItem);
        }
        protected abstract void SelectorChangedImpl(ItemType selctedItem);
        protected abstract void Refresh();

        public override void SetStyle(ControlStyle style)
        {
            Selector.SegmentedStyle = style.Segmented;
        }
    }
}
