using Aco228.Common;
using Aco228.GoogleServices.Models;
using Aco228.GoogleServices.Services;

namespace Aco228.GoogleServices.Extensions;

public static class BucketFileExtensions
{
    public static Task<string> GetTextAsync(this BucketFile bucketFile)
    {
        var httpClientFactory = ServiceProviderHelper.GetService<IHttpClientFactory>()!;
        var client = httpClientFactory.CreateClient();
        return client.GetStringAsync($"https://storage.googleapis.com/{bucketFile.BucketName}/{bucketFile.FileName}");
    }
    
    public static Task<byte[]> GetBinaryAsync(this BucketFile bucketFile)
    {
        var httpClientFactory = ServiceProviderHelper.GetService<IHttpClientFactory>()!;
        var client = httpClientFactory.CreateClient();
        return client.GetByteArrayAsync($"https://storage.googleapis.com/{bucketFile.BucketName}/{bucketFile.FileName}");
    }

    public static async Task<bool> DeleteAsync(this BucketFile bucketFile)
    {
        var bucket = DefaultGoogleBucket.Construct(bucketFile.BucketName);
        return await bucket.DeleteFile(bucketFile);
    }
    
}