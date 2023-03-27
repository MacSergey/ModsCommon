using ColossalFramework.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

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
        private static Dictionary<Type, FieldInfo[]> EventFields { get; } = new Dictionary<Type, FieldInfo[]>();


        static ComponentPool()
        {
            SceneManager.activeSceneChanged += (_, _) => Clear();
        }

        private static ComponentType Get<ComponentType>(UIComponent parent, string otherName, int delta, string name = null)
            where ComponentType : UIComponent, IReusable
        {
            if (parent.Find(otherName) is UIComponent before)
                return Get<ComponentType>(parent, name, before.zOrder + delta);
            else
                return Get<ComponentType>(parent, name: name);

        }

        public static ComponentType Get<ComponentType>(UIComponent parent, string name = null, int zOrder = -1)
            where ComponentType : UIComponent
        {
            ComponentType component;

            var queue = GetQueue(typeof(ComponentType));
            if (queue.Count != 0)
            {
                component = queue.Dequeue() as ComponentType;
                parent.AttachUIComponent(component.gameObject);
            }
            else
                component = parent.AddUIComponent<ComponentType>();

            if (component is IReusable reusable)
                reusable.InCache = false;

            if (name != null)
                component.cachedName = name;
            if (zOrder != -1)
                component.zOrder = zOrder;

            return component;
        }

        public static ComponentType GetAfter<ComponentType>(UIComponent parent, string beforeName, string name = null)
            where ComponentType : UIComponent, IReusable
        => Get<ComponentType>(parent, beforeName, 1, name);
        public static ComponentType GetBefore<ComponentType>(UIComponent parent, string afterName, string name = null)
            where ComponentType : UIComponent, IReusable
        => Get<ComponentType>(parent, afterName, 0, name);

        public static void Free<ComponentType>(ComponentType component)
            where ComponentType : UIComponent
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

                    var type = component.GetType();
                    if (!EventFields.TryGetValue(type, out var eventFields))
                    {
                        eventFields = GetFields(type);
                        EventFields[type] = eventFields;
                    }
                    foreach (var field in eventFields)
                        field.SetValue(component, null);

                    var queue = GetQueue(type);
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
        private static FieldInfo[] GetFields(Type type)
        {
            if (type == null)
                return new FieldInfo[0];
            else
            {
                var flags = BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
                var fields = type.GetFields(flags).Where(f => typeof(Delegate).IsAssignableFrom(f.FieldType) && !f.IsDefined(typeof(HideInInspector), inherit: true));
                fields = fields.Concat(GetFields(type.BaseType));
                return fields.ToArray();
            }
        }

        private static void Clear()
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
