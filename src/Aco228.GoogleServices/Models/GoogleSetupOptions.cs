using Aco228.Common.Helpers;

namespace Aco228.GoogleServices.Models;

public class GoogleSetupOptions
{
    public string ServiceAccountCredentialsName { get; set; }

    public string GetGoogleCredentialsPath()
    {
        if (string.IsNullOrEmpty(ServiceAccountCredentialsName))
            throw new ArgumentNullException($"Credential name is null");

        if (!ServiceAccountCredentialsName.EndsWith(".json"))
            ServiceAccountCredentialsName += ".json";
        
        if(!AssemblyFileLocator.TryFindAssemblyFile(ServiceAccountCredentialsName, out var fileInfo))
            throw new ArgumentNullException($"Credential file {ServiceAccountCredentialsName} not found");
        
        return fileInfo.FullName;
    }
}