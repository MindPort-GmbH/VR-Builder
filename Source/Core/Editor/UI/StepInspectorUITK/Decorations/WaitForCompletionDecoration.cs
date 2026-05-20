using System;
using UnityEngine.UIElements;
using VRBuilder.Core.Behaviors;
using VRBuilder.Core.Editor.UndoRedo;

namespace VRBuilder.Core.Editor.UI.StepInspectorUITK.Decorations
{
    /// <summary>
    /// Appends the "Wait for completion" toggle to any behavior whose <c>Data</c> implements
    /// <see cref="IBackgroundBehaviorData"/>. Replaces the legacy <c>[DrawIsBlockingToggle]</c>
    /// metadata that the IMGUI inspector attached at the behavior-list level.
    /// </summary>
    public sealed class WaitForCompletionDecoration : IEntityDecoration
    {
        public int Order => 100;

        public bool AppliesTo(object entity)
        {
            return entity is IDataOwner owner && owner.Data is IBackgroundBehaviorData;
        }

        public VisualElement Build(object entity, Action onChanged)
        {
            if (entity is not IDataOwner owner || owner.Data is not IBackgroundBehaviorData data)
            {
                return null;
            }

            Toggle toggle = new Toggle("Wait for completion")
            {
                value = data.IsBlocking,
                tooltip = "When checked, the step waits for this behavior to finish before moving on."
            };
            toggle.AddToClassList("vrb-decoration");
            toggle.AddToClassList("vrb-decoration--wait-for-completion");

            toggle.RegisterCallback<ChangeEvent<bool>>(evt =>
            {
                bool newValue = evt.newValue;
                bool oldValue = data.IsBlocking;
                if (newValue == oldValue)
                {
                    return;
                }

                RevertableChangesHandler.Do(new ProcessCommand(
                    () => { data.IsBlocking = newValue; onChanged?.Invoke(); },
                    () => { data.IsBlocking = oldValue; onChanged?.Invoke(); }));
            });

            return toggle;
        }
    }
}
