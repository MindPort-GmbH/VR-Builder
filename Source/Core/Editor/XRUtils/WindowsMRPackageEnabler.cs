// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2024 MindPort GmbH

#if UNITY_XR_MANAGEMENT && WINDOWS_XR
namespace VRBuilder.Core.Editor.XRUtils
{
    /// <summary>
    /// Enables the Windows MR Plugin.
    /// </summary>
    internal sealed class WindowsMRPackageEnabler : XRProvider
    {
        /// <inheritdoc/>
        public override string Package { get; } = "com.unity.xr.windowsmr";
        
        /// <inheritdoc/>
        public override int Priority { get; } = 2;

        protected override string XRLoaderName { get; } = "WindowsMRLoader";
    }
}
#endif
