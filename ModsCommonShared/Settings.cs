using ColossalFramework;
using ColossalFramework.UI;
using ICities;
using ModsCommon.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace ModsCommon
{
    public abstract class BaseSettings<TypeMod>
        where TypeMod : BaseMod<TypeMod>
    {
        private UIPanel MainPanel { get; set; }
        protected TabStrip TabStrip { get; set; }
        protected List<CustomUIPanel> TabPanels { get; set; }
        protected virtual string GeneralTabName => "General";
        protected UIHelper GeneralTab { get; private set; }

        public void OnSettingsUI(UIHelperBase helper)
        {
            var scrollable = (helper as UIHelper).self as UIScrollablePanel;
            MainPanel = scrollable.parent as UIPanel;

            foreach (var components in MainPanel.components)
                components.isVisible = false;

            MainPanel.autoLayout = true;
            MainPanel.autoLayoutDirection = LayoutDirection.Vertical;
            MainPanel.autoLayoutPadding = new RectOffset(0, 0, 0, 15);
            CreateTabStrip();

            GeneralTab = CreateTab(GeneralTabName);
            GeneralTab.AddGroup(SingletonMod<TypeMod>.Instance.Name);

            OnSettingsUI();

            TabStrip.SelectedTab = 0;
            TabStrip.isVisible = TabPanels.Count > 1;
        }
        protected virtual void OnSettingsUI() { }

        protected void CreateTabStrip()
        {
            TabPanels = new List<CustomUIPanel>();

            TabStrip = MainPanel.AddUIComponent<TabStrip>();
            TabStrip.SelectedTabChanged += OnSelectedTabChanged;
            TabStrip.SelectedTab = -1;
            TabStrip.width = MainPanel.width - MainPanel.autoLayoutPadding.horizontal;
            TabStrip.eventSizeChanged += (_, _) => TabStripSizeChanged();
        }
        private void OnSelectedTabChanged(int index)
        {
            if (index >= 0 && TabPanels.Count > index)
            {
                foreach (var tab in TabPanels)
                    tab.isVisible = false;

                TabPanels[index].isVisible = true;
            }
        }
        private void TabStripSizeChanged()
        {
            foreach (var tab in TabPanels)
                SetTabSize(tab);
        }
        private void SetTabSize(UIPanel panel)
        {
            panel.size = new Vector2(MainPanel.width, MainPanel.height - (TabStrip.isVisible ? MainPanel.autoLayoutPadding.vertical + TabStrip.height : 0f));
        }

        protected UIHelper CreateTab(string name)
        {
            TabStrip.AddTab(name, 1.25f);

            var tabPanel = MainPanel.AddUIComponent<AdvancedScrollablePanel>();
            tabPanel.Content.autoLayoutPadding = new RectOffset(8, 8, 0, 0);
            SetTabSize(tabPanel);
            tabPanel.isVisible = false;
            TabPanels.Add(tabPanel);

            return new UIHelper(tabPanel.Content);
        }

        #region UTILITY

        protected void AddFloatField(UIHelper group, string label, SavedFloat saved, float? defaultValue, float? min = null, float? max = null, Action onSubmit = null)
        {
            UITextField field = null;
            field = group.AddTextfield(label, saved.ToString(), OnChanged, OnSubmitted) as UITextField;

            static void OnChanged(string distance) { }
            void OnSubmitted(string text)
            {
                if (float.TryParse(text, out float value))
                {
                    if ((min.HasValue && value < min.Value) || (max.HasValue && value > max.Value))
                        value = defaultValue ?? 0;

                    saved.value = value;
                    field.text = value.ToString();
                }
                else
                    field.text = saved.ToString();

                onSubmit?.Invoke();
            }
        }

        protected void AddCheckBox(UIHelper group, string label, SavedBool saved, Action onChanged = null)
        {
            group.AddCheckbox(label, saved, OnValueChanged);

            void OnValueChanged(bool value)
            {
                saved.value = value;
                onChanged?.Invoke();
            }
        }

        protected void AddCheckboxPanel(UIHelper group, string mainLabel, SavedBool mainSaved, SavedInt optionsSaved, string[] labels, Action onChanged = null)
        {
            var inProcess = false;
            var checkBoxes = new UICheckBox[labels.Length];
            var optionsPanel = default(CustomUIPanel);

            var mainCheckBox = group.AddCheckbox(mainLabel, mainSaved, OnMainChanged) as UICheckBox;

            optionsPanel = (group.self as UIComponent).AddUIComponent<CustomUIPanel>();
            optionsPanel.autoLayout = true;
            optionsPanel.autoLayoutDirection = LayoutDirection.Vertical;
            optionsPanel.autoFitChildrenHorizontally = true;
            optionsPanel.autoFitChildrenVertically = true;
            optionsPanel.autoLayoutPadding = new RectOffset(25, 0, 0, 5);
            var panelHelper = new UIHelper(optionsPanel);

            for (var i = 0; i < checkBoxes.Length; i += 1)
            {
                var index = i;
                checkBoxes[i] = panelHelper.AddCheckbox(labels[i], optionsSaved == i, (value) => Set(index, value)) as UICheckBox;
            }

            SetVisible();

            void OnMainChanged(bool value)
            {
                mainSaved.value = value;
                onChanged?.Invoke();
                SetVisible();
            }
            void SetVisible() => optionsPanel.isVisible = mainSaved;
            void Set(int index, bool value)
            {
                if (!inProcess)
                {
                    inProcess = true;
                    optionsSaved.value = index;
                    onChanged?.Invoke();
                    for (var i = 0; i < checkBoxes.Length; i += 1)
                        checkBoxes[i].isChecked = optionsSaved == i;
                    inProcess = false;
                }
            }
        }
        protected UIButton AddButton(UIHelper group, string text, OnButtonClicked click, float width = 400)
        {
            var button = group.AddButton(text, click) as UIButton;
            button.autoSize = false;
            button.textHorizontalAlignment = UIHorizontalAlignment.Center;
            button.width = width;

            return button;
        }
        protected void AddLabel(UIHelper helper, string text, float size = 1.125f, Color? color = null, int padding = 0)
        {
            var component = helper.self as UIComponent;

            var label = component.AddUIComponent<CustomUILabel>();
            label.text = text;
            label.textScale = size;
            label.textColor = color ?? Color.white;
            label.padding = new RectOffset(padding, 0, 0, 0);
        }
        protected KeymappingsPanel AddKeyMappingPanel(UIHelper helper) => (helper.self as UIPanel).gameObject.AddComponent<KeymappingsPanel>();

        protected string[] GetSupportLanguages()
        {
            var languages = new HashSet<string> { "en" };

            var resourceAssembly = $"{Assembly.GetExecutingAssembly().GetName().Name}.resources";

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                var assemblyName = assembly.GetName();
                if (assemblyName.Name == resourceAssembly)
                    languages.Add(assemblyName.CultureInfo.Name.ToLower());
            }

            return languages.OrderBy(l => l).ToArray();
        }

        #endregion
    }
    public class LanguageDropDown : UIDropDown<string>
    {
        public LanguageDropDown()
        {
            SetSettingsStyle(new Vector2(300, 31));
        }
    }
}
