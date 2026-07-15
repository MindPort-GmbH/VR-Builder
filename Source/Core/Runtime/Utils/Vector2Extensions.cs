// Copyright (c) 2026 Aron Schaub
// SPDX-License-Identifier: Apache-2.0

using UnityEngine;
using VRBuilder.Core.Primitives;
using VRBuilder.Core.Properties;

namespace VRBuilder.Core.Runtime.Utils
{
    public static class Vector2Extensions
    {
        public static IVector2 ToVector2Data(this Vector2 vector) => new Vector2Data(vector.x, vector.y);
        public static Vector2 ToUnity(this IVector2 vector) => new(vector.X, vector.Y);
    }
}