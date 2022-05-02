using Newtonsoft.Json;

namespace Pwa.Utility
{
    public class ErrorDetails
    {
        public int StatusCode { get; set; }
        public Object? Message { get; set; }
        public override string ToString() => JsonConvert.SerializeObject(this);
    }

    public class TransactionErrorDto
    {
        public string? Title { get; set; } = "One or more validation errors occured";
        public List<TransactionSubError>? Details { get; set; }
        public override string ToString() => JsonConvert.SerializeObject(this);
    }

    public class TransactionSubError
    {
        public string? Position { get; set; }
        public string? Header { get; set; }
        public string? Value { get; set; }
        public string? Error { get; set; }

        public override string ToString() => JsonConvert.SerializeObject(this);
    }
}
