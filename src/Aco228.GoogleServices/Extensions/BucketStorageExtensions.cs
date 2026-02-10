using Aco228.GoogleServices.Models;
using Aco228.GoogleServices.Services;
using Google.Cloud.Storage.V1;

namespace Aco228.GoogleServices.Extensions;

public static class BucketStorageExtensions
{
    internal static async Task<BucketFile> UploadAndGetBucketFile(
        this GoogleBucket bucket,
        Stream memoryStream,
        string fileName,
        string contentType)
    {
         var obj = await bucket.Client.UploadObjectAsync(
            bucket: bucket.BucketName,
            objectName: fileName,
            contentType: contentType,
            source: memoryStream,
            new()
            {
                PredefinedAcl = PredefinedObjectAcl.PublicRead,
                Projection = Projection.NoAcl,
            });
         
         return CreateBucketFile(bucket, obj);
    }
    
    internal static BucketFile CreateBucketFile(this GoogleBucket bucket, Google.Apis.Storage.v1.Data.Object storageObject)
    {
        return new()
        {
            BucketName = bucket.BucketName,
            FileName = storageObject.Name,
            Size = storageObject.Size ?? 0,
        };
    }
}