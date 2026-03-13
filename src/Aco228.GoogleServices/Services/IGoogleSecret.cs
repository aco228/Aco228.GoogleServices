using Aco228.Common.Models;
using Aco228.Common.Services;
using Aco228.GoogleServices.Models;
using Google.Cloud.SecretManager.V1;

namespace Aco228.GoogleServices.Services;

public interface IGoogleSecret : ISecretProvider, ISingleton
{
}

public class GoogleSecret : IGoogleSecret
{
    private readonly GoogleSetupOptions _googleSetupOptions;
    private SecretManagerServiceClient _client;

    public GoogleSecret(
        GoogleSetupOptions googleSetupOptions,
        IGoogleClientProvider googleClientProvider)
    {
        _googleSetupOptions = googleSetupOptions;
        _client = googleClientProvider.CreateSecretClient();
    }

    public string Get(string key)
    {
        var secretVersionName = new SecretVersionName(_googleSetupOptions.ProjectId, key, "latest");
        var response = _client.AccessSecretVersion(secretVersionName);
        return response.Payload.Data.ToStringUtf8();
    }
}