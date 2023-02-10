using UnityEngine;
using VRBuilder.Core.Properties;
using VRBuilder.XRInteraction.Properties;

namespace VRBuilder.Networking
{
    [RequireComponent(typeof(SnapZoneProperty))]
    public class SnapZonePropertyNetwork : LockablePropertyNetwork
    {
        protected SnapZoneProperty snapZoneProperty;
        protected override LockableProperty lockableProperty => snapZoneProperty;

        private void Awake()
        {
            snapZoneProperty = GetComponent<SnapZoneProperty>();            
        }
    }
}