using System;

namespace VRBuilder.Core.Properties.Operations
{
    /// <summary>
    /// "Or" boolean operation.
    /// </summary>
    public class OrOperation : IOperationCommand<bool, bool>
    {
        /// <inheritdoc/>
        public bool Execute(bool leftOperand, bool rightOperand)
        {
            return leftOperand || rightOperand;
        }

        public string ToString(string format, IFormatProvider formatProvider)
        {
            return "||";
        }
    }
}
