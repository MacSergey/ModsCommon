using ColossalFramework.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ModsCommon.UI
{
    public abstract class SelectItemDropDown<ObjectType, EntityType, PopupType> : ObjectDropDown<ObjectType, EntityType, PopupType>
        where PopupType : CustomUIPanel, IPopup<ObjectType, EntityType>
        where EntityType : CustomUIButton, IPopupEntity<ObjectType>
    {
        public EntityType Entity { get; private set; }

        protected override IEnumerable<ObjectType> Objects => ObjectList;
        protected List<ObjectType> ObjectList { get; } = new List<ObjectType>();

        private int selectedIndex = -1;
        public int SelectedIndex
        {
            get => selectedIndex;
            set
            {
                if(value != selectedIndex)
                {
                    selectedIndex = (value >= 0 && value < ObjectList.Count) ? value : -1;
                    Entity.SetObject(-1, SelectedObject, false);
                    SelectObjectEvent(SelectedObject);
                }
            }
        }
        public ObjectType SelectedObject
        {
            get => SelectedIndex >= 0 && SelectedIndex < ObjectList.Count ? ObjectList[SelectedIndex] : default;
            set => SelectedIndex = ObjectList.FindIndex(o => IsEqual(o, value));
        }

        private IComparer<ObjectType> comparer;
        public Func<ObjectType, bool> ItemSelector { get; set; }
        public IComparer<ObjectType> ItemComparer 
        {
            get => comparer;
            set
            {
                comparer = value;
                if(comparer != null)
                    ObjectList.Sort(comparer);
            }
        }
        protected override Func<ObjectType, bool> Selector => ItemSelector;
        protected override IComparer<ObjectType> Comparer => ItemComparer;

        public bool CanWheel { get; private set; }
        public bool UseWheel { get; set; }
        public bool WheelTip
        {
            set => tooltip = value ? CommonLocalize.ListPanel_ScrollWheel : string.Empty;
        }

        public SelectItemDropDown()
        {
            Entity = AddUIComponent<EntityType>();
            Entity.relativePosition = Vector3.zero;

            Entity.isInteractive = false;
            foreach (var item in Entity.GetComponentsInChildren<UIComponent>())
                item.isInteractive = false;
        }

        public virtual void AddItem(ObjectType item)
        {
            if(Comparer != null)
            {
                var index = ObjectList.BinarySearch(item, Comparer);
                if(index < 0)
                    ObjectList.Insert(~index, item);
            }
            else
                ObjectList.Add(item);
        }
        public virtual void Clear()
        {
            ObjectList.Clear();
            selectedIndex = -1;
            Entity.SetObject(-1, default, false);
        }

        protected override void InitPopup()
        {
            base.InitPopup();
            Popup.SelectedObject = SelectedObject;
        }
        public virtual void DeInit()
        {
            UseWheel = false;
            WheelTip = false;
        }

        protected override void SelectObject(ObjectType value) => SelectedObject = value;
        protected override void OnSizeChanged()
        {
            base.OnSizeChanged();
            Entity.size = size;
        }

        protected override void OnMouseMove(UIMouseEventParameter p)
        {
            base.OnMouseMove(p);
            CanWheel = true;
        }
        protected override void OnMouseLeave(UIMouseEventParameter p)
        {
            base.OnMouseLeave(p);
            CanWheel = false;
        }
        protected sealed override void OnMouseWheel(UIMouseEventParameter p)
        {
            m_TooltipShowing = true;
            tooltipBox.Hide();

            if (UseWheel && (CanWheel || Time.realtimeSinceStartup - m_HoveringStartTime >= UIHelper.PropertyScrollTimeout))
            {
                if (p.wheelDelta > 0)
                {
                    for (var index = SelectedIndex - 1; index >= 0; index -= 1)
                    {
                        if (Selector == null || Selector(ObjectList[index]))
                        {
                            SelectedIndex = index;
                            break;
                        }
                    }
                }
                else if (p.wheelDelta < 0)
                {             
                    for (var index = SelectedIndex + 1; index < ObjectList.Count; index += 1)
                    {
                        if (Selector == null || Selector(ObjectList[index]))
                        {
                            SelectedIndex = index;
                            break;
                        }                   
                    }
                }

                p.Use();
            }
        }

        public override DropDownStyle DropDownStyle 
        { 
            get => base.DropDownStyle;
            set
            {
                Entity.TextColors = value.TextColors;
                Entity.SelTextColors = value.SelTextColors;
                base.DropDownStyle = value;
            }
        }
    }
}
