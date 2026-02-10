using Aco228.Common.Models;
using Aco228.GoogleServices.Models;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Http;
using Google.Cloud.Storage.V1;

namespace Aco228.GoogleServices.Services;

public interface IGoogleClientProvider : ISingleton
{
    HttpClient GetGoogleHttpClient();
    StorageClient GetStorageClient();
}

public class GoogleClientProvider : IGoogleClientProvider
{
    private readonly GoogleSetupOptions _googleSetupOptions;

    public GoogleClientProvider(GoogleSetupOptions googleSetupOptions)
    {
        _googleSetupOptions = googleSetupOptions;
    }
    
    public HttpClient GetGoogleHttpClient()
    {
        GoogleCredential credential = GoogleCredential.FromFile(_googleSetupOptions.GetGoogleCredentialsPath());
        var handler = credential.ToDelegatingHandler(new HttpClientHandler());
        return new(handler);
    }

    public StorageClient GetStorageClient()
    {
        GoogleCredential credential = GoogleCredential.FromFile(_googleSetupOptions.GetGoogleCredentialsPath());
        var storage = StorageClient.Create(credential);
        return storage;
    }
}