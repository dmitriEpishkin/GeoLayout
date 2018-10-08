
namespace Nordwest {
    public class OperationResult {

        public OperationResult(OperationStatus status, string message = null) {
            Status = status;
            Message = message;
        }

        public OperationStatus Status { get; }
        public string Message { get; }
    }
}
