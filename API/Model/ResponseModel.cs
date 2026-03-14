namespace API.Model
{
    public class ResponseModel
    {
        public ResponseStatus Status { get; set; }
        public string Message { get; set; } = string.Empty;
        public object? Data { get; set; }
    }
    public enum ResponseStatus
    {
        Success, Failure 
    }
}
