namespace Core.DTOs.Response.Queue
{
    public class Message
    {
        public int FileTypeId { get; set; }
        public string BlobName { get; set; }
        public bool Override { get; set; } = false;
    }
}