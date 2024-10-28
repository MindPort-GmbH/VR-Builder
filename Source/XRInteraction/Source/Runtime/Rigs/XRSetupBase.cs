using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using VRBuilder.BasicInteraction.RigSetup;

namespace VRBuilder.XRInteraction.Rigs
{
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
            return Object.FindFirstObjectByType<XRInteractionManager>() != null;
        }
    }
}