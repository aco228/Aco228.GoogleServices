using Aco228.MongoDb.Models;

namespace Aco228.GoogleServices.Models;

public class BucketFile
{
    public string BucketName { get; set; }
    public string FileName { get; set; }
    public ulong Size { get; set; }
}