// Copyright (c) 2026 Aron Schaub
// SPDX-License-Identifier: Apache-2.0

using UnityEngine;
using VRBuilder.Core.Primitives;

namespace Core.Runtime.Utils
{
    /// <summary>
    /// this provides extension methods for Color to convert from CoreRuntime ColorData to Unity and back.
    /// </summary>
    public static class ColorExtensions
    {
        public static ColorData ToColorData(this Color color) => new(color.r, color.g, color.b, color.a);
        public static Color ToUnity(this IColor color) => new(color.R, color.G, color.B, color.A);
    }
}