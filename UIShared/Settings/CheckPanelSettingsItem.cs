using ModsCommon.Utilities;
using System;
using System.Collections.Generic;
using System.Text;
using ColossalFramework.UI;
using UnityEngine;
using ICities;

namespace ModsCommon.UI
{
    public class CheckPanelSettingsItem : BorderSettingsItem
    {
        public Action<int> OnValueChanged;

        private List<CustomUICheckBox> CheckBoxes { get; } = new List<CustomUICheckBox>();

        private int value;
        public int Value
        {
            get => value;
            set => OnStateChanged(value);
        }

        public CheckPanelSettingsItem() : base()
        {
            AutoLayout = AutoLayout.Vertical;
            autoChildrenVertically = AutoLayoutChildren.Fit;
            autoChildrenHorizontally = AutoLayoutChildren.Fill;
            autoLayoutSpace = 5;
            Padding = new RectOffset(45, 20, 7, 7);
        }

        public void Init(string[] labels)
        {
            PauseLayout(() =>
            {
                if (labels.Length > CheckBoxes.Count)
                {
                    var count = labels.Length - CheckBoxes.Count;
                    for (int i = 0; i < count; i += 1)
                    {
                        var checkBox = AddUIComponent<CustomUICheckBox>();
                        checkBox.SettingsStyle();
                        checkBox.AutoHeight = true;
                        checkBox.WordWrap = true;
                        checkBox.textScale = 0.9f;
                        var index = CheckBoxes.Count;
                        checkBox.OnStateChanged += (state) => OnStateChanged(index);
                        CheckBoxes.Add(checkBox);
                    }
                }
                else if (CheckBoxes.Count > labels.Length)
                {
                    for (int i = labels.Length; i < CheckBoxes.Count; i += 1)
                    {
                        ComponentPool.Free(CheckBoxes[i]);
                    }
                    CheckBoxes.RemoveRange(labels.Length, CheckBoxes.Count - labels.Length);
                }

                for (int i = 0; i < labels.Length; i += 1)
                {
                    CheckBoxes[i].Label = labels[i];
                }

                if (CheckBoxes.Count > 0)
                    OnStateChanged(0);
            });
        }

        private bool inProcess;
        public void OnStateChanged(int index)
        {
            if (!inProcess)
            {
                inProcess = true;

                value = index;
                OnValueChanged?.Invoke(value);

                for (var i = 0; i < CheckBoxes.Count; i += 1)
                    CheckBoxes[i].IsChecked = value == i;

                inProcess = false;
            }
        }

        public void SetLabel(int index, string label) => CheckBoxes[index].Label = label;
    }
}
