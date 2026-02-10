namespace Aco228.GoogleServices.Models;

public class BucketFile
{
    public required string BucketName { get; set; }
    public required MimeTypes MimeType { get; set; }
    public string FileName { get; set; }
    public ulong Size { get; set; }
}