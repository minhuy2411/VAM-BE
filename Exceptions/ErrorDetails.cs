namespace VAM.Exceptions
{
    public class ErrorDetails
    {
        public string Message { get; }
        public int StatusCode { get; }
        public string Code { get; }

        public ErrorDetails(string message, int statusCode, string code)
        {
            Message = message;
            StatusCode = statusCode;
            Code = code;
        }
    }
}
