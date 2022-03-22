namespace VRBuilder.Core.ProcessUtils
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
    }
}