// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2024 MindPort GmbH

using System.IO;
using System.Text;
using UnityEngine;
using NUnit.Framework;

namespace VRBuilder.Tests.IO
{
    public class IOTests
    {
        protected const string FolderName = "TestFolder";
        protected const string FileName = "TestFile.txt";
        protected const string FileContent = "Hello Test!";
        protected const string NonExistingFilePath = "I do not exist.sad";

        protected string RelativeFilePath;
        protected string AbsoluteStreamingAssetsFilePath;
        protected string AbsolutePersistentDataFilePath;

        [OneTimeSetUp]
        public virtual void OneTimeSetUp()
        {
            RelativeFilePath = Path.Combine(FolderName, FileName);
            AbsoluteStreamingAssetsFilePath = CreateDirectory(Application.streamingAssetsPath);
            AbsolutePersistentDataFilePath = CreateDirectory(Application.persistentDataPath);
        }

        [SetUp]
        public virtual void SetUp()
        {
            if (TestContext.CurrentContext.Test.MethodName.Contains("Write"))
            {
                return;
            }

            CreateFile(AbsoluteStreamingAssetsFilePath);
            CreateFile(AbsolutePersistentDataFilePath);
        }

        [TearDown]
        public virtual void TearDown()
        {
            DeleteFile(AbsoluteStreamingAssetsFilePath);
            DeleteFile(AbsolutePersistentDataFilePath);
        }

        [OneTimeTearDown]
        public virtual void OneTimeTearDown()
        {
            DeleteDirectory(Application.streamingAssetsPath);
            DeleteDirectory(Application.persistentDataPath);
        }

        private void CreateFile(string cacheLocation)
        {
            using (FileStream fileStream = File.Create(cacheLocation))
            {
                byte[] fileData = new UTF8Encoding(true).GetBytes(FileContent);
                fileStream.Write(fileData, 0, fileData.Length);
            }
        }

        private void DeleteFile(string absoluteFilePath)
        {
            if (File.Exists(absoluteFilePath))
            {
                File.Delete(absoluteFilePath);
            }
        }

        private string CreateDirectory(string cacheLocation)
        {
            string fileDirectory = Path.Combine(cacheLocation, FolderName);

            if (Directory.Exists(fileDirectory) == false)
            {
                Directory.CreateDirectory(fileDirectory);
            }

            return Path.Combine(fileDirectory, FileName);
        }

        private void DeleteDirectory(string cacheLocation)
        {
            string fileDirectory = Path.Combine(cacheLocation, FolderName);

            if (Directory.Exists(fileDirectory))
            {
                Directory.Delete(fileDirectory);
            }
        }
    }
}
