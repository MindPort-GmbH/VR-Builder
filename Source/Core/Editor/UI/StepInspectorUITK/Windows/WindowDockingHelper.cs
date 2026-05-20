using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace VRBuilder.Core.Editor.UI.StepInspectorUITK.Windows
{
    /// <summary>
    /// Reflection wrapper around Unity's internal <c>HostView</c> / <c>DockArea</c> / <c>SplitView</c>
    /// types so we can dock manually-created <see cref="EditorWindow"/> instances together as
    /// real Unity tabs or split them into stacked panes. Once arranged the user can drag any tab
    /// in/out, swap order, or further split via Unity's native dock UI.
    /// </summary>
    /// <remarks>
    /// Unity has no public API for any of this. The internal class / member names targeted here
    /// have been stable across Unity 2020 through 6.x. All operations are wrapped in try/catch —
    /// on any reflection failure we log a warning and the caller can fall back to a simpler
    /// arrangement (free-floating window, tab in anchor's container, …).
    /// </remarks>
    internal static class WindowDockingHelper
    {
        // ───────── public API ─────────

        /// <summary>Docks <paramref name="toDock"/> as a tab of <paramref name="anchor"/>'s container.</summary>
        public static bool DockAsTab(EditorWindow anchor, EditorWindow toDock)
        {
            if (anchor == null || toDock == null || anchor == toDock) return false;

            try
            {
                object anchorHost = GetHostOf(anchor);
                if (anchorHost == null) return false;

                MethodInfo addTab = FindAddTabMethod(anchorHost.GetType());
                if (addTab == null) return false;

                InvokeAddTab(addTab, anchorHost, toDock);
                return true;
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogWarning(
                    $"[VR Builder] DockAsTab failed; falling back to free-floating window. ({e.GetType().Name}: {e.Message})");
                return false;
            }
        }

        /// <summary>
        /// Wraps <paramref name="anchor"/>'s container and a freshly-created <c>DockArea</c>
        /// (holding <paramref name="bottomWindows"/> as tabs) in a vertical <c>SplitView</c>.
        /// Anchor stays on top, the new dock area appears below.
        /// </summary>
        public static bool SplitBelow(EditorWindow anchor, EditorWindow[] bottomWindows)
        {
            if (anchor == null || bottomWindows == null || bottomWindows.Length == 0) return false;

            try
            {
                Assembly editorAsm = typeof(EditorWindow).Assembly;
                Type viewType = editorAsm.GetType("UnityEditor.View");
                Type splitViewType = editorAsm.GetType("UnityEditor.SplitView");
                Type dockAreaType = editorAsm.GetType("UnityEditor.DockArea");
                if (viewType == null || splitViewType == null || dockAreaType == null)
                {
                    UnityEngine.Debug.LogWarning("[VR Builder] SplitBelow: internal Unity types not found via reflection.");
                    return false;
                }

                object anchorDock = GetHostOf(anchor);
                if (anchorDock == null) return false;

                // 1. New DockArea, fill it with the bottom-row windows.
                ScriptableObject newDock = ScriptableObject.CreateInstance(dockAreaType);
                MethodInfo addTabMethod = FindAddTabMethod(dockAreaType);
                if (addTabMethod == null) return false;

                foreach (EditorWindow window in bottomWindows)
                {
                    if (window == null) continue;
                    InvokeAddTab(addTabMethod, newDock, window);
                }

                // 2. Find anchor's parent view + the anchor's index inside it.
                PropertyInfo parentProp = viewType.GetProperty("parent",
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                object anchorParent = parentProp?.GetValue(anchorDock);
                if (anchorParent == null)
                {
                    UnityEngine.Debug.LogWarning("[VR Builder] SplitBelow: anchor has no parent view.");
                    return false;
                }

                int anchorIndex = IndexOfChild(viewType, anchorParent, anchorDock);
                if (anchorIndex < 0)
                {
                    UnityEngine.Debug.LogWarning("[VR Builder] SplitBelow: anchor not found in its parent's children.");
                    return false;
                }

                // 3. If anchor's parent is already a vertical SplitView, just insert the new
                //    dock as the next sibling — no extra wrapping needed.
                if (IsVerticalSplitView(splitViewType, anchorParent))
                {
                    InvokeAddChild(viewType, anchorParent, newDock, anchorIndex + 1);
                    return true;
                }

                // 4. Otherwise wrap (anchorDock, newDock) in a fresh vertical SplitView and
                //    plug it into anchorParent at anchorDock's original index.
                ScriptableObject vertical = ScriptableObject.CreateInstance(splitViewType);
                SetVertical(splitViewType, vertical, true);

                InvokeRemoveChild(viewType, anchorParent, anchorDock);
                InvokeAddChild(viewType, anchorParent, vertical, anchorIndex);
                InvokeAddChild(viewType, vertical, anchorDock, -1);
                InvokeAddChild(viewType, vertical, newDock, -1);

                return true;
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogWarning(
                    $"[VR Builder] SplitBelow failed; falling back to tabbed dock. ({e.GetType().Name}: {e.Message})");
                return false;
            }
        }

        // ───────── reflection helpers ─────────

        private static object GetHostOf(EditorWindow window)
        {
            FieldInfo parentField = typeof(EditorWindow).GetField(
                "m_Parent", BindingFlags.NonPublic | BindingFlags.Instance);
            return parentField?.GetValue(window);
        }

        private static MethodInfo FindAddTabMethod(Type hostType)
        {
            const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

            for (Type current = hostType; current != null; current = current.BaseType)
            {
                MethodInfo m = current.GetMethod(
                    "AddTab", flags, binder: null,
                    types: new[] { typeof(EditorWindow), typeof(bool) },
                    modifiers: null);
                if (m != null) return m;

                m = current.GetMethod(
                    "AddTab", flags, binder: null,
                    types: new[] { typeof(EditorWindow) },
                    modifiers: null);
                if (m != null) return m;
            }
            return null;
        }

        private static void InvokeAddTab(MethodInfo addTab, object host, EditorWindow window)
        {
            ParameterInfo[] pars = addTab.GetParameters();
            object[] args = pars.Length == 2 ? new object[] { window, true } : new object[] { window };
            addTab.Invoke(host, args);
        }

        private static int IndexOfChild(Type viewType, object parent, object child)
        {
            PropertyInfo childrenProp = viewType.GetProperty("children",
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (childrenProp?.GetValue(parent) is not Array children) return -1;

            for (int i = 0; i < children.Length; i++)
            {
                if (ReferenceEquals(children.GetValue(i), child)) return i;
            }
            return -1;
        }

        private static bool IsVerticalSplitView(Type splitViewType, object candidate)
        {
            if (candidate == null || !splitViewType.IsInstanceOfType(candidate)) return false;

            FieldInfo verticalField = splitViewType.GetField("vertical",
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (verticalField != null)
            {
                object v = verticalField.GetValue(candidate);
                if (v is bool b) return b;
            }

            // Some Unity versions expose the orientation via a property instead of a field.
            PropertyInfo verticalProp = splitViewType.GetProperty("vertical",
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (verticalProp != null)
            {
                object v = verticalProp.GetValue(candidate);
                if (v is bool b2) return b2;
            }

            return false;
        }

        private static void SetVertical(Type splitViewType, object splitView, bool value)
        {
            FieldInfo verticalField = splitViewType.GetField("vertical",
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (verticalField != null)
            {
                verticalField.SetValue(splitView, value);
                return;
            }

            PropertyInfo verticalProp = splitViewType.GetProperty("vertical",
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            verticalProp?.SetValue(splitView, value);
        }

        private static void InvokeAddChild(Type viewType, object parent, object child, int index)
        {
            const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

            if (index >= 0)
            {
                MethodInfo addChildIdx = viewType.GetMethod("AddChild", flags, binder: null,
                    types: new[] { viewType, typeof(int) }, modifiers: null);
                if (addChildIdx != null)
                {
                    addChildIdx.Invoke(parent, new object[] { child, index });
                    return;
                }
            }

            MethodInfo addChild = viewType.GetMethod("AddChild", flags, binder: null,
                types: new[] { viewType }, modifiers: null);
            addChild?.Invoke(parent, new object[] { child });
        }

        private static void InvokeRemoveChild(Type viewType, object parent, object child)
        {
            const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            MethodInfo removeChild = viewType.GetMethod("RemoveChild", flags, binder: null,
                types: new[] { viewType }, modifiers: null);
            removeChild?.Invoke(parent, new object[] { child });
        }
    }
}
