// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2022 MindPort GmbH

using System;
using System.Text;
using System.Collections;
using VRBuilder.Core.IO;
using UnityEngine;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace VRBuilder.Tests.IO
{
    public class FileManagerTests : IOTests
    {
        [UnityTest]
        public IEnumerator Read()
        {
            // Given an existing file in a relative path.
            Assert.IsTrue(FileManager.Exists(RelativeFilePath));

            // When reading the file from the relative path.
            byte[] fileData = FileManager.Read(RelativeFilePath);
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
            Assert.IsTrue(FileManager.Exists(RelativeFilePath));
            yield break;
        }

        [UnityTest]
        public IEnumerator Write()
        {
            Assert.IsFalse(FileManager.Exists(RelativeFilePath));

            // Given
            byte[] fileData = new UTF8Encoding(true).GetBytes(FileContent);

            // When
            FileManager.Write(RelativeFilePath, fileData);

            // Then
            Assert.IsTrue(FileManager.Exists(RelativeFilePath));
            yield break;
        }

        [UnityTest]
        public IEnumerator ArgumentException()
        {
            // Given invalid paths and files data.
            string nullPath = null;
            string empPath = string.Empty;
            string absolutePath = Application.dataPath;
            byte[] nullData = null;
            byte[] fileData = FileManager.Read(RelativeFilePath);

            // When trying to read, write or check if the file exits using invalid arguments.

            // Then assert that a proper exception is thrown.
            Assert.Throws<ArgumentException>(()=> FileManager.Read(nullPath));
            Assert.Throws<ArgumentException>(()=> FileManager.Read(empPath));
            Assert.Throws<ArgumentException>(()=> FileManager.Read(absolutePath));

            Assert.Throws<ArgumentException>(()=> FileManager.Write(nullPath, fileData));
            Assert.Throws<ArgumentException>(()=> FileManager.Write(empPath, fileData));
            Assert.Throws<ArgumentException>(()=> FileManager.Write(absolutePath, fileData));
            Assert.Throws<ArgumentException>(()=> FileManager.Write(RelativeFilePath, nullData));

            Assert.Throws<ArgumentException>(()=> FileManager.Exists(nullPath));
            Assert.Throws<ArgumentException>(()=> FileManager.Exists(empPath));
            Assert.Throws<ArgumentException>(()=> FileManager.Exists(absolutePath));
            yield break;
        }

        [UnityTest]
        public IEnumerator NotExistingFile()
        {
            // Given a relative path to a file that does not exit.
            // When checking if the file exits.
            // Then assert that the file does not exit.
            Assert.IsFalse(FileManager.Exists(NonExistingFilePath));
            yield break;
        }
    }
}
