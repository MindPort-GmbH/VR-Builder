using UnityEngine;

namespace VRBuilder.XRInteraction.XRI
{
    /// <summary>
    /// Utility methods for locating component instances.
    /// </summary>
    /// <remarks>This is an internal class in XRI 3, it has been copied to allow external access.</remarks>
    /// <typeparam name="T">The component type.</typeparam>
    public static class ComponentLocatorUtility<T> where T : Component
    {
        /// <summary>
        /// Cached reference to a found component of type <see cref="T"/>.
        /// </summary>
        static T s_ComponentCache;

        /// <summary>
        /// Cached reference to a found component of type <see cref="T"/>.
        /// </summary>
        internal static T componentCache => s_ComponentCache;

        /// <summary>
        /// Last frame that <see cref="Find"/> was called.
        /// </summary>
        static int s_LastTryFindFrame = -1;

        static bool FindWasPerformedThisFrame() => s_LastTryFindFrame == Time.frameCount;

        /// <summary>
        /// Find or create a new GameObject with component <typeparamref name="T"/>.
        /// </summary>
        /// <returns>Returns the found or created component.</returns>
        /// <remarks>
        /// Does not include inactive GameObjects when finding the component, but if a component was previously created
        /// as a direct result of this class, it will return that component even if the GameObject is now inactive.
        /// </remarks>
        public static T FindOrCreateComponent()
        {
            if (s_ComponentCache == null)
            {
                s_ComponentCache = Find();

                if (s_ComponentCache == null)
                    s_ComponentCache = new GameObject(typeof(T).Name, typeof(T)).GetComponent<T>();
            }

            return s_ComponentCache;
        }

        /// <summary>
        /// Find a component <typeparamref name="T"/>.
        /// </summary>
        /// <returns>Returns the found component, or <see langword="null"/> if one could not be found.</returns>
        /// <remarks>
        /// Does not include inactive GameObjects when finding the component, but if a component was previously created
        /// as a direct result of this class, it will return that component even if the GameObject is now inactive.
        /// </remarks>
        public static T FindComponent()
        {
            TryFindComponent(out var component);
            return component;
        }

        /// <summary>
        /// Find a component <typeparamref name="T"/>.
        /// </summary>
        /// <param name="component">When this method returns, contains the found component, or <see langword="null"/> if one could not be found.</param>
        /// <returns>Returns <see langword="true"/> if the component exists, otherwise returns <see langword="false"/>.</returns>
        /// <remarks>
        /// Does not include inactive GameObjects when finding the component, but if a component was previously created
        /// as a direct result of this class, it will return that component even if the GameObject is now inactive.
        /// </remarks>
        /// <see cref="FindOrCreateComponent"/>
        public static bool TryFindComponent(out T component)
        {
            if (s_ComponentCache != null)
            {
                component = s_ComponentCache;
                return true;
            }

            s_ComponentCache = Find();
            component = s_ComponentCache;
            return component != null;
        }

        /// <summary>
        /// Find a component <typeparamref name="T"/>.
        /// </summary>
        /// <param name="component">When this method returns, contains the found component, or <see langword="null"/> if one could not be found.</param>
        /// <param name="limitTryFindPerFrame">If <see langword="true"/>, this method will only perform <see cref="Find"/> if it has not already been unsuccessfully called this frame.</param>
        /// <returns>Returns <see langword="true"/> if the component exists, otherwise returns <see langword="false"/>.</returns>
        /// <remarks>This function will return a cached component from a previous search regardless if <see cref="limitTryFindPerFrame"/> is <see langword="true"/>.</remarks>
        internal static bool TryFindComponent(out T component, bool limitTryFindPerFrame)
        {
            // If a search for this component has already been unsuccessfully performed this frame, don't search again.
            if (limitTryFindPerFrame && FindWasPerformedThisFrame() && s_ComponentCache == null)
            {
                component = null;
                return false;
            }
            return TryFindComponent(out component);
        }

        static T Find()
        {
            s_LastTryFindFrame = Time.frameCount;

            return Object.FindFirstObjectByType<T>();
        }
    }
}