using System.Linq;
using VRBuilder.Core.Conditions;
using VRBuilder.Core.Configuration;
using VRBuilder.Core.SceneObjects;
using VRBuilder.Core.Utils;

namespace VRBuilder.Tests.Builder
{
    /// <summary>
    /// Static class to provide fast access to predefined builders.
    /// </summary>
    public static class BasicProcessSteps
    {
        /// <summary>
        /// Gets the <see cref="ISceneObject"/> with given <paramref name="name"/> from the registry.
        /// </summary>
        /// <param name="name">Name of scene object.</param>
        /// <returns><see cref="ISceneObject"/> with given name.</returns>
        private static ISceneObject GetFromRegistry(string name)
        {
            return RuntimeConfigurator.Configuration.SceneObjectRegistry[name];
        }

        /// <summary>
        /// Get builder for a step where the user has to enter collider.
        /// </summary>
        /// <param name="name">The name of the step.</param>
        /// <param name="targetAreaCollider">Target collider for the user to enter.</param>
        /// <param name="triggerDelay">How long the user should stay inside the collider to continue.</param>
        /// <returns>Configured builder.</returns>
        public static BasicStepBuilder UserInArea(string name, string targetAreaCollider, float triggerDelay = 0f)
        {
            return UserInArea(name, GetFromRegistry(targetAreaCollider), triggerDelay);
        }

        /// <summary>
        /// Get builder for a step where the user has to enter collider.
        /// </summary>
        /// <param name="name">The name of the step.</param>
        /// <param name="targetAreaCollider">Target collider for the user to enter.</param>
        /// <param name="triggerDelay">How long the user should stay inside the collider to continue.</param>
        /// <returns>Configured builder.</returns>
        public static BasicStepBuilder UserInArea(string name, ISceneObject targetAreaCollider, float triggerDelay = 0f)
        {
            return PutIntoCollider(name, targetAreaCollider, triggerDelay, RuntimeConfigurator.Configuration.User);
        }

        /// <summary>
        /// Get builder for a step during which user has to put given objects into given collider.
        /// </summary>
        /// <param name="name">Name of the step.</param>
        /// <param name="targetCollider">Collider in which user should put objects.</param>
        /// <param name="triggerDelay">How long an object should be inside the collider to be registered.</param>
        /// <param name="objectsToPut">List of objects to put into collider.</param>
        /// <returns>Configured builder.</returns>
        public static BasicStepBuilder PutIntoCollider(string name, ISceneObject targetCollider, float triggerDelay = 0f, params ISceneObject[] objectsToPut)
        {
            return PutIntoCollider(name, ProcessReferenceUtils.GetNameFrom(targetCollider), triggerDelay, objectsToPut.Select(ProcessReferenceUtils.GetNameFrom).ToArray());
        }

        /// <summary>
        /// Get builder for a step during which user has to put given objects into given collider.
        /// </summary>
        /// <param name="name">Name of the step.</param>
        /// <param name="targetCollider">Collider in which user should put objects.</param>
        /// <param name="triggerDelay">How long an object should be inside the collider to be registered.</param>
        /// <param name="objectsToPut">List of objects to put into collider.</param>
        /// <returns>Configured builder.</returns>
        public static BasicStepBuilder PutIntoCollider(string name, string targetCollider, float triggerDelay = 0f, params string[] objectsToPut)
        {
            BasicStepBuilder builder = new BasicStepBuilder(name);

            foreach (string objectToPut in objectsToPut)
            {
                builder.AddCondition(new ObjectInColliderCondition(targetCollider, objectToPut, 0));
            }

            return builder;
        }
    }
}
