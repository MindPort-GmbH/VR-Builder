// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2023 MindPort GmbH

using System.Collections.Generic;
using System.Linq;
using VRBuilder.Core.Configuration;
using VRBuilder.Core.Configuration.Modes;

namespace VRBuilder.Tests.Utils
{
    /// <summary>
    /// DynamicDefinition allows to dynamically adjust process modes.
    /// </summary>
    public class DynamicRuntimeConfiguration : DefaultRuntimeConfiguration
    {
        public DynamicRuntimeConfiguration()
        {
            Modes = new BaseModeHandler(new List<IMode> {DefaultMode});
        }

        public void SetAvailableModes(IList<IMode> modes)
        {
            Modes = new BaseModeHandler(modes.ToList());
        }

        public DynamicRuntimeConfiguration(params IMode[] modes)
        {
            SetAvailableModes(modes.ToList());
        }
    }
}
