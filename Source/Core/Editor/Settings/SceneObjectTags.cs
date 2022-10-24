using System.Collections.Generic;
using UnityEngine;
using VRBuilder.Core.Runtime.Utils;

namespace VRBuilder.Editor.Settings
{
    public class SceneObjectTags : SettingsObject<SceneObjectTags>
    {
        [SerializeField]
        private List<string> Tags = new List<string>();
    }
}
