using System;

namespace VRBuilder.Core.ProcessUtils
{
    /// <summary>
    /// True if left and right are not equal.
    /// </summary>
    public class NotEqualToOperation<T> : IOperationCommand<T, bool> where T : IEquatable<T>
    {
        /// <inheritdoc/>
        public bool Execute(T leftOperand, T rightOperand)
        {
            return leftOperand != null && leftOperand.Equals(rightOperand) == false;
        }
    }
}