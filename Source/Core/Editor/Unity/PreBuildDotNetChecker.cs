//// Copyright (c) 2013-2019 Innoactive GmbH
//// Licensed under the Apache License, Version 2.0
//// Modifications copyright (c) 2021-2025 MindPort GmbH

//using UnityEditor;
//using UnityEditor.Build;
//using UnityEditor.Build.Reporting;
//using UnityEngine;

//namespace VRBuilder.Core.Editor.Unity
//{
//    /// <summary>
//    /// Pre-process validation to identify if the 'API Compatibility Level' is set to '.Net 4.x'.
//    /// </summary>
//    internal class PreBuildDotNetChecker : IPreprocessBuildWithReport, IPostprocessBuildWithReport
//    {
//        ///<inheritdoc />
//        public int callbackOrder => 0;

//        ///<inheritdoc />
//        public void OnPreprocessBuild(BuildReport report)
//        {
//            if (EditorUtils.GetCurrentCompatibilityLevel() != ApiCompatibilityLevel.NET_Unity_4_8)
//            {
//                DotNetWindow dotnet = ScriptableObject.CreateInstance<DotNetWindow>();
//                dotnet.ShowModalUtility();

//                if (dotnet.ShouldAbortBuilding())
//                {
//                    throw new BuildFailedException("The build was aborted.");
//                }
//            }
//        }

//        ///<inheritdoc />
//        public void OnPostprocessBuild(BuildReport report)
//        {
//            if (EditorUtils.GetCurrentCompatibilityLevel() != ApiCompatibilityLevel.NET_Unity_4_8)
//            {
//                UnityEngine.Debug.LogError("This Unity project uses {currentLevel} but some VR Builder features require .NET 4.X support.\nThe built application might not work as expected."
//                               + "\nIn order to prevent this, go to Edit > Project Settings > Player Settings > Other Settings and set the Api Compatibility Level to .NET 4.X.");
//            }
//        }
//    }
//}
