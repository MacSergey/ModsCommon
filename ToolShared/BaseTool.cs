﻿using ColossalFramework;
using ColossalFramework.Math;
using ColossalFramework.UI;
using ICities;
using ModsCommon.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ModsCommon
{
    public interface ITool
    {
        public event Action<bool> OnStateChanged;
        public bool enabled { get; }

        public ICustomMod ModInstance { get; }
        public Shortcut Activation { get; }
        public IEnumerable<Shortcut> Shortcuts { get; }
        public string ToolTip { get; }

        public Segment3 Ray { get; }
        public Ray MouseRay { get; }
        public float MouseRayLength { get; }
        public bool MouseRayValid { get; }
        public Vector3 MousePosition { get; }
        public Vector3 PrevMousePosition { get; }
        public bool MouseMoved { get; }
        public Vector3 MousePositionScaled { get; }
        public Vector3 MouseWorldPosition { get; }
        public Vector3 CameraDirection { get; }

        public void OnLevelLoad();
        public void OnLevelUnload();
        public void Toggle();
        public void Enable();
        public void Disable(bool setPrev = true);
    }
    public abstract class SingletonTool<T> : SingletonItem<T>
    where T : ITool
    {
        public static Shortcut Activation => Instance.Activation;
        public static IEnumerable<Shortcut> Shortcuts => Instance.Shortcuts;
        public static ILogger Logger => Instance.ModInstance.Logger;
    }
    public abstract partial class BaseTool<TypeMod, TypeTool> : ToolBase, ITool
        where TypeMod : ICustomMod
        where TypeTool : ToolBase, ITool
    {
        #region STATIC

        public static void Create()
        {
            if (ToolsModifierControl.toolController.gameObject.GetComponent<TypeTool>() == null)
            {
                SingletonMod<TypeMod>.Instance.Logger.Debug($"Create tool");
                SingletonTool<TypeTool>.Instance = ToolsModifierControl.toolController.gameObject.AddComponent<TypeTool>();
            }
        }

        #endregion

        #region PROPERTIES

        public ICustomMod ModInstance => SingletonMod<TypeMod>.Instance;
        public event Action<bool> OnStateChanged;

        protected bool IsInit { get; set; } = false;
        public IToolMode Mode { get; private set; }
        public IToolMode NextMode { get; private set; }
        public abstract Shortcut Activation { get; }
        public virtual IEnumerable<Shortcut> Shortcuts { get { yield break; } }
        public string ToolTip => Activation.NotSet ? ModInstance.NameRaw : $"{ModInstance.NameRaw} ({Activation})";

        private ToolBase PrevTool { get; set; }

        protected virtual bool ShowToolTip => UIInput.hoveredComponent == null;
        protected abstract IToolMode DefaultMode { get; }

        public Segment3 Ray { get; private set; }
        public Ray MouseRay { get; private set; }
        public float MouseRayLength { get; private set; }
        public virtual bool MouseRayValid => !UIView.HasModalInput() && (UIInput.hoveredComponent?.isInteractive != true) && Cursor.visible;
        public Vector3 MousePosition { get; private set; }
        public Vector3 PrevMousePosition { get; private set; }
        public bool MouseMoved => MousePosition != PrevMousePosition;
        public Vector3 MousePositionScaled { get; private set; }
        public Vector3 MouseWorldPosition { get; private set; }
        public Vector3 CameraDirection { get; private set; }

        #endregion

        #region BASIC

        public BaseTool()
        {
            enabled = false;
        }
        public virtual void OnLevelLoad()
        {
            if (!IsInit)
                Init();
        }
        public virtual void OnLevelUnload() { }

        private void Init()
        {
            ModInstance.Logger.Debug($"Init tool");

            InitProcess();
            IsInit = true;

            ModInstance.Logger.Debug($"Tool inited");
        }

        protected virtual void InitProcess() { }

        public Mode CreateToolMode<Mode>() where Mode : BaseToolMode<TypeTool>
        {
            return gameObject.AddComponent<Mode>();
        }

        protected override void OnEnable()
        {
            ModInstance.Logger.Debug($"Enable tool");
            Reset(DefaultMode);
            base.OnEnable();
            OnStateChanged?.Invoke(true);
        }
        protected override void OnDisable()
        {
            ModInstance.Logger.Debug($"Disable tool");
            Reset(null);
            OnStateChanged?.Invoke(false);
        }
        protected void Reset(IToolMode mode)
        {
            NextMode = null;
            SetModeNow(mode);
            cursorInfoLabel.isVisible = false;
            cursorInfoLabel.text = string.Empty;
            extraInfoLabel.isVisible = false;
            extraInfoLabel.text = string.Empty;

            OnReset();
        }
        protected virtual void OnReset() { }

        public void Toggle()
        {
            if (ToolsModifierControl.toolController.CurrentTool == this)
                Disable();
            else
                Enable();
        }
        public void Enable()
        {
            PrevTool = ToolsModifierControl.toolController.CurrentTool;
            ToolsModifierControl.SetTool<TypeTool>();
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
            //ProcessEnter(Event.current);

            if (!CheckInfoMode(Singleton<InfoManager>.instance.NextMode, Singleton<InfoManager>.instance.NextSubMode))
            {
                Disable(false);
                return;
            }

            if (NextMode is IToolMode nextMode)
            {
                NextMode = null;
                SetModeNow(nextMode);
            }

            UpdateMouse();

            if (Mode is IToolMode mode)
            {
                mode.OnToolUpdate();
                Info(mode);
                ExtraInfo(mode);
            }

            base.OnToolUpdate();
        }

        private bool IsMouseMove { get; set; }
        private float LastPrimary { get; set; }
        private float LastSecondary { get; set; }
        private void ProcessEnter(Event e)
        {
            if (Mode is not IToolMode mode)
                return;

            switch (e.type)
            {
                case EventType.KeyDown:
                case EventType.KeyUp:
                    Shortcuts.FirstOrDefault(s => s.Press(e));
                    break;
                case EventType.MouseDown when MouseRayValid && e.button == 0:
                    IsMouseMove = false;
                    mode.OnMouseDown(e);
                    e.Use();
                    break;
                case EventType.MouseDrag when e.button == 0:
                    IsMouseMove = true;
                    mode.OnMouseDrag(e);
                    e.Use();
                    break;
                case EventType.MouseUp when e.button == 0:
                    if (IsMouseMove)
                    {
                        mode.OnMouseUp(e);
                        e.Use();
                    }
                    else if (Time.realtimeSinceStartup - LastPrimary > 0.25f)
                    {
                        mode.OnPrimaryMouseClicked(e);
                        LastPrimary = Time.realtimeSinceStartup;
                        e.Use();
                    }
                    else
                    {
                        mode.OnPrimaryMouseDoubleClicked(e);
                        LastPrimary = 0f;
                        e.Use();
                    }
                    break;
                case EventType.MouseUp when e.button == 1:
                    if (Time.realtimeSinceStartup - LastSecondary > 0.25f)
                    {
                        mode.OnSecondaryMouseClicked();
                        LastSecondary = Time.realtimeSinceStartup;
                        e.Use();
                    }
                    else
                    {
                        mode.OnSecondaryMouseDoubleClicked();
                        LastSecondary = 0f;
                        e.Use();
                    }
                    break;
            }
        }

        protected virtual bool CheckInfoMode(InfoManager.InfoMode mode, InfoManager.SubInfoMode subInfo) => mode == InfoManager.InfoMode.None && subInfo == InfoManager.SubInfoMode.Default;
        private void UpdateMouse()
        {
            var uiView = UIView.GetAView();
            PrevMousePosition = MousePosition;
            MousePosition = uiView.ScreenPointToGUI(Input.mousePosition / uiView.inputScale);
            MousePositionScaled = MousePosition * uiView.inputScale;
            MouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            MouseRayLength = Camera.main.farClipPlane;
            Ray = new Segment3(MouseRay.origin, MouseRay.origin + MouseRay.direction.normalized * MouseRayLength);
            RayCast(new RaycastInput(MouseRay, MouseRayLength), out RaycastOutput output);
            MouseWorldPosition = output.m_hitPos;

            var cameraDirection = Vector3.forward.TurnDeg(Camera.main.transform.eulerAngles.y, true);
            cameraDirection.y = 0;
            CameraDirection = cameraDirection.normalized;
        }

        protected override void OnToolGUI(Event e)
        {
            ProcessEnter(e);

            if (Mode is IToolMode mode)
                mode.OnToolGUI(e);

            base.OnToolGUI(e);
        }

        #endregion

        #region INFO

        private void Info(IToolMode mode)
        {
            if (!UIView.HasModalInput() && ShowToolTip && mode.GetToolInfo() is string info && !string.IsNullOrEmpty(info))
                ShowToolInfo(info);
            else
                cursorInfoLabel.isVisible = false;
        }
        private void ShowToolInfo(string text)
        {
            if (cursorInfoLabel == null)
                return;

            cursorInfoLabel.BringToFront();
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
        private void ExtraInfo(IToolMode mode)
        {
            if (!UIView.HasModalInput() && mode.GetExtraInfo(out var text, out var color, out float size, out var position, out var direction))
                ShowExtraInfo(text, color, size, position, direction);
            else
                extraInfoLabel.isVisible = false;
        }
        public void ShowExtraInfo(string text, Color color, float size, Vector3 worldPos, Vector3 direction)
        {
            extraInfoLabel.isVisible = true;
            extraInfoLabel.text = text ?? string.Empty;
            extraInfoLabel.textColor = color;
            extraInfoLabel.textScale = size;

            var uIView = extraInfoLabel.GetUIView();
            var startScreenPosition = Camera.main.WorldToScreenPoint(worldPos);
            var endScreenPosition = Camera.main.WorldToScreenPoint(worldPos + direction);
            var screenDir = ((Vector2)(endScreenPosition - startScreenPosition)).normalized;
            screenDir.y *= -1;
            var relativePosition = uIView.ScreenPointToGUI(startScreenPosition / uIView.inputScale) - extraInfoLabel.size * 0.5f + screenDir * (extraInfoLabel.size.magnitude * 0.5f);

            extraInfoLabel.relativePosition = relativePosition;
        }

        #endregion

        #region OVERLAY

        public override void RenderOverlay(RenderManager.CameraInfo cameraInfo)
        {
            Mode.RenderOverlay(cameraInfo);
            base.RenderOverlay(cameraInfo);
        }
        public override void RenderGeometry(RenderManager.CameraInfo cameraInfo)
        {
            Mode.RenderGeometry(cameraInfo);
            base.RenderGeometry(cameraInfo);
        }

        #endregion
    }
    public abstract class BaseTool<TypeMod, TypeTool, TypeModeType> : BaseTool<TypeMod, TypeTool>
        where TypeMod : ICustomMod
        where TypeTool : ToolBase, ITool
        where TypeModeType : Enum
    {
        protected Dictionary<TypeModeType, IToolMode<TypeModeType>> ToolModes { get; set; } = new Dictionary<TypeModeType, IToolMode<TypeModeType>>();
        public IEnumerable<IToolMode<TypeModeType>> Modes => ToolModes.Values;

        public new IToolMode<TypeModeType> Mode => base.Mode as IToolMode<TypeModeType>;
        public TypeModeType CurrentMode => Mode != null ? Mode.Type : 0.ToEnum<TypeModeType>();

        protected abstract IEnumerable<IToolMode<TypeModeType>> GetModes();
        protected override void InitProcess() => ToolModes = GetModes().ToDictionary(i => i.Type, i => i);

        public void SetMode(TypeModeType mode) => SetMode(ToolModes[mode]);
    }
    public abstract class ToolShortcut<TypeMod, TypeTool, TypeMode> : ModShortcut<TypeMod>
    where TypeMod : BaseMod<TypeMod>
    where TypeTool : BaseTool<TypeMod, TypeTool, TypeMode>
    where TypeMode : Enum
    {
        public TypeMode Mode { get; }
        public ToolShortcut(string name, string labelKey, InputKey key, Action action, TypeMode mode) : base(name, labelKey, key, action)
        {
            Mode = mode;
        }
        public override bool Press(Event e) => (SingletonTool<TypeTool>.Instance.CurrentMode.ToInt() & Mode.ToInt()) != 0 && base.Press(e);
    }
    public abstract class BaseThreadingExtension<TypeTool> : ThreadingExtensionBase
        where TypeTool : ITool
    {
        protected virtual bool Detected(TypeTool instance) => !UIView.HasModalInput() && !UIView.HasInputFocus() && instance.Activation.IsPressed;
        public override void OnUpdate(float realTimeDelta, float simulationTimeDelta)
        {
            if (SingletonTool<TypeTool>.Instance is TypeTool toolInstance && Detected(toolInstance))
            {
                SingletonTool<TypeTool>.Logger.Debug($"On press shortcut");
                toolInstance.Toggle();
            }
        }
    }
    public abstract class BaseToolLoadingExtension<TypeTool> : LoadingExtensionBase
        where TypeTool : ITool
    {
        public sealed override void OnLevelLoaded(LoadMode mode)
        {
            switch (mode)
            {
                case LoadMode.NewGame:
                case LoadMode.LoadGame:
                case LoadMode.NewGameFromScenario:
                case LoadMode.NewAsset:
                case LoadMode.LoadAsset:
                case LoadMode.NewMap:
                case LoadMode.LoadMap:
                    OnLoad();
                    break;
            }
        }
        public sealed override void OnLevelUnloading() => OnUnload();

        protected virtual void OnLoad() => SingletonTool<TypeTool>.Instance?.OnLevelLoad();
        protected virtual void OnUnload() => SingletonTool<TypeTool>.Instance?.OnLevelUnload();
    }
}
