using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.TestTools;
using VRBuilder.Core.Settings;
using VRBuilder.Tests.Utils;

namespace VRBuilder.Tests
{
    public class SceneObjectTagTests : RuntimeTests
    {
        private List<Guid> tags = new List<Guid>();
        private string[] tagLabels = { "delete this test tag immediately", "testTagHHH", "adding another test tag for good measure" };

        [SetUp]
        public void CreateTestTags()
        {
            foreach (string label in tagLabels)
            {
                tags.Add(SceneObjectTags.Instance.CreateTag(label, Guid.NewGuid()).Guid);
            }
        }

        [TearDown]
        public void RemoveTestTags()
        {
            foreach (Guid guid in tags)
            {
                SceneObjectTags.Instance.RemoveTag(guid);
            }
        }

        [UnityTest]
        public IEnumerator TagIsAdded()
        {
            // When a tag is added to the scene object registry,
            Guid newTagGuid = Guid.NewGuid();
            tags.Add(newTagGuid);
            SceneObjectTags.Instance.CreateTag("a test tag that has been added in a test, delete it if you see it", newTagGuid);

            // Then it can be found in the registry
            Assert.True(SceneObjectTags.Instance.TagExists(newTagGuid));

            yield return null;
        }

        [UnityTest]
        public IEnumerator NoNameTagIsNotAdded()
        {
            // When a no name tag is added to the scene object registry,
            Guid newTagGuid = Guid.NewGuid();
            tags.Add(newTagGuid);
            SceneObjectTags.Tag tag = SceneObjectTags.Instance.CreateTag("", newTagGuid);

            // Then it is not added.
            Assert.False(SceneObjectTags.Instance.TagExists(newTagGuid));
            Assert.IsNull(tag);

            yield return null;
        }

        [UnityTest]
        public IEnumerator DuplicateNameTagIsRenamed()
        {
            // When a duplicate name tag is added to the scene object registry,
            Guid newTagGuid = Guid.NewGuid();
            tags.Add(newTagGuid);
            SceneObjectTags.Tag tag = SceneObjectTags.Instance.CreateTag("testTagHHH", newTagGuid);

            // Then it is renamed.
            Assert.True(SceneObjectTags.Instance.TagExists(newTagGuid));
            Assert.AreEqual(tag.Label, "testTagHHH_1");

            yield return null;
        }

        [UnityTest]
        public IEnumerator TagIsDeleted()
        {
            // Given a tag in the registry,
            Guid newTagGuid = Guid.NewGuid();
            tags.Add(newTagGuid);
            SceneObjectTags.Instance.CreateTag("a test tag that has been added in a test, delete it if you see it", newTagGuid);
            bool wasInRegistry = SceneObjectTags.Instance.TagExists(newTagGuid);

            // When it is removed,
            SceneObjectTags.Instance.RemoveTag(newTagGuid);

            // Then it cannot be found anymore.
            Assert.True(wasInRegistry);
            Assert.False(SceneObjectTags.Instance.TagExists(newTagGuid));

            yield return null;
        }
    }
}