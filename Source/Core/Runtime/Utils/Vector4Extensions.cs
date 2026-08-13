// Copyright (c) 2026 Aron Schaub
// SPDX-License-Identifier: Apache-2.0

using UnityEngine;
using VRBuilder.Core.Primitives;
using VRBuilder.Core.Properties;

namespace VRBuilder.Core.Runtime.Utils
{
    public static class Vector4Extensions
    {
        public static IVector4 ToVector4Data(this Vector4 vector) => new Vector4Data(vector.x, vector.y, vector.z, vector.w);
        public static Vector4 ToUnity(this IVector4 vector) => new(vector.X, vector.Y, vector.Z, vector.W);
    }
}
