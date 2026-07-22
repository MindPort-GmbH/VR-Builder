// Copyright (c) 2021 MindPort GmbH
// Licensed under the Apache License, Version 2.0

using UnityEditor;
using VRBuilder.Core.Configuration;
using VRBuilder.Core.Editor.Configuration;
using VRBuilder.Core.Editor.ProcessAssets;
using VRBuilder.Core.Runtime.Registry;

namespace VRBuilder.Core.Editor.Menu
{
    internal static class OpenProcessMenuEntry
    {
        /// <summary>
        /// Open the Workflow Editor window.
        /// </summary>
        [MenuItem("Tools/VR Builder/Process Editor...", false, 15)]
        [MenuItem("Window/VR Builder/Process Editor", false, 100)]
        private static void OpenWorkflowEditor()
        {
            GlobalEditorHandler.SetCurrentProcess(ProcessAssetUtils.GetProcessNameFromPath(ServiceRegistry.Get<RuntimeService>().SelectedProcess));
            GlobalEditorHandler.StartEditingProcess();
        }

        [MenuItem("Tools/VR Builder/Process Editor...", true, 2)]
        [MenuItem("Window/VR Builder/Process Editor", true, 100)]
        private static bool ValidateOpenWorkflowEditor()
        {
            if (ServiceRegistry.Has<RuntimeService>() == false)
            {
                return false;
            }

            // if (RuntimeConfiguratorEditor.IsProcessListEmpty())
            // {
            //     return false;
            // }

            return true;
        }
    }
}
