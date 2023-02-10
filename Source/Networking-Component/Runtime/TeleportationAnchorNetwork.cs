using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class TeleportationAnchorNetwork : TeleportationAnchor
{
    protected override void OnDeactivated(DeactivateEventArgs args)
    {
        TeleportationProvider provider = args.interactorObject.transform.GetComponentInParent<TeleportationProvider>();
        if (provider != null)
            teleportationProvider = provider;

        base.OnDeactivated(args);
    }
}
