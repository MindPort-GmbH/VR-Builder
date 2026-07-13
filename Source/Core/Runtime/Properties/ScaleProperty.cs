// Copyright (c) 2026 Aron Schaub
// SPDX-License-Identifier: Apache-2.0

using UnityEngine;
using VRBuilder.Core.Configuration;
using VRBuilder.Core.Primitives;
using VRBuilder.Core.Runtime.Utils;

namespace VRBuilder.Core.Properties
{
    public class ScaleProperty : ProcessSceneObjectProperty, IScaleProperty
    {
        private Vector3 initialScale;

        private void Start()
        {
            initialScale = transform.localScale;
        }

        public void ScaleTo(IVector3 targetScale, float progress)
        {
            RuntimeConfigurator.Configuration.SceneObjectManager.RequestAuthority(SceneObject);
            transform.localScale = Vector3.LerpUnclamped(initialScale, targetScale.ToUnity(), progress);
        }
    }
}