using System.Text;
using Aco228.Common.Helpers;
using Aco228.GoogleServices.Extensions;
using Aco228.GoogleServices.Helpers;
using Aco228.GoogleServices.Models;
using Google.Cloud.Storage.V1;
using Microsoft.AspNetCore.StaticFiles;

namespace Aco228.GoogleServices.Services;

public interface IGoogleBucket
{
    string BucketName { get; }
    
    // upload
    Task<BucketFile?> UploadFileAsync(byte[] fileBytes, string fileName);
    Task<BucketFile?> UploadFileAsync(FileInfo file, string? fileName = null);
    Task<BucketFile?> UploadStringFile(string? data, MimeTypes type = MimeTypes.json, string? fileName = null);
    
    Task<bool> DeleteFileByName(string fileName);
    Task<bool> DeleteFile(BucketFile bucketFile);
    
    Task DeleteDirectory(string directoryName, int concurrency = 10);
    Task UploadDirectory(string directoryName, DirectoryInfo directoryInfo, int concurrency = 10);
    
    Task<List<BucketFile>> GetAllFiles();
    IAsyncEnumerable<BucketFile> EnumerateAllFiles();
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
            var mimeType = MimeTypeHelper.GetMimeType(extension)!.Value;
            
            using var memoryStream = new MemoryStream(fileBytes);
            var file = await this.UploadAndGetBucketFile(memoryStream, fileName, mimeType);
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
            var mimeType = MimeTypeHelper.GetMimeType(extension)!.Value;
            var name = fileName ?? file.Name;

            await using FileStream fileStream = File.Open(file.FullName, FileMode.Open);
            var result = await this.UploadAndGetBucketFile(fileStream, name, mimeType);
            return result;
        }
        catch
        {
            return null;
        }
    }

    public async Task<BucketFile?> UploadStringFile(
        string? data,
        MimeTypes type = MimeTypes.json,
        string? fileName = null)
    {
        if(string.IsNullOrEmpty(data))
            return null;

        try
        {
            var name = fileName ?? $"{IdHelper.GetId()}.{type.ToString()}";
            await using MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(data.Trim()));
            
            var result = await this.UploadAndGetBucketFile(stream, name, type);
            return result;
        }
        catch (Exception e)
        {
            return null;
        }
    }

    public async Task<bool> DeleteFileByName(string fileName)
    {
        try
        {
            await Client.DeleteObjectAsync(bucket: BucketName, objectName: fileName);
            return true;
        }
        catch (Exception ex)
        {
            if (ex.ToString().Contains($"No such object: {BucketName}/{fileName} [404]"))
            {
                return true;
            }
            
            Console.WriteLine($"Google.Storage:: Could not delete {fileName}");
            return false;
        }
    }

    public async Task<bool> DeleteFile(BucketFile bucketFile)
    {
        try
        {
            await Client.DeleteObjectAsync(bucket: BucketName, objectName: bucketFile.FileName);
            return true;
        }
        catch (Exception ex)
        {
            if (ex.ToString().Contains($"No such object: {BucketName}/{bucketFile.FileName} [404]"))
            {
                return true;
            }
            
            Console.WriteLine($"Google.Storage:: Could not delete {bucketFile.FileName}");
            return false;
        }
    }

    public async Task DeleteDirectory(string directoryName, int concurrency = 10)
    {
        var objects = Client.ListObjectsAsync(BucketName, directoryName);
        using var semaphore = new SemaphoreSlim(concurrency);

        var tasks = new List<Task>();
        await foreach (var obj in objects)
        {
            var objName = obj.Name;
            tasks.Add(Task.Run(async () =>
            {
                await semaphore.WaitAsync();
                try
                {
                    await Client.DeleteObjectAsync(BucketName, objName);
                }
                finally
                {
                    semaphore.Release();
                }
            }));
        }

        await Task.WhenAll(tasks);
    }

    public async Task UploadDirectory(string directoryName, DirectoryInfo directoryInfo, int concurrency = 10)
    {
        var files = directoryInfo.GetFiles("*", SearchOption.AllDirectories);
        using var semaphore = new SemaphoreSlim(concurrency); // max 10 concurrent uploads

        var tasks = files.Select(async file =>
        {
            await semaphore.WaitAsync();
            try
            {
                var relativePath = Path.GetRelativePath(directoryInfo.FullName, file.FullName)
                    .Replace("\\", "/");

                var objectName = $"{directoryName.TrimEnd('/')}/{relativePath}";
                var provider = new FileExtensionContentTypeProvider();
                if (!provider.TryGetContentType(file.Name, out var contentType))
                    contentType = "application/octet-stream";
                
                using var stream = file.OpenRead();
                await Client.UploadObjectAsync(
                    BucketName, 
                    objectName, 
                    contentType, 
                    stream,
                    new UploadObjectOptions { PredefinedAcl = PredefinedObjectAcl.PublicRead }
                );
            }
            finally
            {
                semaphore.Release();
            }
        });

        await Task.WhenAll(tasks);
    }

    public async Task<List<BucketFile>> GetAllFiles()
    {
        var response = new List<BucketFile>();
        await foreach (var bucketObject in Client.ListObjectsAsync(bucket: BucketName).AsRawResponses())
        {
            foreach (var obj in bucketObject.Items)
                response.Add(new()
                {
                    FileName = obj.Name,
                    MimeType = MimeTypes.unknown,
                    BucketName = BucketName,
                    Size = obj.Size ?? 0,
                });
        }

        return response;
    }

    public async IAsyncEnumerable<BucketFile> EnumerateAllFiles()
    {
        await foreach (var bucketObject in Client.ListObjectsAsync(bucket: BucketName).AsRawResponses())
        {
            foreach (var obj in bucketObject.Items)
                yield return new()
                {
                    FileName = obj.Name,
                    MimeType = MimeTypes.unknown,
                    BucketName = BucketName,
                    Size = obj.Size ?? 0,
                };
        }
    }
}
