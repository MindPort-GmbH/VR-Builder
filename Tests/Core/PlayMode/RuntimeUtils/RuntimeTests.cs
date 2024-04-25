// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2024 MindPort GmbH
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using VRBuilder.Core.Serialization;
using VRBuilder.Core.Serialization.NewtonsoftJson;
using VRBuilder.Core.Utils;
using VRBuilder.Editor;
using VRBuilder.Editor.Setup;
using VRBuilder.Unity;

namespace VRBuilder.Core.Tests.RuntimeUtils
{
    public abstract class RuntimeTests
    {
        protected IProcessSerializer Serializer { get; } = new NewtonsoftJsonProcessSerializer();

        [SetUp]
        public virtual void SetUp()
        {
            UnitTestChecker.IsUnitTesting = true;
            new RuntimeConfigurationSetup().Setup(new DefaultSceneSetupConfiguration());
        }

        [TearDown]
        public virtual void TearDown()
        {
            foreach (GameObject gameObject in SceneUtils.GetActiveAndInactiveGameObjects())
            {
                if (gameObject.name == "Code-based tests runner")
                {
                    continue;
                }

                if (gameObject.GetComponents(typeof(Component)).Any(component => component.GetType().IsSubclassOfGenericDefinition(typeof(UnitySingleton<>))))
                {
                    continue;
                }

                Object.DestroyImmediate(gameObject, false);
            }

            CoroutineDispatcher.Instance.StopAllCoroutines();
            UnitTestChecker.IsUnitTesting = false;
        }
    }
}
