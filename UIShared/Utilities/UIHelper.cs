using ColossalFramework.UI;
using ModsCommon.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ModsCommon.UI
{
    public static class UIHelper
    {
        public static float PropertyScrollTimeout => 3f;
        private static UIView UIRoot { get; set; } = null;

        private static void FindUIRoot()
        {
            UIRoot = null;
            foreach (UIView uiview in UnityEngine.Object.FindObjectsOfType<UIView>())
            {
                bool flag = uiview.transform.parent == null && uiview.name == "UIView";
                if (flag)
                {
                    UIRoot = uiview;
                    break;
                }
            }
        }
        public static bool FindComponent<T>(string name, out T component, UIComponent parent = null, FindOptions options = FindOptions.None)
            where T : MonoBehaviour
        {
            if (FindComponents<T>(name, parent, options).FirstOrDefault() is T found)
            {
                component = found;
                return true;
            }
            else
            {
                component = null;
                return false;
            }
        }
        public static IEnumerable<T> FindComponents<T>(string name, UIComponent parent = null, FindOptions options = FindOptions.None)
            where T : MonoBehaviour
        {
            if (UIRoot == null)
            {
                FindUIRoot();
                if (UIRoot == null)
                    yield break;
            }
            foreach (T component in UnityEngine.Object.FindObjectsOfType<T>())
            {
                if ((options & FindOptions.NameContains) > FindOptions.None ? component.name.Contains(name) : component.name == name)
                {
                    var transform = (parent ?? (MonoBehaviour)UIRoot).transform;
                    var currentParent = component.transform.parent;
                    while (currentParent != null && currentParent != transform)
                        currentParent = currentParent.parent;

                    if (currentParent != null)
                        yield return component;
                }
            }
        }

        public static IEnumerable<T> GetCompenentsWithName<T>(string name)
            where T : UIComponent
        {
            return GameObject.FindObjectsOfType<T>().Where(c => c.name == name);
        }

        [Flags]
        public enum FindOptions
        {
            None = 0,
            NameContains = 1
        }
    }
}
