// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2022 MindPort GmbH

using System.IO;
using System.Text;
using System.Collections;
using System.Threading.Tasks;
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
        public async Task Read()
        {
            // Given an existing file in a relative path.
            Assert.IsTrue(await defaultFileSystem.Exists(RelativeFilePath));

            // When reading the file from the relative path.
            byte[] fileData = await defaultFileSystem.Read(RelativeFilePath);
            string message = Encoding.Default.GetString(fileData);

            // Then assert that the file content was retrieved.
            Assert.That(fileData != null && fileData.Length > 0);
            Assert.IsFalse(string.IsNullOrEmpty(message));
        }

        [UnityTest]
        public async Task Exists()
        {
            // Given a file in a relative path.
            // When checking if the file exits.
            // Then assert that the file exits.
            Assert.IsFalse(string.IsNullOrEmpty(RelativeFilePath));
            Assert.IsFalse(Path.IsPathRooted(RelativeFilePath));
            Assert.IsTrue(await defaultFileSystem.Exists(RelativeFilePath));
        }

        [UnityTest]
        public async Task Write()
        {
            Assert.IsFalse(await FileManager.Exists(RelativeFilePath));

            // Given invalid paths and files data.
            byte[] fileData = new UTF8Encoding(true).GetBytes(FileContent);

            // When trying to read, write or check if the file exits using invalid arguments.
            await defaultFileSystem.Write(RelativeFilePath, fileData);

            // Then assert that a proper exception is thrown.
            Assert.IsTrue(await defaultFileSystem.Exists(RelativeFilePath));
        }

        [UnityTest]
        public async Task NotExistingFile()
        {
            // Given a relative path to a file that does not exit.
            // When checking if the file exits.
            // Then assert that the file does not exit.
            Assert.IsFalse(await defaultFileSystem.Exists(NonExistingFilePath));
        }
    }
}