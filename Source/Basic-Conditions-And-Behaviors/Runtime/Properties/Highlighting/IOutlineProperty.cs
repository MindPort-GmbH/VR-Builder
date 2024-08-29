using System;
using UnityEngine;
using UnityEngine.Events;

namespace VRBuilder.Core.Properties
{
    /// <summary>
    /// Interface for scene objects that can be highlighted with an outline.
    /// </summary>
    public interface IOutlineProperty : ISceneObjectProperty
    {
        /// <summary>
        /// Emitted when the object gets outlined.
        /// </summary>
        UnityEvent<OutlinePropertyEventArgs> OutlineStarted { get; }

        /// <summary>
        /// Emitted when the object gets unoutlined.
        /// </summary>
        UnityEvent<OutlinePropertyEventArgs> OutlineEnded { get; }

        /// <summary>
        /// Is object currently outlined.
        /// </summary>
        bool IsOutlined { get; }

        /// <summary>
        /// Outline this object and use <paramref name="outlineColor"/>.
        /// </summary>
        /// <param name="outlineColor">Color to use for highlighting.</param>
        void ShowOutline(Color outlineColor);

        /// <summary>
        /// Disable outline.
        /// </summary>
        void HideOutline();
    }

    /// <summary>
    /// Event args for <see cref="IOutlineProperty"/>.
    /// </summary>
    public class OutlinePropertyEventArgs : EventArgs
    {
        public readonly Color? OutlineColor;

        public OutlinePropertyEventArgs(Color? outlineColor)
        {
            OutlineColor = outlineColor;
        }
    }
}
