using Aco228.Common.Models;
using Aco228.GoogleServices.Services;

namespace Aco228.GoogleServices.Consoler.Buckets;

public interface ITempBucket : IGoogleBucket, ITransient
{
}

public class TempBucket : GoogleBucket, ITempBucket
{
    public override string BucketName => "arbo-temp-files";

    public TempBucket(IGoogleClientProvider googleClientProvider)
        : base(googleClientProvider)
    {
        
    }
}