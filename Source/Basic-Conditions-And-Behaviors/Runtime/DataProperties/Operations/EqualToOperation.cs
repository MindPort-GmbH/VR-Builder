using System;

namespace VRBuilder.Core.ProcessUtils
{
    /// <summary>
    /// True if left and right are equal.
    /// </summary>
    public class EqualToOperation<T> : IOperationCommand<T, bool> where T : IEquatable<T>
    {
        /// <inheritdoc/>
        public bool Execute(T leftOperand, T rightOperand)
        {
            return leftOperand != null && leftOperand.Equals(rightOperand);
        }
    }
}