using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VRBuilder.Core.Runtime.Utils;

namespace VRBuilder.Core.Settings
{
    /// <summary>
    /// Settings for global list of scene object groups.
    /// </summary>
    public class SceneObjectGroups : SettingsObject<SceneObjectGroups>
    {
        public const string UniqueGuidNameItalic = "[<i>Object ID</i>]";
        public const string UniqueGuidName = "[Object ID]";
        public const string GuidNotRegisteredText = "[<i>Missing Group</i>]";

        [Serializable]
        public class SceneObjectGroup
        {
            [SerializeField]
            private string label;

            /// <summary>
            /// Text name for this group.
            /// </summary>
            /// <remarks>
            /// We do not guarantee that this name is unique.
            /// </remarks> 
            public string Label => label;

            [SerializeField]
            private string guidString;

            private Guid guid;

            /// <summary>
            /// Unique Guid representing the group.
            /// </summary>
            public Guid Guid
            {
                get
                {
                    if (guid == null || guid == Guid.Empty)
                    {
                        guid = Guid.Parse(guidString);
                    }

                    return guid;
                }
            }

            public SceneObjectGroup(string label)
            {
                this.label = label;
                this.guidString = Guid.NewGuid().ToString();
            }

            public SceneObjectGroup(string label, Guid guid)
            {
                this.label = label;
                this.guidString = guid.ToString();
            }

            /// <summary>
            /// Renames the scene object group with the specified label.
            /// </summary>
            /// <param name="label">The new label for the scene object group.</param>
            public void Rename(string label)
            {
                this.label = label;
            }
        }

        [SerializeField, HideInInspector]
        private List<SceneObjectGroup> groups = new List<SceneObjectGroup>();

        /// <summary>
        /// All groups in the list.
        /// </summary>
        public IEnumerable<SceneObjectGroup> Groups => groups;

        /// <summary>
        /// Create a new group and add it to the list.
        /// </summary>
        public SceneObjectGroup CreateGroup(string label, Guid guid)
        {
            if (groups.Any(group => group.Guid == guid))
            {
                return null;
            }

            SceneObjectGroup group = new SceneObjectGroup(label, guid);

            if (RenameGroup(group, label))
            {
                groups.Add(group);
                return group;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// True if a group with this label can be created.
        /// </summary>
        public bool CanCreateGroup(string label)
        {
            return string.IsNullOrEmpty(label) == false &&
                Groups.Any(group => group.Label == label) == false;
        }

        /// <summary>
        /// Remove the specified group from the list.
        /// </summary>
        public bool RemoveGroup(Guid guid)
        {
            return groups.RemoveAll(group => group.Guid == guid) > 0;
        }

        /// <summary>
        /// True if the specified group is present in the list.
        /// </summary>
        public bool GroupExists(Guid guid)
        {
            return groups.Any(group => group.Guid == guid);
        }

        /// <summary>
        /// Tries to get the group associated with the specified GUID.
        /// </summary>
        /// <param name="guid">The GUID of the group to retrieve.</param>
        /// <param name="group">When this method returns, contains the group associated with the specified GUID, if found; otherwise, the default value.</param>
        /// <returns><c>true</c> if a group with the specified GUID is found; otherwise, <c>false</c>.</returns>
        public bool TryGetGroup(Guid guid, out SceneObjectGroup group)
        {
            group = groups.FirstOrDefault(group => group.Guid == guid);
            return group != null;
        }

        /// <summary>
        /// Returns the text label associated with the specified guid.
        /// </summary>
        public string GetLabel(Guid guid)
        {
            if (GroupExists(guid))
            {
                return Groups.First(group => group.Guid == guid).Label;
            }
            else
            {
                return "";
            }
        }

        /// <summary>
        /// Tries to get a group associated with the specified label. Takes the first label found.
        /// </summary>
        /// <param name="label">The label of the group to retrieve.</param>
        /// <param name="group">When this method returns, contains the first group found associated with the specified label, if found; otherwise, the default value.</param>
        /// <returns><c>true</c> if a group with the specified label is found; otherwise, <c>false</c>.</returns>
        public bool TryGetGroup(string label, out SceneObjectGroup group)
        {
            group = groups.FirstOrDefault(g => g.Label == label);
            return group != null;
        }

        /// <summary>
        /// Get a GUID associated with a specified label will. Takes the first label found.
        /// </summary>
        /// <param name="label">The label of the group to retrieve.</param>
        /// <returns>The GUID associated with the specified label. Will take the first Label found or if no group with that label exists <see cref="Guid.Empty"/>.</returns>
        public Guid GetGuid(string label)
        {
            var group = groups.FirstOrDefault(g => g.Label == label);
            return group?.Guid ?? Guid.Empty;
        }

        /// <summary>
        /// Attempts to rename a group.
        /// </summary>
        public bool RenameGroup(SceneObjectGroup group, string label)
        {
            if (string.IsNullOrEmpty(label))
            {
                return false;
            }

            int counter = 0;
            string baseLabel = label;

            while (groups.Any(group => group.Label == label))
            {
                counter++;
                label = $"{baseLabel}_{counter}";
            }

            group.Rename(label);
            return true;
        }
    }
}
