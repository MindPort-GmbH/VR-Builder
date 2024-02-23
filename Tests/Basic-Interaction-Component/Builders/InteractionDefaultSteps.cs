using System;
using System.Linq;
using VRBuilder.BasicInteraction.Conditions;
using VRBuilder.BasicInteraction.Properties;
using VRBuilder.Core.Configuration;
using VRBuilder.Core.SceneObjects;
using VRBuilder.Core.Utils;
using VRBuilder.Tests.Builder;

namespace VRBuilder.BasicInteraction.Builders
{
    public static class InteractionDefaultSteps
    {

        /// <summary>
        /// Gets the <see cref="ISceneObject"/> with given <paramref name="name"/> from the registry.
        /// </summary>
        /// <param name="name">Name of scene object.</param>
        /// <returns><see cref="ISceneObject"/> with given name.</returns>
        private static ISceneObject GetFromRegistry(string name)
        {
            return RuntimeConfigurator.Configuration.SceneObjectRegistry.GetByTag(Guid.Parse(name)).FirstOrDefault();
        }

        /// <summary>
        /// Get grab step builder.
        /// </summary>
        /// <param name="name">Name of the step.</param>
        /// <param name="objectsToGrab">List of objects that have to be grabbed before chapter continues.</param>
        /// <returns>Configured builder.</returns>
        public static BasicStepBuilder Grab(string name, params IGrabbableProperty[] objectsToGrab)
        {
            return Grab(name, objectsToGrab.Select(o => ProcessReferenceUtils.GetUniqueIdFrom(o).ToString()).ToArray());
        }

        /// <summary>
        /// Get grab step builder.
        /// </summary>
        /// <param name="name">Name of the step.</param>
        /// <param name="objectsToGrab">List of objects that have to be grabbed before chapter continues.</param>
        /// <returns>Configured builder.</returns>
        public static BasicStepBuilder Grab(string name, params string[] objectsToGrab)
        {
            BasicStepBuilder builder = new BasicStepBuilder(name);

            foreach (string objectToGrab in objectsToGrab)
            {
                builder.AddCondition(new GrabbedObjectWithTagCondition(Guid.Parse(objectToGrab)));
            }

            return builder;
        }

        /// <summary>
        /// Get builder for a step during which user has to put objects into a snap zone.
        /// </summary>
        /// <param name="name">Name of the step.</param>
        /// <param name="snapZone">Snap zone in which user should put objects.</param>
        /// <param name="objectsToPut">List of objects to put into collider.</param>
        /// <returns>Configured builder.</returns>
        public static BasicStepBuilder PutIntoSnapZone(string name, ISnapZoneProperty snapZone, params ISnappableProperty[] objectsToPut)
        {
            BasicStepBuilder builder = new BasicStepBuilder(name);

            foreach (ISnappableProperty objectToPut in objectsToPut)
            {
                builder.AddCondition(new SnappedObjectWithTagCondition(objectToPut, snapZone));
            }

            return builder;
        }

        /// <summary>
        /// Get builder for a step during which user has to put objects into a snap zone.
        /// </summary>
        /// <param name="name">Name of the step.</param>
        /// <param name="snapZone">Snap zone in which user should put objects.</param>
        /// <param name="objectsToPut">List of objects to put into collider.</param>
        /// <returns>Configured builder.</returns>
        public static BasicStepBuilder PutIntoSnapZone(string name, string snapZone, params string[] objectsToPut)
        {
            return PutIntoSnapZone(name, GetFromRegistry(snapZone).GetProperty<ISnapZoneProperty>(), objectsToPut.Select(GetFromRegistry).Select(t => t.GetProperty<ISnappableProperty>()).ToArray());
        }

        /// <summary>
        /// Get builder for a step during which user has to activate some objects.
        /// </summary>
        /// <param name="name">Name of the step.</param>
        /// <param name="objectsToUse">List of objects to use.</param>
        /// <returns>Configured builder.</returns>
        public static BasicStepBuilder Use(string name, params IUsableProperty[] objectsToUse)
        {
            BasicStepBuilder builder = new BasicStepBuilder(name);

            foreach (IUsableProperty objectToUse in objectsToUse)
            {
                builder.AddCondition(new UsedCondition(objectToUse));
            }

            return builder;
        }

        /// <summary>
        /// Get builder for a step during which user has to activate some objects.
        /// </summary>
        /// <param name="name">Name of the step.</param>
        /// <param name="objectsToUse">List of objects to use.</param>
        /// <returns>Configured builder.</returns>
        public static BasicStepBuilder Use(string name, params string[] objectsToUse)
        {
            return Use(name, objectsToUse.Select(GetFromRegistry).Select(t => t.GetProperty<IUsableProperty>()).ToArray());
        }

        /// <summary>
        /// Get builder for a step during which user has to touch some objects.
        /// </summary>
        /// <param name="name">Name of the step.</param>
        /// <param name="objectsToTouch">List of objects to touch.</param>
        /// <returns>Configured builder.</returns>
        public static BasicStepBuilder Touch(string name, params ISceneObject[] objectsToTouch)
        {
            return Touch(name, objectsToTouch.Select(o => ProcessReferenceUtils.GetUniqueIdFrom(o).ToString()).ToArray());
        }

        /// <summary>
        /// Get builder for a step during which user has to touch some objects.
        /// </summary>
        /// <param name="name">Name of the step.</param>
        /// <param name="objectsToTouch">List of objects to touch.</param>
        /// <returns>Configured builder.</returns>
        public static BasicStepBuilder Touch(string name, params string[] objectsToTouch)
        {
            BasicStepBuilder builder = new BasicStepBuilder(name);

            foreach (string objectToTouch in objectsToTouch)
            {
                builder.AddCondition(new TouchedCondition(Guid.Parse(objectToTouch)));
            }

            return builder;
        }
    }
}