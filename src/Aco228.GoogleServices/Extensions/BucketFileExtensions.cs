using Aco228.Common;
using Aco228.GoogleServices.Models;
using Aco228.GoogleServices.Services;

namespace Aco228.GoogleServices.Extensions;

public static class BucketFileExtensions
{
    public static string GetUrl(this BucketFile? bucketFile)
    {
        if (bucketFile == null) return string.Empty;
        return $"https://storage.googleapis.com/{bucketFile.BucketName}/{bucketFile.FileName}";
    }
    
    public static Task<string> GetTextAsync(this BucketFile? bucketFile)
    {
        if (bucketFile == null) return Task.FromResult(string.Empty);
        var httpClientFactory = ServiceProviderHelper.GetService<IHttpClientFactory>()!;
        var client = httpClientFactory.CreateClient();
        return client.GetStringAsync($"https://storage.googleapis.com/{bucketFile.BucketName}/{bucketFile.FileName}");
    }
    
    public static Task<byte[]> GetBinaryAsync(this BucketFile? bucketFile)
    {
        if (bucketFile == null) return Task.FromResult(Array.Empty<byte>());
        var httpClientFactory = ServiceProviderHelper.GetService<IHttpClientFactory>()!;
        var client = httpClientFactory.CreateClient();
        return client.GetByteArrayAsync($"https://storage.googleapis.com/{bucketFile.BucketName}/{bucketFile.FileName}");
    }

    public static async Task<bool> DeleteAsync(this BucketFile? bucketFile)
    {
        if (bucketFile == null) return false;
        var bucket = DefaultGoogleBucket.Construct(bucketFile.BucketName);
        return await bucket.DeleteFile(bucketFile);
    }
    
}