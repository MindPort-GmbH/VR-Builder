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
        public IEnumerator Read()
        {
            yield return null;

            // Given an existing file in a relative path.
            Task<bool> exists = Task<bool>.Run(() => defaultFileSystem.Exists(RelativeFilePath));
            exists.Wait();
            Assert.IsTrue(exists.IsCompleted);
            Assert.IsTrue(exists.Result);

            // When reading the file from the relative path.
            Task<byte[]> getFileData = Task<byte[]>.Run(() => defaultFileSystem.Read(RelativeFilePath));
            getFileData.Wait();                        
            string message = Encoding.Default.GetString(getFileData.Result);

            // Then assert that the file content was retrieved.
            Assert.IsTrue(getFileData.IsCompleted);
            Assert.That(getFileData.Result != null && getFileData.Result.Length > 0);
            Assert.IsFalse(string.IsNullOrEmpty(message));
        }

        [UnityTest]
        public IEnumerator Exists()
        {
            yield return null;

            Task<bool> exists = Task<bool>.Run(() => defaultFileSystem.Exists(RelativeFilePath));
            exists.Wait();                       
            
            // Given a file in a relative path.
            // When checking if the file exits.
            // Then assert that the file exits.
            Assert.IsFalse(string.IsNullOrEmpty(RelativeFilePath));
            Assert.IsFalse(Path.IsPathRooted(RelativeFilePath));
            Assert.IsTrue(exists.IsCompleted);
            Assert.IsTrue(exists.Result);
        }

        [UnityTest]
        public IEnumerator Write()
        {
            yield return null;

            Task<bool> exists = Task<bool>.Run(() => defaultFileSystem.Exists(RelativeFilePath));
            exists.Wait();
            Assert.IsTrue(exists.IsCompleted);
            Assert.IsFalse(exists.Result);

            // Given invalid paths and files data.
            byte[] fileData = new UTF8Encoding(true).GetBytes(FileContent);

            // When trying to read, write or check if the file exits using invalid arguments.
            Task tryWrite = Task.Run(() => defaultFileSystem.Write(RelativeFilePath, fileData));
            tryWrite.Wait();

            // Then assert that a proper exception is thrown.
            Assert.IsTrue(tryWrite.IsCompleted);

            Task<bool> existsAfterWriting = Task<bool>.Run(() => defaultFileSystem.Exists(RelativeFilePath));
            exists.Wait();

            Assert.IsTrue(existsAfterWriting.Result);
        }

        [UnityTest]
        public IEnumerator NotExistingFile()
        {
            yield return null;

            // Given a relative path to a file that does not exist.
            // When checking if the file exists.
            Task<bool> exists = Task<bool>.Run(() => defaultFileSystem.Exists(NonExistingFilePath));
            exists.Wait();
            // Then assert that the file does not exist.
            Assert.IsTrue(exists!.IsCompleted);
            Assert.IsFalse(exists.Result);
        }
    }
}