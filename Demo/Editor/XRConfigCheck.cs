using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace VRBuilder.Editor.DemoScene
{
    [ExecuteInEditMode]
    public class XRConfigCheck : MonoBehaviour
    {
        private const string coreEditorAssembly = "VRBuilder.Editor";
        private const string packageOperationsManagerClass = "VRBuilder.Editor.PackageManager.PackageOperationsManager";
        private const string loadPackageMethod = "LoadPackage";
        private const string XRManagementPlugin = "com.unity.xr.management@4.0.1";

        private void Awake()
        {
#if UNITY_EDITOR && !UNITY_XR_MANAGEMENT
            if(AppDomain.CurrentDomain.GetAssemblies().Any(assembly => assembly.GetName().Name == "VRBuilder.Editor"))
            {
                if (EditorUtility.DisplayDialog("Demo Scene Config", "The Unity XR Management plugin is not installed. It's recommended you configure your VR headset before trying the demo scene. Install now?", "Yes", "No"))
                {
                    Assembly vpgEditor = AppDomain.CurrentDomain.GetAssemblies().First(assembly => assembly.GetName().Name == coreEditorAssembly);
                    Type packageManager = vpgEditor.GetType(packageOperationsManagerClass);
                    MethodInfo loadPackage = packageManager.GetMethod(loadPackageMethod);
                    loadPackage.Invoke(null, new object[] { XRManagementPlugin, null });
                }
            }
#endif
        }
    }
}