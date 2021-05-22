using ColossalFramework.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ModsCommon.UI
{
    public interface IReusable
    {
        void DeInit();
        bool InCache { get; set; }
    }
    public static class ComponentPool
    {
        private static Dictionary<Type, Queue<UIComponent>> Pool { get; } = new Dictionary<Type, Queue<UIComponent>>();

        public static Component GetAfter<Component>(UIComponent parent, string beforeName, string name = null)
            where Component : UIComponent, IReusable
        => Get<Component>(parent, beforeName, 1, name);
        public static Component GetBefore<Component>(UIComponent parent, string afterName, string name = null)
            where Component : UIComponent, IReusable
        => Get<Component>(parent, afterName, 0, name);

        private static Component Get<Component>(UIComponent parent, string otherName, int delta, string name = null)
                where Component : UIComponent, IReusable
        {
            if (parent.Find(otherName) is UIComponent before)
                return Get<Component>(parent, name, before.zOrder + delta);
            else
                return Get<Component>(parent, name: name);

        }

        public static Component Get<Component>(UIComponent parent, string name = null, int zOrder = -1)
            where Component : UIComponent, IReusable
        {
            Component component;

            var queue = GetQueue(typeof(Component));
            if (queue.Count != 0)
            {
                component = queue.Dequeue() as Component;
                parent.AttachUIComponent(component.gameObject);
            }
            else
                component = parent.AddUIComponent<Component>();

            component.InCache = false;

            if (name != null)
                component.cachedName = name;
            if (zOrder != -1)
                component.zOrder = zOrder;

            return component;
        }

        public static void Free<Component>(Component component)
            where Component : UIComponent
        {
            if (component is IReusable reusable)
            {
                if (!reusable.InCache)
                {
                    component.parent?.RemoveUIComponent(component);
                    component.transform.parent = null;
                    component.cachedName = string.Empty;
                    component.isVisible = true;
                    component.isEnabled = true;
                    reusable.DeInit();

                    var queue = GetQueue(component.GetType());
                    queue.Enqueue(component);
                    reusable.InCache = true;
                }
            }
            else
                Delete(component);
        }

        private static Queue<UIComponent> GetQueue(Type type)
        {
            if (!Pool.TryGetValue(type, out Queue<UIComponent> queue))
            {
                queue = new Queue<UIComponent>();
                Pool[type] = queue;
            }
            return queue;
        }

        public static void Clear()
        {
            foreach (var type in Pool.Values)
            {
                while (type.Any())
                    Delete(type.Dequeue());
            }

            Pool.Clear();
        }
        private static void Delete(UIComponent component)
        {
            if (component != null)
            {
                component.parent?.RemoveUIComponent(component);
                UnityEngine.Object.Destroy(component.gameObject);
                UnityEngine.Object.Destroy(component);
            }
        }
    }
}
