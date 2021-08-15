using ModsCommon.Utilities;
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
        public bool GetExtraInfo(out string text, out Color color, out float size, out Vector3 position, out Vector3 direction);

        public void OnToolGUI(Event e);
        public void OnMouseDown(Event e);
        public void OnMouseDrag(Event e);
        public void OnMouseUp(Event e);
        public void OnPrimaryMouseClicked(Event e);
        public void OnPrimaryMouseDoubleClicked(Event e);
        public void OnSecondaryMouseClicked();
        public void OnSecondaryMouseDoubleClicked();
        public bool OnEscape();
        public void RenderOverlay(RenderManager.CameraInfo cameraInfo);
        public void RenderGeometry(RenderManager.CameraInfo cameraInfo);
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

    public abstract class BaseToolMode<TypeTool> : MonoBehaviour, IToolMode
        where TypeTool : ITool
    {
        protected TypeTool Tool => SingletonTool<TypeTool>.Instance;

        public BaseToolMode()
        {
            Disable();
        }

        public virtual void Activate(IToolMode prevMode)
        {
            enabled = true;
#if DEBUG
            Tool.ModInstance.Logger.Debug($"Enable {GetType().Name}");
#endif
            Reset(prevMode);
        }
        public virtual void Deactivate() => Disable();
        private void Disable()
        {
            enabled = false;
#if DEBUG
            Tool.ModInstance.Logger.Debug($"Disable {GetType().Name}");
#endif
        }

        protected virtual void Reset(IToolMode prevMode) { }

        public virtual void OnToolUpdate() { }
        public virtual string GetToolInfo() => null;
        public virtual bool GetExtraInfo(out string text, out Color color, out float size, out Vector3 position, out Vector3 direction)
        {
            text = default;
            color = default;
            size = default;
            position = default;
            direction = default;
            return false;
        }

        public virtual void OnToolGUI(Event e) { }
        public virtual void OnMouseDown(Event e) { }
        public virtual void OnMouseDrag(Event e) { }
        public virtual void OnMouseUp(Event e) => OnPrimaryMouseClicked(e);
        public virtual void OnPrimaryMouseClicked(Event e) { }
        public virtual void OnPrimaryMouseDoubleClicked(Event e) { }
        public virtual void OnSecondaryMouseClicked() { }
        public virtual void OnSecondaryMouseDoubleClicked() { }
        public virtual bool OnEscape() => false;
        public virtual void RenderOverlay(RenderManager.CameraInfo cameraInfo) { }
        public virtual void RenderGeometry(RenderManager.CameraInfo cameraInfo) { }
    }
}
