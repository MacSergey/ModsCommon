using ColossalFramework.UI;
using System;
using System.Collections.Generic;
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
        public ObjectType SelectedObject
        {
            get => selectedIndex >= 0 ? ObjectList[selectedIndex] : default;
            set
            {
                selectedIndex = ObjectList.FindIndex(o => IsEqualDelegate?.Invoke(o, value) ?? ReferenceEquals(o, value) || (o != null && o.Equals(value)));
                Entity.SetObject(-1, SelectedObject, false);
            }
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
            ObjectList.Add(item);
        }
        public virtual void Clear()
        {
            ObjectList.Clear();
            selectedIndex = -1;
        }

        protected override void InitPopup()
        {
            base.InitPopup();
            Popup.SelectedObject = SelectedObject;
        }
        protected override void SelectObject(ObjectType value)
        {
            SelectedObject = value;
            base.SelectObject(value);
        }
        protected override void OnSizeChanged()
        {
            base.OnSizeChanged();
            Entity.size = size;
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
