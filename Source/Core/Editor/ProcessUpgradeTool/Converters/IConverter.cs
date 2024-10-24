using System;

namespace VRBuilder.Core.Editor.ProcessUpgradeTool.Converters
{
    /// <summary>
    /// Replaces an object with another up to date object with comparable functionality.
    /// </summary>
    public interface IConverter
    {
        /// <summary>
        /// Type supported by this converter.
        /// </summary>
        Type ConvertedType { get; }

        /// <summary>
        /// Returns an object which is an up to date version of the provided object.
        /// </summary>
        object Convert(object oldEntity);
    }
}
