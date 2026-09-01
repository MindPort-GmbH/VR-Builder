// Copyright (c) 2026 Aron Schaub
// SPDX-License-Identifier: Apache-2.0

using UnityEngine;
using VRBuilder.Core.Primitives;
using VRBuilder.Core.Properties;

namespace VRBuilder.Core.Runtime.Utils
{
    public static class Vector3Extensions
    {
        public static IVector3 ToVector3Data(this Vector3 vector) => new Vector3Data(vector.x, vector.y, vector.z);
        public static Vector3 ToUnity(this IVector3 vector) => new(vector.X, vector.Y, vector.Z);
    }
}