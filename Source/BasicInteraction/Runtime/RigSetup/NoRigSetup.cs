using System;

namespace VRBuilder.BasicInteraction.RigSetup
{
    /// <summary>
    /// Does not initialize any rig.
    /// </summary>
    [Obsolete("This class will be removed in VR Builder 6.0")]

    public class NoRigSetup : InteractionRigProvider
    {
        /// <inheritdoc/>
        public override string Name { get; } = "<None>";

        /// <inheritdoc/>
        public override string PrefabName { get; } = null;
    }
}