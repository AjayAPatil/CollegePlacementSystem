namespace API.Constants
{
    public static class JobStatus
    {
        public const string Draft = "draft";
        public const string Published = "published";
        public const string Closed = "closed";
        public const string Expired = "expired";

        public static readonly string[] AllowedStatuses =
        [
            Draft,
            Published,
            Closed,
            Expired
        ];
    }
}
