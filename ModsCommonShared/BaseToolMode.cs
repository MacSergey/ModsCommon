using System;
using UnityEngine;

namespace ModsCommon
{
    public interface IToolMode
    {
        public void Activate(IToolMode prevMode);
        public void Deactivate();

        public void OnToolUpdate();
        public string GetToolInfo();

        public void OnToolGUI(Event e);
        public void OnMouseDown(Event e);
        public void OnMouseDrag(Event e);
        public void OnMouseUp(Event e);
        public void OnPrimaryMouseClicked(Event e);
        public void OnSecondaryMouseClicked();
        public bool OnEscape();
        public void RenderOverlay(RenderManager.CameraInfo cameraInfo);
    }
    public interface IToolMode<ModeType> : IToolMode
    where ModeType : Enum
    {
        ModeType Type { get; }
    }
    public interface IToolModePanel : IToolMode
    {
        public bool ShowPanel { get; }
    }

    public abstract class BaseToolMode<TypeMod, TypeTool> : MonoBehaviour, IToolMode
        where TypeMod : BaseMod<TypeMod>
        where TypeTool : BaseTool<TypeMod, TypeTool>
    {
        protected TypeTool Tool => SingletonTool<TypeTool>.Instance;

        public BaseToolMode()
        {
            Disable();
        }

        public virtual void Activate(IToolMode prevMode)
        {
            enabled = true;
            Reset(prevMode);
        }
        public virtual void Deactivate() => Disable();
        private void Disable() => enabled = false;

        protected virtual void Reset(IToolMode prevMode) { }

        public virtual void Update() { }

        public virtual void OnToolUpdate() { }
        public virtual string GetToolInfo() => null;

        public virtual void OnToolGUI(Event e) { }
        public virtual void OnMouseDown(Event e) { }
        public virtual void OnMouseDrag(Event e) { }
        public virtual void OnMouseUp(Event e) => OnPrimaryMouseClicked(e);
        public virtual void OnPrimaryMouseClicked(Event e) { }
        public virtual void OnSecondaryMouseClicked() { }
        public virtual bool OnEscape() => false;
        public virtual void RenderOverlay(RenderManager.CameraInfo cameraInfo) { }
    }
}
