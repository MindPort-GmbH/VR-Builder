// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021 MindPort GmbH

#if UNITY_XR_MANAGEMENT && OPEN_XR
using System;
using UnityEditor;

namespace VRBuilder.Editor.XRUtils
{
    /// <summary>
    /// Enables the Open XR Plugin.
    /// </summary>
    internal sealed class OpenXRPackageEnabler : XRProvider
    {
        /// <inheritdoc/>
        public override string Package { get; } = "com.unity.xr.openxr";

        /// <inheritdoc/>
        public override int Priority { get; } = 2;

        protected override string XRLoaderName { get; } = "OpenXRLoader";

        protected override void InitializeXRLoader(object sender, EventArgs e)
        {
            base.InitializeXRLoader(sender, e);

            EditorUtility.DisplayDialog("OpenXR Installed", "Please validate your OpenXR configuration in the Project Settings.", "Ok");
        }
    }
}
#endif
