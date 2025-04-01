using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;
using VRBuilder.Core.SceneObjects;

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
#if UNITY_EDITOR
                UnityEditor.Events.UnityEventTools.AddPersistentListener(unityEvent, call);
                UnityEditor.EditorUtility.SetDirty(eventObject);
                return true;
#else
                UnityEngine.Debug.LogError($"{target.name} attempted to add a persistent listener to {unityEvent.ToString()} at runtime. This is supported only at editor time.");
                return false;
#endif
            }
        }

        /// <summary>
        /// Performs an automatic undo setup for a scene object.
        /// </summary>
        /// <param name="selectedSceneObject">The GameObject to process.</param>
        /// <param name="valueType">The type used for reflection.</param>
        /// <param name="alreadyAttachedProperties">Array of components that are considered original.</param>
        /// <param name="isUndoOperation">Flag indicating if the undo operation was executed.</param>
        public static void UndoSceneObjectAutomaticSetup(GameObject selectedSceneObject, Type valueType, Component[] alreadyAttachedProperties, ref bool isUndoOperation)
        {
            var sceneObject = selectedSceneObject.GetComponent<VRBuilder.Core.SceneObjects.ProcessSceneObject>();
            if (sceneObject == null)
            {
                isUndoOperation = true;
                return;
            }

            RemoveProcessProperty(sceneObject, valueType, alreadyAttachedProperties);

            List<Component> sortedComponents = GetSortedNonOriginalComponents(selectedSceneObject, alreadyAttachedProperties);
            // Remove components in reverse topological order so that dependents are removed before their dependencies.
            for (int i = sortedComponents.Count - 1; i >= 0; i--)
            {
            }

            isUndoOperation = true;
        }

        /// <summary>
        /// Removes a process property from the scene object based on the provided type.
        /// </summary>
        /// <param name="sceneObject">The scene object.</param>
        /// <param name="valueType">The type for determining the property to remove.</param>
        /// <param name="alreadyAttachedProperties">Array of components that are considered original.</param>
        public static void RemoveProcessProperty(ISceneObject sceneObject, Type valueType, Component[] alreadyAttachedProperties)
        {
            Type concreteTypeToRemove = ReflectionUtils.GetImplementationWithDefaultAttribute(valueType);
            if (concreteTypeToRemove == null)
            {
                concreteTypeToRemove = ReflectionUtils.GetImplementationWithoutDefaultAttribute(valueType);
            }
            if (concreteTypeToRemove != null)
            {
                sceneObject.RemoveProcessProperty(concreteTypeToRemove, true, alreadyAttachedProperties);
            }
        }

        /// <summary>
        /// Retrieves a sorted list of components from a given GameObject that are not part of the 
        /// already attached properties. The sorting is performed using a topological sort to ensure 
        /// dependencies between components are respected.
        /// </summary>
        /// <param name="selectedSceneObject">The GameObject from which to retrieve the components.</param>
        /// <param name="alreadyAttachedProperties">An array of components that are considered original and should be excluded from the result.</param>
        /// <returns>A sorted list of components that are not part of the already attached properties.</returns>
        public static List<Component> GetSortedNonOriginalComponents(GameObject selectedSceneObject, Component[] alreadyAttachedProperties)
        {
            List<Component> nonOriginalComponents = new List<Component>();
            foreach (Component comp in selectedSceneObject.GetComponents<Component>())
            {
                if (!((IEnumerable<Component>)alreadyAttachedProperties).Contains(comp))
                    nonOriginalComponents.Add(comp);
            }

            List<Component> sorted = new List<Component>();
            HashSet<Component> temporaryMark = new HashSet<Component>();
            HashSet<Component> permanentMark = new HashSet<Component>();

            foreach (Component comp in nonOriginalComponents)
            {
                TopoVisit(comp, nonOriginalComponents, sorted, temporaryMark, permanentMark);
            }
            return sorted;
        }

        /// <summary>
        /// Performs a topological visit on a component to resolve dependencies and sort components
        /// based on their requirements.
        /// </summary>
        /// <param name="comp">The component being visited.</param>
        /// <param name="nonOriginal">A list of components that are not part of the original set.</param>
        /// <param name="sorted">The list where sorted components will be added in dependency order.</param>
        /// <param name="temporary">A set of components currently being visited to detect cyclic dependencies.</param>
        /// <param name="permanent">A set of components that have already been visited and sorted.</param>
        private static void TopoVisit(Component comp, List<Component> nonOriginal, List<Component> sorted, HashSet<Component> temporary, HashSet<Component> permanent)
        {
            if (permanent.Contains(comp))
                return;
            if (temporary.Contains(comp))
                return;

            temporary.Add(comp);
            foreach (RequireComponent req in comp.GetType().GetCustomAttributes(typeof(RequireComponent), true))
            {
                Type[] reqTypes = new Type[] { req.m_Type0, req.m_Type1, req.m_Type2 };
                foreach (Type reqType in reqTypes)
                {
                    if (reqType == null)
                        continue;
                    foreach (Component other in nonOriginal)
                    {
                        if (other == comp)
                            continue;
                        if (reqType.IsAssignableFrom(other.GetType()))
                        {
                            TopoVisit(other, nonOriginal, sorted, temporary, permanent);
                        }
                    }
                }
            }
            temporary.Remove(comp);
            permanent.Add(comp);
            sorted.Add(comp);
        }
    }
}