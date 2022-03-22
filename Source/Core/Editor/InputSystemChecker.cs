// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2022 MindPort GmbH

using System;
using VRBuilder.Core.Input;
using UnityEditor;

[InitializeOnLoad]
public static class InputSystemChecker
{
    private const string message =
        "VR Builder requires Unity's new Input System." +
        "\n\nTo switch from the legacy input system to the new one, open the 'Player Settings' and set the " +
        "option 'Active Input Handling' to 'Both' or 'Input System Package (New)'.";

    /// <summary>
    /// This is a check if the new input system is active OR another concrete implementation of the InputController exists.
    /// </summary>
    static InputSystemChecker()
    {
        try
        {
            // This will throw an InvalidOperationException when no concrete implementation is found.
            Type type = InputController.ConcreteType;
        }
        catch (InvalidOperationException)
        {
            if (BuilderProjectSettings.Load().IsFirstTimeStarted == false)
            {
                EditorUtility.DisplayDialog("Attention required!", message, "Understood");
            }
        }
    }
}
