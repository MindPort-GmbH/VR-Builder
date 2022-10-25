using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VRBuilder.Core.Runtime.Utils;

namespace VRBuilder.Editor.Settings
{
    public class SceneObjectTags : SettingsObject<SceneObjectTags>
    {
        [SerializeField]
        private List<string> tags = new List<string>();

        public IEnumerable<string> Tags => tags.Where(t =>  string.IsNullOrEmpty(t) == false);
    }
}
