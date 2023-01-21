using System;
using System.Collections.Generic;
using UnityEngine;

namespace ModsCommon.UI
{
    public interface IUISelector<ValueType> : IAutoLayoutPanel, IReusable
    {
        Func<ValueType, ValueType, bool> IsEqualDelegate { get; set; }

        void AddItem(ValueType item, string label = null);
        void Clear();
        void SetDefaultStyle(Vector2? size = null);
    }
    public interface IUIOnceSelector<ValueType> : IUISelector<ValueType>
    {
        event Action<ValueType> OnSelectObjectChanged;

        ValueType SelectedObject { get; set; }
        bool UseWheel { get; set; }
        bool WheelTip { set; }
    }
    public interface IUIMultySelector<ValueType> : IUISelector<ValueType>
    {
        event Action<List<ValueType>> OnSelectObjectsChanged;

        List<ValueType> SelectedObjects { get; set; }
    }
}
