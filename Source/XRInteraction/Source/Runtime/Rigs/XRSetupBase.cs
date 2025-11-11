using System;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using VRBuilder.BasicInteraction.RigSetup;

namespace VRBuilder.XRInteraction.Rigs
{
    [Obsolete("This class will be removed in VR Builder 6.0")]
    public abstract class XRSetupBase : InteractionRigProvider
    {
        protected readonly bool IsPrefabMissing;

        public XRSetupBase()
        {
            IsPrefabMissing = Resources.Load(PrefabName) == null;
            Resources.UnloadUnusedAssets();
        }

        protected bool IsEventManagerInScene()
        {
            return UnityEngine.Object.FindFirstObjectByType<XRInteractionManager>() != null;
        }
    }
}