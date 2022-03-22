using UnityEngine;
using VRBuilder.Core.Runtime.Utils;

/// <summary>
/// Settings for <see cref="SnapZone"/>s for e.g. automatic creation of such snap zones.
/// </summary>
[CreateAssetMenu(fileName = "InteractionComponentSettings", menuName = "VR Builder/InteractionComponentSettings", order = 1)]
public class InteractionComponentSettings : SettingsObject<InteractionComponentSettings>
{
    [SerializeField]
    public bool EnableXRInteractionComponent = true;
}
