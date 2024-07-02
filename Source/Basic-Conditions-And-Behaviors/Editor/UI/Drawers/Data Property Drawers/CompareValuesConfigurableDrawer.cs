using System;
using UnityEngine;
using VRBuilder.Core.Conditions;
using VRBuilder.Editor.UI.Drawers;

namespace VRBuilder.Editor.Core.UI.Drawers
{
    /// <summary>
    /// Implementation of <see cref="CompareValuesDrawer{T}"/> for comparing floats.
    /// </summary>
    [DefaultProcessDrawer(typeof(CompareValuesCondition<bool>.EntityData))]
    [DefaultProcessDrawer(typeof(CompareValuesCondition<float>.EntityData))]
    [DefaultProcessDrawer(typeof(CompareValuesCondition<string>.EntityData))]
    internal class CompareValuesConfigurableDrawer : AbstractDrawer
    {
        private const bool useFastDrawers = false;
        private ObjectDrawer compareBoolDrawer;
        private ObjectDrawer compareFloatDrawer;
        private ObjectDrawer compareStringDrawer;

        public CompareValuesConfigurableDrawer()
        {
            if (useFastDrawers)
            {
                /// TODO implement
            }
            else
            {
                compareBoolDrawer = new CompareBooleansDrawer();
                compareFloatDrawer = new CompareNumbersDrawer();
                compareStringDrawer = new CompareTextDrawer();
            }
        }

        public override Rect Draw(Rect rect, object currentValue, Action<object> changeValueCallback, GUIContent label)
        {
            if (currentValue is CompareValuesCondition<bool>.EntityData)
            {
                return compareBoolDrawer.Draw(rect, currentValue, changeValueCallback, label);
            }

            if (currentValue is CompareValuesCondition<float>.EntityData)
            {
                return compareFloatDrawer.Draw(rect, currentValue, changeValueCallback, label);
            }

            if (currentValue is CompareValuesCondition<string>.EntityData)
            {
                return compareStringDrawer.Draw(rect, currentValue, changeValueCallback, label);
            }

            return rect;
        }
    }
}
