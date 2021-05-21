using ColossalFramework.PlatformServices;
using ColossalFramework.Plugins;
using ColossalFramework.UI;
using ICities;
using ModsCommon.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using static ColossalFramework.Plugins.PluginManager;

namespace ModsCommon.Utilities
{
    public abstract class DependenciesWatcher
    {
        public abstract PluginMessage AddMessage();
        public abstract void RemoveMessage(PluginMessage message);
    }
    public class DependenciesWatcher<TypeMod> : DependenciesWatcher
        where TypeMod : BaseMod<TypeMod>
    {
        public PluginInfo Plugin { get; }
        private List<BaseDependencyWatcher> Dependencies { get; } = new List<BaseDependencyWatcher>();
        private DependenciesMessageBox MessageBox { get; set; }

        public DependenciesWatcher(PluginInfo plugin, List<BaseDependencyInfo> infos)
        {
            Plugin = plugin;
            foreach (var info in infos)
            {
                var dependency = info.GetWatcher(this);
                Dependencies.Add(dependency);
            }

            PluginManager.instance.eventPluginsChanged += Update;

            Update();
        }
        private void Update()
        {
            foreach (var dependency in Dependencies)
                dependency.Update();
        }
        public override PluginMessage AddMessage()
        {
            if (MessageBox == null)
            {
                MessageBox = UI.MessageBox.Show<DependenciesMessageBox>();
                MessageBox.CaptionText = SingletonMod<TypeMod>.NameRaw;
            }

            return MessageBox.Content.AddUIComponent<PluginMessage>();
        }
        public override void RemoveMessage(PluginMessage message)
        {
            MessageBox.Content.RemoveUIComponent(message);
            UnityEngine.Object.Destroy(message);

            if (MessageBox.IsEmpty)
            {
                UI.MessageBox.Hide(MessageBox);
                MessageBox = null;
            }
        }
    }
    public abstract class BaseDependencyInfo
    {
        public DependencyState State { get; }
        public PluginSearcher Searcher { get; }

        public BaseDependencyInfo(DependencyState state, PluginSearcher searcher, string name = null, ulong id = 0ul)
        {
            State = state;
            Searcher = searcher;
        }
        public abstract BaseDependencyWatcher GetWatcher(DependenciesWatcher mainWatcher);
    }
    public class NeedDependencyInfo : BaseDependencyInfo
    {
        public string Name { get; }
        public ulong Id { get; }

        public NeedDependencyInfo(DependencyState state, PluginSearcher searcher, string name, ulong id) : base(state, searcher)
        {
            Name = name;
            Id = id;
        }
        public override BaseDependencyWatcher GetWatcher(DependenciesWatcher mainWatcher) => State switch
        {
            DependencyState.Subscribe => new NeedDependencyWatcher(mainWatcher, this),
            DependencyState.Enable => new EnableDependencyWatcher(mainWatcher, this),
        };
    }
    public class ConflictDependencyInfo : BaseDependencyInfo
    {
        public ConflictDependencyInfo(DependencyState state, PluginSearcher searcher) : base(state, searcher) { }
        public override BaseDependencyWatcher GetWatcher(DependenciesWatcher mainWatcher) => State switch
        {
            DependencyState.Unsubscribe => new UnsubscribeDependencyWatcher(mainWatcher, this),
            DependencyState.Disable => new DisableDependencyWatcher(mainWatcher, this),
        };
    }

    public abstract class BaseDependencyWatcher
    {
        public bool WorkshopAvailable => PlatformService.platformType == PlatformType.Steam && !noWorkshop;

        public DependenciesWatcher MainWatcher { get; }

        public BaseDependencyWatcher(DependenciesWatcher watcher)
        {
            MainWatcher = watcher;
        }

        public abstract void Update();
        protected PluginMessage AddMessage() => MainWatcher.AddMessage();
        protected void RemoveMessage(PluginMessage message) => MainWatcher.RemoveMessage(message);
    }
    public class NeedDependencyWatcher : BaseDependencyWatcher
    {
        protected NeedDependencyInfo Info { get; }
        protected PluginStateWatcher Watcher { get; set; }
        protected PluginMessage Message { get; set; }

        protected virtual bool NeedAddMessage => Watcher == null;
        protected virtual bool NeedRemoveMessage => Watcher != null;

        public NeedDependencyWatcher(DependenciesWatcher watcher, NeedDependencyInfo info) : base(watcher)
        {
            Info = info;
        }

        public override void Update()
        {
            if (PluginUtilities.GetPlugin(Info.Searcher) is not PluginInfo plugin)
                RemoveWatcher();
            else if (Watcher == null || Watcher.Plugin != plugin)
                AddWatcher(plugin);

            UpdateMessage();
        }

        private void AddWatcher(PluginInfo plugin)
        {
            RemoveWatcher();

            Watcher = new PluginStateWatcher(plugin);
            Watcher.StateChanged += WatcherStateChanged;
        }
        private void RemoveWatcher()
        {
            if (Watcher != null)
            {
                Watcher.StateChanged -= WatcherStateChanged;
                Watcher = null;
            }
        }

        private void WatcherStateChanged(PluginInfo plugin, bool state) => UpdateMessage();

        protected void UpdateMessage()
        {
            if (Message != null && NeedRemoveMessage)
            {
                RemoveMessage(Message);
                Message = null;
            }

            if (Message == null && NeedAddMessage)
                Message = AddMessageImpl();
        }
        protected virtual PluginMessage AddMessageImpl()
        {
            var message = AddMessage();
            message.Text = Info.Name;
            message.ButtonText = WorkshopAvailable ? CommonLocalize.Dependency_Subscribe : CommonLocalize.Dependency_Get;
            message.OnButtonClick = Subscribe;

            return message;
        }
        private void Subscribe()
        {
            if (Info.Id != 0ul)
            {
                if (WorkshopAvailable)
                    PlatformService.workshop.Subscribe(new PublishedFileId(Info.Id));
                else
                    Info.Id.OpenWorkshop();
            }
        }
    }
    public class EnableDependencyWatcher : NeedDependencyWatcher
    {
        protected override bool NeedAddMessage => base.NeedAddMessage || !Watcher.IsEnabled;
        protected override bool NeedRemoveMessage => base.NeedRemoveMessage && Watcher.IsEnabled;

        public EnableDependencyWatcher(DependenciesWatcher watcher, NeedDependencyInfo info) : base(watcher, info) { }

        protected override PluginMessage AddMessageImpl()
        {
            if (Watcher == null)
                return base.AddMessageImpl();
            else
            {
                var message = AddMessage();
                message.Text = (Watcher.Plugin.userModInstance as IUserMod).Name;
                message.ButtonText = CommonLocalize.Dependency_Enable;
                message.OnButtonClick = () => Watcher.Plugin.SetState(true);

                return message;
            }
        }
    }
    public abstract class ConflictDependencyWatcher : BaseDependencyWatcher
    {
        protected ConflictDependencyInfo Info { get; }
        protected Dictionary<PluginInfo, PluginStateWatcher> Watchers { get; } = new Dictionary<PluginInfo, PluginStateWatcher>();
        protected Dictionary<PluginInfo, PluginMessage> Messages { get; } = new Dictionary<PluginInfo, PluginMessage>();

        public ConflictDependencyWatcher(DependenciesWatcher watcher, ConflictDependencyInfo info) : base(watcher)
        {
            Info = info;
        }

        public override void Update()
        {
            var plugins = PluginUtilities.GetPlugins(Info.Searcher).ToHashSet();

            foreach (var plugin in plugins)
            {
                if (!Watchers.ContainsKey(plugin))
                {
                    var watcher = new PluginStateWatcher(plugin);
                    watcher.StateChanged += WatcherStateChanged;
                    Watchers[plugin] = watcher;
                }
            }

            foreach (var plugin in Watchers.Keys.ToArray())
            {
                if (!plugins.Contains(plugin))
                {
                    var watcher = Watchers[plugin];
                    watcher.StateChanged -= WatcherStateChanged;
                    Watchers.Remove(plugin);
                }
            }

            UpdateMessages();
        }

        protected virtual void UpdateMessages()
        {
            foreach (var plugin in Messages.Keys.ToArray())
                RemoveMessage(plugin);

            foreach (var plugin in Watchers.Keys)
                AddMessage(plugin);
        }
        protected void RemoveMessage(PluginInfo plugin)
        {
            if (Messages.TryGetValue(plugin, out var message) && NeedRemoveMessage(plugin))
            {
                RemoveMessage(message);
                Messages.Remove(plugin);
            }
        }
        protected void AddMessage(PluginInfo plugin)
        {
            if (!Messages.ContainsKey(plugin) && NeedAddMessage(plugin))
                Messages[plugin] = AddMessageImpl(plugin);
        }

        protected abstract PluginMessage AddMessageImpl(PluginInfo plugin);
        protected abstract bool NeedAddMessage(PluginInfo plugin);
        protected abstract bool NeedRemoveMessage(PluginInfo plugin);

        protected virtual void WatcherStateChanged(PluginInfo plugin, bool state) { }
    }
    public class UnsubscribeDependencyWatcher : ConflictDependencyWatcher
    {
        public UnsubscribeDependencyWatcher(DependenciesWatcher watcher, ConflictDependencyInfo info) : base(watcher, info) { }

        protected override PluginMessage AddMessageImpl(PluginInfo plugin)
        {
            var message = AddMessage();
            message.Text = (plugin.userModInstance as IUserMod).Name;
            message.ButtonText = WorkshopAvailable ? CommonLocalize.Dependency_Unsubscribe : CommonLocalize.Dependency_Remove;
            message.OnButtonClick = () => Unsubscribe(plugin);

            return message;
        }

        protected override bool NeedAddMessage(PluginInfo plugin) => Watchers.ContainsKey(plugin);
        protected override bool NeedRemoveMessage(PluginInfo plugin) => !Watchers.ContainsKey(plugin);

        private void Unsubscribe(PluginInfo plugin)
        {
            if (WorkshopAvailable && plugin.publishedFileID != PublishedFileId.invalid)
                PlatformService.workshop.Unsubscribe(plugin.publishedFileID);
            //else
            //    Info.Id.OpenWorkshop();
        }
    }
    public class DisableDependencyWatcher : ConflictDependencyWatcher
    {
        public DisableDependencyWatcher(DependenciesWatcher watcher, ConflictDependencyInfo info) : base(watcher, info) { }

        protected override PluginMessage AddMessageImpl(PluginInfo plugin)
        {
            var message = AddMessage();
            message.Text = (plugin.userModInstance as IUserMod).Name;
            message.ButtonText = CommonLocalize.Dependency_Disable;
            message.OnButtonClick = () => plugin.SetState(false);

            return message;
        }
        protected override bool NeedAddMessage(PluginInfo plugin) => Watchers.TryGetValue(plugin, out var watcher) && watcher.IsEnabled;
        protected override bool NeedRemoveMessage(PluginInfo plugin) => !Watchers.TryGetValue(plugin, out var watcher) || !watcher.IsEnabled;
        protected override void WatcherStateChanged(PluginInfo plugin, bool state)
        {
            RemoveMessage(plugin);
            AddMessage(plugin);
        }
    }

    public class DependenciesMessageBox : MessageBoxBase
    {
        private CustomUIButton OkButton { get; }
        private CustomUIButton CloseButton { get; }

        public Func<bool> OnOkClick { private get; set; }
        public Func<bool> OnCloseClick { private get; set; }

        public UIAutoLayoutScrollablePanel Content => Panel.Content;
        public bool IsEmpty => Content.components.Count == 0;

        public DependenciesMessageBox()
        {
            Panel.Content.autoLayoutPadding = new RectOffset((int)ButtonsSpace, (int)ButtonsSpace, ContentSpacing, 0);

            OkButton = AddButton(OkClick);
            OkButton.text = CommonLocalize.MessageBox_OK;

            CloseButton = AddButton(CloseClick);
            CloseButton.text = CommonLocalize.MessageBox_Cancel;
        }
        private void OkClick()
        {
            if (OnOkClick?.Invoke() != false)
                Close();
        }
        private void CloseClick()
        {
            if (OnCloseClick?.Invoke() != false)
                Close();
        }
    }
    public class PluginMessage : CustomUIPanel
    {
        private CustomUILabel Label { get; }
        private CustomUIButton Button { get; }

        public Action OnButtonClick { private get; set; }

        public string Text { set => Label.text = value; }
        public string ButtonText { set => Button.text = value; }

        public PluginMessage()
        {
            Label = AddUIComponent<CustomUILabel>();
            Label.eventTextChanged += (_, _) => SetLabel();

            Button = AddUIComponent<CustomUIButton>();
            Button.SetMenuStyle();
            Button.size = new Vector2(150f, 30f);
            Button.eventClick += ButtonClick;

            height = 40f;
        }

        private void ButtonClick(UIComponent component, UIMouseEventParameter eventParam) => OnButtonClick?.Invoke();

        protected override void OnSizeChanged()
        {
            base.OnSizeChanged();

            SetLabel();
            SetButton();
        }
        private void SetLabel()
        {
            Label.relativePosition = new Vector2(0f, (height - Label.height) / 2f);
        }
        private void SetButton()
        {
            Button.relativePosition = new Vector2(width - Button.width, (height - Button.height) / 2f);
        }
    }
    public enum DependencyState
    {
        Need = 1,
        Conflict = 2,

        Subscribe = 4 | Need,
        Enable = 8 | Need,
        Unsubscribe = 16 | Conflict,
        Disable = 32 | Conflict,
    }
}
