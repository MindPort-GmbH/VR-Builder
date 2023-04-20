// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2023 MindPort GmbH

using System;
using System.Text;
using System.Collections;
using System.Threading.Tasks;
using VRBuilder.Core.IO;
using UnityEngine;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace VRBuilder.Tests.IO
{
    public class FileManagerTests : IOTests
    {
        public static IEnumerator Execute(Task task)
        {
            while (!task.IsCompleted)
            {
                yield return null;
            }

            if (task.IsFaulted)
            {
                throw task.Exception;
            }
        }

        [UnityTest]
        public IEnumerator Read()
        {
            yield return Execute(ReadAsync());
        }

        public async Task ReadAsync()
        {
            // Given an existing file in a relative path.
            Assert.IsTrue(await FileManager.Exists(RelativeFilePath));

            // When reading the file from the relative path.
            byte[] fileData = await FileManager.Read(RelativeFilePath);
            string message = Encoding.Default.GetString(fileData);

            // Then assert that the file content was retrieved.
            Assert.That(fileData != null && fileData.Length > 0);
            Assert.IsFalse(string.IsNullOrEmpty(message));
        }

        [UnityTest]
        public IEnumerator Exists()
        {
            yield return Execute(ExistsAsync());
        }

        public async Task ExistsAsync()
        {
            // Given a file in a relative path.
            // When checking if the file exits.
            // Then assert that the file exits.
            Assert.IsTrue(await FileManager.Exists(RelativeFilePath));
        }

        [UnityTest]
        public IEnumerator Write()
        {
            yield return Execute(WriteAsync());
        }

        public async Task WriteAsync()
        {
            Assert.IsFalse(await FileManager.Exists(RelativeFilePath));

            // Given
            byte[] fileData = new UTF8Encoding(true).GetBytes(FileContent);

            // When
            await FileManager.Write(RelativeFilePath, fileData);

            // Then
            Assert.IsTrue(await FileManager.Exists(RelativeFilePath));
        }

        [UnityTest]
        public IEnumerator ArgumentException()
        {
            yield return null;
            // Given invalid paths and files data.
            string nullPath = null;
            string empPath = string.Empty;
            string absolutePath = Application.dataPath;
            byte[] nullData = null;
            Task<byte[]> fileDataTask = Task.Run(() => FileManager.Read(RelativeFilePath));
            fileDataTask.Wait();
            byte[] fileData = fileDataTask.Result;

            // When trying to read, write or check if the file exits using invalid arguments.

            // Then assert that a proper exception is thrown.
            Task ReadNullPath() => FileManager.Read(nullPath);            
            Task ReadEmptyPath() => FileManager.Read(empPath);
            Task ReadAbsolutePath() => FileManager.Read(absolutePath);
            Task WriteFileData() => FileManager.Write(nullPath, fileData);
            Task WriteEmptyFileData() => FileManager.Write(empPath, fileData);
            Task WriteAbsoluteFileData() => FileManager.Write(absolutePath, fileData);
            Task WriteRelativeNullData() => FileManager.Write(RelativeFilePath, nullData);
            Task ExistNullData() => FileManager.Exists(nullPath);
            Task ExistEmptyPath() => FileManager.Exists(empPath);
            Task ExistAbsolutePath() => FileManager.Exists(absolutePath);

            Assert.That(ReadNullPath().Exception.InnerException.GetType() == typeof(ArgumentException));
            Assert.That(ReadEmptyPath().Exception.InnerException.GetType() == typeof(ArgumentException));
            Assert.That(ReadAbsolutePath().Exception.InnerException.GetType() == typeof(ArgumentException));
            Assert.That(WriteFileData().Exception.InnerException.GetType() == typeof(ArgumentException));
            Assert.That(WriteEmptyFileData().Exception.InnerException.GetType() == typeof(ArgumentException));
            Assert.That(WriteAbsoluteFileData().Exception.InnerException.GetType() == typeof(ArgumentException));
            Assert.That(WriteRelativeNullData().Exception.InnerException.GetType() == typeof(ArgumentException));
            Assert.That(ExistNullData().Exception.InnerException.GetType() == typeof(ArgumentException));
            Assert.That(ExistEmptyPath().Exception.InnerException.GetType() == typeof(ArgumentException));
            Assert.That(ExistAbsolutePath().Exception.InnerException.GetType() == typeof(ArgumentException));
        }

        [UnityTest]
        public IEnumerator NotExistingFile()
        {
            yield return Execute(NotExistingFileAsync());
        }

        public async Task NotExistingFileAsync()
        {
            // Given a relative path to a file that does not exit.
            // When checking if the file exits.
            // Then assert that the file does not exit.
            Assert.IsFalse(await FileManager.Exists(NonExistingFilePath));
        }
    }
}