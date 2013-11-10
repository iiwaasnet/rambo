using rambo.Interfaces;

namespace rambo.Implementation
{
    internal class OperationRequest
    {
        internal OperationType OpType { get; set; }
        internal IObjectValue Value { get; set; }
        internal IObjectId Id { get; set; }
    }
}