namespace DUC_DCC_LogAPI.Models.Dtos
{
    public class FileDownloadDto
    {
        public byte[]? Content { get; set; }
        public string FileName { get; set; } = null!;
        public string ContentType { get; set; } = null!;
    }
}
