using System.Text;
using Aco228.Common.Helpers;
using Aco228.GoogleServices.Extensions;
using Aco228.GoogleServices.Helpers;
using Aco228.GoogleServices.Models;
using Google.Cloud.Storage.V1;

namespace Aco228.GoogleServices.Services;

public interface IGoogleBucket
{
    // upload
    Task<BucketFile?> UploadFileAsync(byte[] fileBytes, string fileName);
    Task<BucketFile?> UploadFileAsync(FileInfo file, string? fileName = null);
    Task<BucketFile?> UploadStringFile(string data, string? fileName = null);
}

public abstract class GoogleBucket : IGoogleBucket
{
    public abstract string BucketName { get; }
    internal readonly StorageClient Client;
    
    public GoogleBucket(IGoogleClientProvider googleClientProvider)
    {
        Client = googleClientProvider.GetStorageClient();
    }
    
    public async Task<BucketFile?> UploadFileAsync(byte[] fileBytes, string fileName)
    {
        try
        {
            var extension = fileName.Substring(fileName.LastIndexOf('.'));
            var contentType = MimeTypeHelper.GetMimeType(extension);
            
            using var memoryStream = new MemoryStream(fileBytes);
            var file = await this.UploadAndGetBucketFile(memoryStream, fileName, contentType);
            return file;
        }
        catch
        {
            return null;
        }
    }

    public async Task<BucketFile?> UploadFileAsync(FileInfo file, string? fileName = null)
    {
        try
        {
            var extension = file.Extension;
            var contentType = MimeTypeHelper.GetMimeType(extension);
            var name = fileName ?? file.Name;

            await using FileStream fileStream = File.Open(file.FullName, FileMode.Open);
            var result = await this.UploadAndGetBucketFile(fileStream, name, contentType);
            return result;
        }
        catch
        {
            return null;
        }
    }

    public async Task<BucketFile?> UploadStringFile(string data, string? fileName = null)
    {
        if(string.IsNullOrEmpty(data))
            return null;

        try
        {
            var contentType  = "application/json";
            var name = fileName ?? $"{IdHelper.GetId()}.json";
            await using MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(data));
            
            var result = await this.UploadAndGetBucketFile(stream, name, contentType);
            return result;
        }
        catch (Exception e)
        {
            return null;
        }
    }
}
