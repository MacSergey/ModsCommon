using System;
using UnityEngine;

namespace ModsCommon
{
    public abstract class BaseToolMode : MonoBehaviour
    {
        public virtual bool ShowPanel => true;

        protected BaseTool Tool => BaseTool.Instance;

        public BaseToolMode()
        {
            Disable();
        }

        public virtual void Activate(BaseToolMode prevMode)
        {
            enabled = true;
            Reset(prevMode);
        }
        public virtual void Deactivate() => Disable();
        private void Disable() => enabled = false;

        protected virtual void Reset(BaseToolMode prevMode) { }

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
    public abstract class BaseToolMode<ModeType> : BaseToolMode
        where ModeType : Enum
    {
        public abstract ModeType Type { get; }
    }
}
