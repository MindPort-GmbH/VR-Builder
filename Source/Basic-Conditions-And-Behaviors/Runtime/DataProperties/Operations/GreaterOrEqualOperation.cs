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

        /// <summary>
        /// Constructs concrete types in order for them to be seen by IL2CPP's ahead of time compilation.
        /// </summary>
        private class AOTHelper
        {
            EqualToOperation<float> flt = new EqualToOperation<float>();
            EqualToOperation<string> str = new EqualToOperation<string>();
            EqualToOperation<bool> bln = new EqualToOperation<bool>();
        }
    }
}