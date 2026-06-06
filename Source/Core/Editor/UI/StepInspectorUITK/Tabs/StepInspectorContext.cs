using System;

namespace VRBuilder.Core.Editor.UI.StepInspectorUITK.Tabs
{
    internal sealed class StepInspectorContext : IElementDrawerContext
    {
        private readonly object stepDataOwner;
        private readonly Action<object> changeCallback;

        private StepInspectorContext(
            object stepDataOwner,
            Action<object> changeCallback)
        {
            this.stepDataOwner = stepDataOwner;
            this.changeCallback = changeCallback;
        }

        public void NotifyStepModified()
        {
            changeCallback?.Invoke(stepDataOwner);
        }

        /// <summary>
        /// Builds a context around a <see cref="Step.EntityData"/> being drawn.
        /// </summary>
        public static IElementDrawerContext For(Step.EntityData stepData, Action<object> changeCallback)
        {
            return new StepInspectorContext(
                stepDataOwner: stepData,
                changeCallback: changeCallback);
        }
    }
}
