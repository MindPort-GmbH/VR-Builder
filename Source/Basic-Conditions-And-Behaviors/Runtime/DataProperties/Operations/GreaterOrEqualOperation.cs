using System;

namespace VRBuilder.Core.ProcessUtils
{
    /// <summary>
    /// True if left >= right.
    /// </summary>
    public class GreaterOrEqualOperation<T> : IOperationCommand<T, bool> where T : IComparable<T>
    {
        /// <inheritdoc/>
        public bool Execute(T leftOperand, T rightOperand)
        {
            return leftOperand != null && leftOperand.CompareTo(rightOperand) >= 0;
        }
    }
}