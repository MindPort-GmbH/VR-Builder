﻿using System;
using System.Collections.Generic;

namespace VRBuilder.UX
{
    /// <summary>
    /// Process controller for standalone devices like the Oculus Quest.
    /// </summary>
    public class StandaloneProcessController : BaseProcessController
    {
        /// <inheritdoc />
        public override string Name { get; } = "Standard";
        
        /// <inheritdoc />
        public override int Priority { get; } = 128;
        
        /// <inheritdoc />
        protected override string PrefabName { get; } = "StandaloneProcessController";       

        /// <inheritdoc />
        public override List<Type> GetRequiredSetupComponents()
        {
            List<Type> requiredComponents = base.GetRequiredSetupComponents();
            return requiredComponents;
        }
    }
}