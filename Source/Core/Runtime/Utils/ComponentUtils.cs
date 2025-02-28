using UnityEngine.Events;

namespace VRBuilder.Core.Utils
{
    /// <summary>
    /// Utilities for operations on components.
    /// </summary>
    public static class ComponentUtils
    {
        /// <summary>
        /// Returns true if a persistent listener with the given name is already added to the specified object.
        /// </summary>
        public static bool HasPersistentListener(UnityEventBase unityEvent, UnityEngine.Object target, string methodName)
        {
            int count = unityEvent.GetPersistentEventCount();
            for (int i = 0; i < count; i++)
            {
                if (unityEvent.GetPersistentTarget(i) == target && unityEvent.GetPersistentMethodName(i) == methodName)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Adds a persistent listener to the specified event if it is not present already.
        /// </summary>
        /// <param name="target">Object target of the event.</param>
        /// <param name="eventObject">Object the event is on.</param>
        /// <param name="unityEvent">Event to add the listener to.</param>
        /// <param name="call">Method to bind to the event.</param>
        /// <returns>True if a listener was added, false otherwise.</returns>
        public static bool AddPersistentListener<T>(UnityEngine.Object target, UnityEngine.Object eventObject, UnityEvent<T> unityEvent, UnityAction<T> call)
        {
            if (HasPersistentListener(unityEvent, target, call.Method.Name))
            {
                return false;
            }
            else
            {
# if UNITY_EDITOR
                UnityEditor.Events.UnityEventTools.AddPersistentListener(unityEvent, call);
                UnityEditor.EditorUtility.SetDirty(eventObject);
                return true;
#else
                UnityEngine.Debug.LogError($"{target.name} attempted to add a persistent listener to {unityEvent.ToString()} at runtime. This is supported only at editor time.");
                return false;
#endif
            }
        }
    }
}