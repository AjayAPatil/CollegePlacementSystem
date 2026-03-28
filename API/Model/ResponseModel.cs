namespace API.Model
{
    public class ResponseModel
    {
        public ResponseStatus Status { get; set; }
        public string Message { get; set; } = string.Empty;
        public object? Data { get; set; }
    }
    public class PagedResult<T>
    {
        public IReadOnlyList<T> Items { get; set; } = Array.Empty<T>();
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public bool HasMore { get; set; }
    }

    public enum ResponseStatus
    {
        Success, Failure 
    }
}
