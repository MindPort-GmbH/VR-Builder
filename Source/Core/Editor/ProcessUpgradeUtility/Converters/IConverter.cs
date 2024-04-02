using System;

namespace VRBuilder.Editor.ProcessUpgradeTool
{
    public interface IConverter
    {
        Type ConvertedType { get; }

        object Convert(object oldEntity);
    }
}
