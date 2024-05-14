using System;
using System.Linq;
using VRBuilder.Core.Conditions;
using VRBuilder.Core.Configuration;
using VRBuilder.Core.SceneObjects;
using VRBuilder.Core.Utils;

namespace VRBuilder.Core.Tests.Utils.Builders
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
            return RuntimeConfigurator.Configuration.SceneObjectRegistry.GetObjects(Guid.Parse(name)).FirstOrDefault();
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
            return PutIntoCollider(name, ProcessReferenceUtils.GetUniqueIdFrom(targetCollider).ToString(), triggerDelay, objectsToPut.Select(o => ProcessReferenceUtils.GetUniqueIdFrom(o).ToString()).ToArray());
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
                builder.AddCondition(new ObjectInColliderCondition(Guid.Parse(targetCollider), Guid.Parse(objectToPut), 0));
            }

            return builder;
        }
    }
}
