
namespace Aco228.GoogleServices.Models;

public class GoogleSetupOptions
{
    public required string ProjectId { get; set; }
    public required string BillingId { get; set; }
    public required string BillingDatasetId { get; set; }
    public required string ServiceAccountCredentialsPath { get; set; }

    public string GetGoogleCredentialsPath()
    {
        if (string.IsNullOrEmpty(ServiceAccountCredentialsPath))
            throw new ArgumentNullException($"Credential name is null");

        if (!ServiceAccountCredentialsPath.EndsWith(".json"))
            ServiceAccountCredentialsPath += ".json";
        
        return ServiceAccountCredentialsPath;
    }
}