﻿using ColossalFramework;
using ColossalFramework.Math;
using ColossalFramework.UI;
using ModsCommon.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ModsCommon
{
    public abstract class BaseTool : ToolBase
    {
        #region STATIC

        public static Segment3 Ray { get; set; }
        public static Ray MouseRay { get; private set; }
        public static float MouseRayLength { get; private set; }
        public static bool MouseRayValid { get; private set; }
        public static Vector3 MousePosition { get; private set; }
        public static Vector3 MousePositionScaled { get; private set; }
        public static Vector3 MouseWorldPosition { get; private set; }
        public static Vector3 CameraDirection { get; private set; }

        #endregion

        #region PROPERTIES

        protected bool IsInit { get; set; } = false;
        public IToolMode Mode { get; private set; }
        public IToolMode NextMode { get; private set; }

        private ToolBase PrevTool { get; set; }

        protected abstract bool ShowToolTip { get; }
        protected abstract IToolMode DefaultMode { get; }

        #endregion

        #region BASIC

        public static BaseTool Instance { get; set; }
        protected static void Create<ToolType>()
            where ToolType : BaseTool
        {
            if (ToolsModifierControl.toolController.gameObject.GetComponent<ToolType>() is not ToolType)
            {
                BaseMod.Logger.Debug($"Create tool");
                Instance = ToolsModifierControl.toolController.gameObject.AddComponent<ToolType>();
            }
        }
        public BaseTool()
        {
            enabled = false;
        }
        public void Init()
        {
            if (IsInit)
                return;

            BaseMod.Logger.Debug($"Init tool");

            InitProcess();

            IsInit = true;

            BaseMod.Logger.Debug($"Tool inited");
        }
        protected virtual void InitProcess() { }

        public Mode CreateToolMode<Mode>() where Mode : BaseToolMode => gameObject.AddComponent<Mode>();

        public static void Remove()
        {
            BaseMod.Logger.Debug($"Remove tool");
            if (Instance != null)
            {
                Destroy(Instance);
                Instance = null;
                BaseMod.Logger.Debug($"Tool removed");
            }
        }
        protected override void OnEnable()
        {
            BaseMod.Logger.Debug($"Enable tool");
            Reset();
            base.OnEnable();
        }
        protected override void OnDisable()
        {
            BaseMod.Logger.Debug($"Disable tool");
            Reset();
        }
        protected void Reset()
        {
            NextMode = null;
            SetModeNow(DefaultMode);
            cursorInfoLabel.isVisible = false;
            cursorInfoLabel.text = string.Empty;
        }
        public void ToggleTool()
        {
            if (ToolsModifierControl.toolController.CurrentTool == this)
                Disable();
            else
                Enable();
        }
        public abstract void Enable();
        protected void Enable<ToolType>()
            where ToolType : BaseTool
        {
            PrevTool = ToolsModifierControl.toolController.CurrentTool;
            ToolsModifierControl.SetTool<ToolType>();
        }
        public void Disable(bool setPrev = true)
        {
            if (setPrev && PrevTool != null)
                ToolsModifierControl.toolController.CurrentTool = PrevTool;
            else
                ToolsModifierControl.SetTool<DefaultTool>();

            PrevTool = null;
        }
        public virtual void Escape()
        {
            if (!Mode.OnEscape())
                Disable();
        }
        public void SetMode(IToolMode mode)
        {
            if (Mode != mode)
                NextMode = mode;
        }
        protected virtual void SetModeNow(IToolMode mode)
        {
            Mode?.Deactivate();
            var prevMode = Mode;
            Mode = mode;
            Mode?.Activate(prevMode);
        }

        #endregion

        #region UPDATE
        protected override void OnToolUpdate()
        {
            if (NextMode != null)
            {
                var nextMode = NextMode;
                NextMode = null;
                SetModeNow(nextMode);
            }
            if (Singleton<InfoManager>.instance.NextMode != InfoManager.InfoMode.None)
            {
                Disable(false);
                return;
            }

            var uiView = UIView.GetAView();
            MousePosition = uiView.ScreenPointToGUI(Input.mousePosition / uiView.inputScale);
            MousePositionScaled = MousePosition * uiView.inputScale;
            MouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            MouseRayLength = Camera.main.farClipPlane;
            MouseRayValid = !UIView.IsInsideUI() && Cursor.visible;
            Ray = new Segment3(MouseRay.origin, MouseRay.origin + MouseRay.direction.normalized * MouseRayLength);
            RayCast(new RaycastInput(MouseRay, MouseRayLength), out RaycastOutput output);
            MouseWorldPosition = output.m_hitPos;

            var cameraDirection = Vector3.forward.TurnDeg(Camera.main.transform.eulerAngles.y, true);
            cameraDirection.y = 0;
            CameraDirection = cameraDirection.normalized;

            Mode.OnToolUpdate();
            Info();

            base.OnToolUpdate();
        }

        #endregion

        #region INFO

        private void Info()
        {
            if (!UIView.HasModalInput() && ShowToolTip && Mode.GetToolInfo() is string info && !string.IsNullOrEmpty(info))
                ShowToolInfo(Mode.GetToolInfo());
            else
                cursorInfoLabel.isVisible = false;
        }
        private void ShowToolInfo(string text)
        {
            if (cursorInfoLabel == null)
                return;

            cursorInfoLabel.isVisible = true;
            cursorInfoLabel.text = text ?? string.Empty;

            UIView uIView = cursorInfoLabel.GetUIView();

            var relativePosition = MousePosition + new Vector3(25, 25);

            var screenSize = fullscreenContainer?.size ?? uIView.GetScreenResolution();
            relativePosition.x = MathPos(relativePosition.x, cursorInfoLabel.width, screenSize.x);
            relativePosition.y = MathPos(relativePosition.y, cursorInfoLabel.height, screenSize.y);

            cursorInfoLabel.relativePosition = relativePosition;

            static float MathPos(float pos, float size, float screen) => pos + size > screen ? (screen - size < 0 ? 0 : screen - size) : Mathf.Max(pos, 0);
        }

        #endregion

        #region OVERLAY

        public override void RenderOverlay(RenderManager.CameraInfo cameraInfo)
        {
            Mode.RenderOverlay(cameraInfo);
            base.RenderOverlay(cameraInfo);
        }

        #endregion
    }
    public abstract class BaseTool<TypeTool, TypeModeType> : BaseTool
        where TypeTool : BaseTool
        where TypeModeType : Enum
    {
        public static void Create() => Create<TypeTool>();

        protected Dictionary<TypeModeType, IToolMode<TypeModeType>> ToolModes { get; set; } = new Dictionary<TypeModeType, IToolMode<TypeModeType>>();
        public new IToolMode<TypeModeType> Mode => base.Mode as IToolMode<TypeModeType>;
        public TypeModeType CurrentMode => Mode != null ? Mode.Type : 0.ToEnum<TypeModeType>();

        protected abstract IEnumerable<IToolMode<TypeModeType>> GetModes();
        protected override void InitProcess() => ToolModes = GetModes().ToDictionary(i => i.Type, i => i);

        public override void Enable() => Enable<TypeTool>();
    }
}
