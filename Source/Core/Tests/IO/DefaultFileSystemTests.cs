// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021 MindPort GmbH

using System.IO;
using System.Text;
using System.Collections;
using VRBuilder.Core.IO;
using UnityEngine;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace VRBuilder.Tests.IO
{
    public class DefaultFileSystemTests : IOTests
    {
        private IPlatformFileSystem defaultFileSystem;

        [OneTimeSetUp]
        public override void OneTimeSetUp()
        {
            base.OneTimeSetUp();
            defaultFileSystem = new DefaultFileSystem(Application.streamingAssetsPath, Application.persistentDataPath);
        }

        [UnityTest]
        public IEnumerator Read()
        {
            // Given an existing file in a relative path.
            Assert.IsTrue(defaultFileSystem.Exists(RelativeFilePath));

            // When reading the file from the relative path.
            byte[] fileData = defaultFileSystem.Read(RelativeFilePath);
            string message = Encoding.Default.GetString(fileData);

            // Then assert that the file content was retrieved.
            Assert.That(fileData != null && fileData.Length > 0);
            Assert.IsFalse(string.IsNullOrEmpty(message));
            yield break;
        }

        [UnityTest]
        public IEnumerator Exists()
        {
            // Given a file in a relative path.
            // When checking if the file exits.
            // Then assert that the file exits.
            Assert.IsFalse(string.IsNullOrEmpty(RelativeFilePath));
            Assert.IsFalse(Path.IsPathRooted(RelativeFilePath));
            Assert.IsTrue(defaultFileSystem.Exists(RelativeFilePath));
            yield break;
        }

        [UnityTest]
        public IEnumerator Write()
        {
            Assert.IsFalse(FileManager.Exists(RelativeFilePath));

            // Given invalid paths and files data.
            byte[] fileData = new UTF8Encoding(true).GetBytes(FileContent);

            // When trying to read, write or check if the file exits using invalid arguments.
            defaultFileSystem.Write(RelativeFilePath, fileData);

            // Then assert that a proper exception is thrown.
            Assert.IsTrue(defaultFileSystem.Exists(RelativeFilePath));
            yield break;
        }

        [UnityTest]
        public IEnumerator NotExistingFile()
        {
            // Given a relative path to a file that does not exit.
            // When checking if the file exits.
            // Then assert that the file does not exit.
            Assert.IsFalse(defaultFileSystem.Exists(NonExistingFilePath));
            yield break;
        }
    }
}
