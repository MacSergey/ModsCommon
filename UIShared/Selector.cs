using System;
using System.Collections.Generic;
using UnityEngine;

namespace ModsCommon.UI
{
    public interface ISelector { }
    public interface ISelector<ValueType> : ISelector, IAutoLayoutPanel, IReusable
    {
        Func<ValueType, ValueType, bool> IsEqualDelegate { set; }

        void AddItem(ValueType item, OptionData optionData);
        void Clear();
        void SetDefaultStyle(Vector2? size = null);
    }
    public interface ISingleSelector<ValueType> : ISelector<ValueType>
    {
        event Action<ValueType> OnSelectObject;

        ValueType SelectedObject { get; set; }
        bool UseWheel { get; set; }
        bool WheelTip { set; }
    }
    public interface IMultiSelector<ValueType> : ISelector<ValueType>
    {
        event Action<List<ValueType>> OnSelectedObjectsChanged;

        List<ValueType> SelectedObjects { get; set; }
    }
}
