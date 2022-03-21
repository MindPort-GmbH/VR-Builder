// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021 MindPort GmbH

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using VRBuilder.Core.SceneObjects;
using VRBuilder.Core.Properties;
using VRBuilder.Core.Utils;
using VRBuilder.Tests.Utils;
using UnityEngine;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace VRBuilder.Tests.Properties
{
    public class ProcessPropertyTests : RuntimeTests
    {
        public static IEnumerable<Type> ProcessProperties
        {
            get { return ReflectionUtils.GetConcreteImplementationsOf(typeof(ProcessSceneObjectProperty)).Where(type => type.IsPublic); }
        }

        public static readonly IEnumerable<Type> NotProcessProperties = new Type[]
        {
            typeof(Rigidbody),
            typeof(BoxCollider),
            typeof(Camera),
            typeof(Light),
            typeof(UnityEngine.Video.VideoPlayer),
            typeof(UnityEngine.UI.Button)
        };

        protected ISceneObject SceneObject;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            GameObject gameObject = new GameObject("Scene Object");
            SceneObject = gameObject.AddComponent<ProcessSceneObject>();
        }

        [UnityTest]
        public IEnumerator AddProcessProperties()
        {
            // Given a ISceneObject.

            // Required for ColliderWithTriggerProperty
            SceneObject.GameObject.AddComponent<BoxCollider>().isTrigger = true;

            foreach (Type propertyType in ProcessProperties)
            {
                // When adding the ISceneObjectProperty to the ISceneObject.
                SceneObject.AddProcessProperty(propertyType);

                yield return null;

                // Then assert that the ISceneObjectProperty is part of ISceneObject.
                Assert.That(SceneObject.GameObject.GetComponent(propertyType));
            }

            int totalOfPublicProperties = ProcessProperties.Count();
            int totalOfAddedProperties = SceneObject.Properties.Count;

            // Then assert that the ISceneObject.Properties considers all the ISceneObjectProperty added to ISceneObject.
            Assert.AreEqual(totalOfAddedProperties, totalOfPublicProperties);
        }

        [UnityTest]
        public IEnumerator AddAndRemoveProcessProperties()
        {
            // Given a ISceneObject.

            // Required for ColliderWithTriggerProperty
            SceneObject.GameObject.AddComponent<BoxCollider>().isTrigger = true;

            // When adding a list of ISceneObjectProperty to the ISceneObject.
            yield return AddProcessProperties();

            foreach (Component propertyComponent in SceneObject.GameObject.GetComponents(typeof(ProcessSceneObjectProperty)))
            {
                // When removing a ISceneObjectProperty from the ISceneObject.
                SceneObject.RemoveProcessProperty(propertyComponent);

                yield return null;

                // Then assert that the ISceneObjectProperty is no longer part of ISceneObject.
                Assert.That(SceneObject.GameObject.GetComponent(propertyComponent.GetType()) == null);
            }

            int totalOfAddedProperties = SceneObject.Properties.Count;

            // Then assert that all ISceneObjectProperty were removed.
            Assert.AreEqual(0, totalOfAddedProperties);
        }

        [UnityTest]
        public IEnumerator AddPropertyWithDependencies()
        {
            // Given an ISceneObjectProperty and a list with its dependencies.
            List<Type> dependencies = new List<Type>();
            Type processProperty = ProcessProperties.First(propertyType => GetAllDependenciesFrom(propertyType, ref dependencies));

            if (processProperty == null)
            {
                Debug.LogWarningFormat("AddPropertyWithItsDependencies from {0} was ignored because no ProcessProperties with dependencies could be found.", GetType().Name);
                Assert.Ignore();
            }

            // When adding the ISceneObjectProperty.
            // Then assert that all its dependencies were added.
            yield return AddPropertyAndVerifyDependencies(processProperty, dependencies);
        }

        [UnityTest]
        public IEnumerator RemovePropertyAndDependencies()
        {
            // Given an ISceneObjectProperty and a list with its dependencies.
            List<Type> dependencies = new List<Type>();
            Type processProperty = ProcessProperties.First(propertyType => GetAllDependenciesFrom(propertyType, ref dependencies));

            if (processProperty == null)
            {
                Debug.LogWarningFormat("RemovePropertyAndDependencies from {0} was ignored because no ProcessProperties with dependencies could be found.", GetType().Name);
                Assert.Ignore();
            }

            // When adding the ISceneObjectProperty, we also make sure that all its dependencies were added.
            yield return AddPropertyAndVerifyDependencies(processProperty, dependencies);

            // When removing the ISceneObjectProperty forcing to remove its dependencies.
            SceneObject.RemoveProcessProperty(processProperty, true);

            yield return null;

            // Then assert that the ISceneObjectProperty is not longer part of ISceneObject.
            Assert.That(SceneObject.GameObject.GetComponent(processProperty) == null);

            foreach (Type dependency in dependencies)
            {
                // Then assert that the dependencies of the ISceneObjectProperty are not longer part of ISceneObject.
                Assert.That(SceneObject.GameObject.GetComponent(dependency) == null);
            }
        }

        [UnityTest]
        public IEnumerator AddPropertyAndRemoveDependency()
        {
            // Given an ISceneObjectProperty, a list with its dependencies and a type dependant of ISceneObjectProperty.
            List<Type> dependencies = new List<Type>();
            Type processProperty = ProcessProperties.First(propertyType => GetAllDependenciesFrom(propertyType, ref dependencies, true)), dependantType = dependencies.First();

            if (processProperty == null || dependantType == null)
            {
                Debug.LogWarningFormat("AddPropertyAndRemoveDependency from {0} was ignored because no ProcessProperties with dependencies as ISceneObjectProperty could be found.", GetType().Name);
                Assert.Ignore();
            }

            // When adding adding the ISceneObjectProperty, we also make sure that all its dependencies were added.
            yield return AddPropertyAndVerifyDependencies(processProperty, dependencies);

            // Then assert that the ISceneObjectProperty and the property that depends of it are part of ISceneObject.
            Assert.That(SceneObject.GameObject.GetComponent(processProperty));
            Assert.That(SceneObject.GameObject.GetComponent(dependantType));

            // When removing the property that depends of ISceneObjectProperty.
            SceneObject.RemoveProcessProperty(dependantType);

            yield return null;

            // Then assert that the ISceneObjectProperty and the property that depends of it are no longer part of ISceneObject.
            Assert.That(SceneObject.GameObject.GetComponent(processProperty) == null);
            Assert.That(SceneObject.GameObject.GetComponent(dependantType) == null);
        }

        [UnityTest]
        public IEnumerator RemovePropertyWithoutDependencies()
        {
            // Given an ISceneObjectProperty and a list with its dependencies.
            List<Type> dependencies = new List<Type>();
            Type processProperty = ProcessProperties.First(propertyType => GetAllDependenciesFrom(propertyType, ref dependencies));

            if (processProperty == null)
            {
                Debug.LogWarningFormat("RemovePropertyAndDependencies from {0} was ignored because no ProcessProperties with dependencies could be found.", GetType().Name);
                Assert.Ignore();
            }

            // When adding adding the ISceneObjectProperty, we also make sure that all its dependencies were added.
            yield return AddPropertyAndVerifyDependencies(processProperty, dependencies);

            // When removing the ISceneObjectProperty without forcing to remove its dependencies.
            // Parameter removeDependencies is automatically initialized as false.
            SceneObject.RemoveProcessProperty(processProperty);

            yield return null;

            // Then assert that the ISceneObjectProperty is not longer part of ISceneObject.
            Assert.That(SceneObject.GameObject.GetComponent(processProperty) == null);

            foreach (Type dependency in dependencies)
            {
                // Then assert that the dependencies of the ISceneObjectProperty continuing in ISceneObject.
                Assert.That(SceneObject.GameObject.GetComponent(dependency));
            }
        }

        [UnityTest]
        public IEnumerator TryToAddNotProcessProperties()
        {
            // Given an ISceneObject and a list of non-ISceneObjectProperty types.
            foreach (Type notAPropertyType in NotProcessProperties)
            {
                // When trying to add a non-ISceneObjectProperty using ISceneObject.AddProcessProperty.
                SceneObject.AddProcessProperty(notAPropertyType);

                yield return null;

                // Then assert that the type was not attached to ISceneObject.
                Assert.That(SceneObject.GameObject.GetComponent(notAPropertyType) == null);
            }
        }

        private IEnumerator AddPropertyAndVerifyDependencies(Type processProperty, List<Type> dependencies)
        {
            // Given an ISceneObject.
            SceneObject.AddProcessProperty(processProperty);

            // When adding an ISceneObjectProperty.
            Assert.That(SceneObject.GameObject.GetComponent(processProperty));

            yield return null;

            foreach (Type dependency in dependencies)
            {
                // Then assert that ISceneObject has all the dependencies required by the ISceneObjectProperty.
                Assert.That(SceneObject.GameObject.GetComponent(dependency));
            }
        }

        private bool GetAllDependenciesFrom(Type processProperty, ref List<Type> dependencies, bool onlyProcessProperties = false)
        {
            RequireComponent[] requireComponents = processProperty.GetCustomAttributes(typeof(RequireComponent), false) as RequireComponent[];

            if (requireComponents == null || requireComponents.Length == 0)
            {
                return false;
            }

            foreach (RequireComponent requireComponent in requireComponents)
            {
                AddTypeToListIfNew(requireComponent.m_Type0, ref dependencies, onlyProcessProperties);
                AddTypeToListIfNew(requireComponent.m_Type1, ref dependencies, onlyProcessProperties);
                AddTypeToListIfNew(requireComponent.m_Type2, ref dependencies, onlyProcessProperties);
            }

            return dependencies.Count > 0;
        }

        private void AddTypeToListIfNew(Type type, ref List<Type> dependencies, bool onlyProcessProperties = false)
        {
            if (type != null && dependencies.Contains(type) == false)
            {
                if (onlyProcessProperties && typeof(ISceneObjectProperty).IsAssignableFrom(type) == false)
                {
                    return;
                }

                dependencies.Add(type);
            }
        }
    }
}
