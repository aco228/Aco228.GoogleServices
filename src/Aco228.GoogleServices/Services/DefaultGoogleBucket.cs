using Aco228.Common;

namespace Aco228.GoogleServices.Services;

public interface IDefaultGoogleBucket : IGoogleBucket
{
    
}

public class DefaultGoogleBucket : GoogleBucket, IDefaultGoogleBucket
{
    private readonly string _bucketName;
    public override string BucketName => _bucketName;
    
    public DefaultGoogleBucket(
        string bucketName,
        IGoogleClientProvider googleClientProvider) 
        : base(googleClientProvider)
    {
        _bucketName = bucketName;
    }

    public static DefaultGoogleBucket Construct(string bucketName)
    {
        var googleClientProvider = ServiceProviderHelper.GetService<IGoogleClientProvider>()!;
        return new DefaultGoogleBucket(bucketName, googleClientProvider);
    }
}