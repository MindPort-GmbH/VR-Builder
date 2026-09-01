// Copyright (c) 2026 Aron Schaub
// SPDX-License-Identifier: Apache-2.0

using System.Linq;
using UnityEngine;
using VRBuilder.Core.Primitives;

namespace VRBuilder.Core.Runtime.Utils
{
    public static class AnimationCurveExtensions
    {
        public static IAnimationCurve ToAnimationCurveData(this AnimationCurve curve)
        {
            return new AnimationCurveData(
                curve.keys.Select(key => new KeyframeData(key.time, key.value, key.inTangent, key.outTangent, (int)key.weightedMode, key.inWeight, key.outWeight)).ToArray(),
                (int)curve.preWrapMode,
                (int)curve.postWrapMode
            );
        }

        public static AnimationCurve ToUnity(this IAnimationCurve curve)
        {
            var c = new AnimationCurve(curve.Keyframes.Select(keyframe => new Keyframe(keyframe.Time, keyframe.Value, keyframe.InTangent, keyframe.OutTangent, keyframe.InWeight, keyframe.OutWeight)).ToArray())
            {
                preWrapMode = (WrapMode)curve.PreWrapMode,
                postWrapMode = (WrapMode)curve.PostWrapMode
            };
            return c;
        }
    }
}
