// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2025 MindPort GmbH

using System;

namespace VRBuilder.Core.Editor.UI.StepInspector.Menu
{
    /// <summary>
    /// This is a helper for generic typed class to be able to get the internal items type.
    /// </summary>
    internal interface IInternalTypeProvider
    {
        Type GetItemType();
    }
}
