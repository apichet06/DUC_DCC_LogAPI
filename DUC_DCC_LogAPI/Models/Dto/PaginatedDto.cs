namespace DUC_DCC_LogAPI.Models.Dto
{
    public class PaginatedDto
    {
        public int Page { get; set; } = 0;
        public int Limit { get; set; } = 5;
        public string? OrderBy { get; set; }
        public string Order { get; set; } = "DESC";
    }

    public class PaginatedResponseDto<T>
    {
        public int? Total { get; set; }
        public int? Count { get; set; }
        public T? Data { get; set; }
    }
}
