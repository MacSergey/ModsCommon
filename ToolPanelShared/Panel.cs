using ColossalFramework.UI;
using ModsCommon.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ModsCommon.UI
{
    public interface IToolPanel { }
    public interface IToolHeader : IReusable
    {
        public UIComponent Target { get; set; }
        public void Init();
        public void Refresh();
    }
    public abstract class ToolPanel<TypeMod, TypeTool, TypePanel> : CustomUIPanel, IToolPanel
        where TypeMod : ICustomMod
        where TypeTool : ITool
        where TypePanel : class, IToolPanel
    {
        public static void CreatePanel()
        {
            SingletonMod<TypeMod>.Logger.Debug($"Create panel");
            SingletonItem<TypePanel>.Instance = UIView.GetAView().AddUIComponent(typeof(TypePanel)) as TypePanel;
            SingletonMod<TypeMod>.Logger.Debug($"Panel created");
        }

        protected float HeaderHeight => 42f;
        protected static Vector2 DefaultPosition { get; } = new Vector2(100f, 100f);

        public virtual bool Active
        {
            get => enabled && isVisible;
            set
            {
                enabled = value;
                isVisible = value;
            }
        }
        public virtual bool IsHover => (isVisible && this.IsHover(SingletonTool<TypeTool>.Instance.MousePosition)) || components.Any(c => c.isVisible && c.IsHover(SingletonTool<TypeTool>.Instance.MousePosition));

        public override void Awake()
        {
            base.Awake();
            Active = false;
        }
        public override void Start()
        {
            base.Start();
            SetDefaultPosition();
        }
        public override void OnEnable()
        {
            base.OnEnable();
            CheckPosition();
        }
        private void CheckPosition()
        {
            if (absolutePosition.x < 0 || absolutePosition.y < 0)
                SetDefaultPosition();
        }
        protected override void OnVisibilityChanged()
        {
            base.OnVisibilityChanged();

            if (isVisible)
                RefreshPanel();
        }
        private void SetDefaultPosition()
        {
            SingletonMod<TypeMod>.Logger.Debug($"Set default panel position");
            absolutePosition = DefaultPosition;
        }

        public virtual void RefreshPanel() { }
    }
    public abstract class ToolPanel<TypeMod, TypeTool, TypePanel, TypeHeader> : ToolPanel<TypeMod, TypeTool, TypePanel>
        where TypeMod : ICustomMod
        where TypeTool : ITool
        where TypePanel : class, IToolPanel
        where TypeHeader : UIComponent, IToolHeader
    {
        protected PropertyGroupPanel Content { get; private set; }
        private TypeHeader Header { get; set; }
        private List<EditorItem> Properties { get; set; } = new List<EditorItem>();

        public ToolPanel()
        {
            AddContent();
            AddHeader();
        }
        private void AddContent()
        {
            Content = ComponentPool.Get<PropertyGroupPanel>(this);
            Content.minimumSize = new Vector2(300f, 0f);
            Content.color = new Color32(72, 80, 80, 255);
            Content.autoLayoutDirection = LayoutDirection.Vertical;
            Content.autoFitChildrenVertically = true;
            Content.eventSizeChanged += (UIComponent component, Vector2 value) => size = value;
        }
        private void AddHeader()
        {
            Header = ComponentPool.Get<TypeHeader>(Content);
            Header.Target = this;
            Header.Init();
        }

        public void SetPanel()
        {
            Content.StopLayout();

            ResetPanel();
            SetPanelProcess();
            FillProperties();

            Content.StartLayout();
        }
        protected virtual void SetPanelProcess() { }
        private void ResetPanel()
        {
            Content.StopLayout();

            ResetPanelProcess();
            ClearProperties();

            Content.StartLayout();
        }
        protected virtual void ResetPanelProcess() { }

        private void FillProperties() => Properties = AddProperties();
        protected abstract List<EditorItem> AddProperties();

        private void ClearProperties()
        {
            foreach (var property in Properties)
                ComponentPool.Free(property);

            Properties.Clear();
        }

        public void RefreshHeader() => Header.Refresh();
        public override void RefreshPanel() => RefreshHeader();
    }
}
