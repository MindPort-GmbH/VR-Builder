// Copyright (c) 2013-2019 Innoactive GmbH
// Modifications copyright (c) 2021-2026 MindPort GmbH
// Modifications copyright (c) 2026 Aron Schaub
// SPDX-License-Identifier: Apache-2.0

using System.Collections.Generic;
using VRBuilder.Core.Configuration.Modes;
using VRBuilder.Core.Properties;
using VRBuilder.Core.Runtime.Registry;

namespace VRBuilder.Core.Configuration
{
    /// <summary>
    /// Process runtime configuration which is used if no other was implemented.
    /// </summary>
    public class DefaultRuntimeConfiguration : BaseRuntimeConfiguration
    {
        private IAudioPlayer audioPlayer;

        public DefaultRuntimeConfiguration()
        {
            Modes = new BaseModeHandler(new List<IModeService> { ServiceRegistry.Get<IModeService>().ActiveOrDefaultMode });
        }
    }
}
