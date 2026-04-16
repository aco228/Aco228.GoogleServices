using Aco228.Common.Models;
using Aco228.GoogleServices.Models;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Http;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Cloud.SecretManager.V1;
using Google.Cloud.Storage.V1;

namespace Aco228.GoogleServices.Services;

public interface IGoogleClientProvider : ISingleton
{
    HttpClient GetGoogleHttpClient();
    StorageClient GetStorageClient();
    SheetsService GetSheetsClient();
    DriveService GetDriveClient();
    SecretManagerServiceClient CreateSecretClient();
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

    public SheetsService GetSheetsClient()
    {
        GoogleCredential credential = GoogleCredential.FromFile(_googleSetupOptions.GetGoogleCredentialsPath());
        var service = new SheetsService(new BaseClientService.Initializer
        {
            HttpClientInitializer = credential,
        });
        return service;
    }

    public DriveService GetDriveClient()
    {
        GoogleCredential credential = GoogleCredential.FromFile(_googleSetupOptions.GetGoogleCredentialsPath());
        var service = new DriveService(new BaseClientService.Initializer
        {
            HttpClientInitializer = credential,
        });
        return service;
    }

    public SecretManagerServiceClient CreateSecretClient()
    {
        var json = File.ReadAllText(_googleSetupOptions.GetGoogleCredentialsPath());
        var builder = new SecretManagerServiceClientBuilder
        {
            JsonCredentials = json
        };

        return builder.Build();
    }
}