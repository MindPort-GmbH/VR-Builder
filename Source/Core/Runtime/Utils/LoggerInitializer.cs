// Copyright (c) 2026 Aron Schaub
// SPDX-License-Identifier: Apache-2.0

using UnityEngine;
using VRBuilder.Core;

namespace Source.Core.Runtime.Utils
{
    /// <summary>
    /// This file is used to initialize the ForwardingLogger without it needing UnityEngine reference.
    /// </summary>
    internal static class LoggerInitializer
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            ForwardingLogger.LogAction = Debug.Log;
            ForwardingLogger.LogWarningAction = Debug.LogWarning;
            ForwardingLogger.LogErrorAction = Debug.LogError;
            ForwardingLogger.LogFormatAction = Debug.LogFormat;
            ForwardingLogger.LogWarningFormatAction = Debug.LogWarningFormat;
            ForwardingLogger.LogErrorFormatAction = Debug.LogErrorFormat;
            ForwardingLogger.LogExceptionAction = Debug.LogException;
            ForwardingLogger.LogAssertionAction = msg => Debug.LogAssertion(msg);
            ForwardingLogger.LogAssertionFormatAction = (format, objects) => Debug.LogAssertionFormat(format, objects);
        }
    }
}