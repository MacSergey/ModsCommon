using System;
using System.Collections.Generic;
using UnityEngine;

namespace ModsCommon.UI
{
    public interface ISelectorRef { }
    public interface ISelector<ValueType, RefType> : IAutoLayoutPanel, IReusable
        where RefType : ISelectorRef
    {
        RefType Ref { get; }
        Func<ValueType, ValueType, bool> IsEqualDelegate { set; }

        void AddItem(ValueType item, OptionData optionData);
        void Clear();
        void SetDefaultStyle(Vector2? size = null);
    }
    public interface ISingleSelector<ValueType, RefType> : ISelector<ValueType, RefType>
        where RefType : ISelectorRef
    {
        event Action<ValueType> OnSelectObject;

        ValueType SelectedObject { get; set; }
        bool UseWheel { get; set; }
        bool WheelTip { set; }
    }
    public interface IMultiSelector<ValueType, RefType> : ISelector<ValueType, RefType>
        where RefType : ISelectorRef
    {
        event Action<List<ValueType>> OnSelectedObjectsChanged;

        List<ValueType> SelectedObjects { get; set; }
    }
}
