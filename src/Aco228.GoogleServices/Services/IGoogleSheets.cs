using Aco228.Common;
using Google.Apis.Drive.v3;
using Google.Apis.Sheets.v4;

namespace Aco228.GoogleServices.Services;

public interface IGoogleSheets
{
}

public abstract class GoogleSheets : IGoogleSheets
{
    protected string SpreadsheetId { get; set; }
    protected SheetsService Client { get; set; }
    
    public GoogleSheets( string spreadsheetId)
    {
        SpreadsheetId = spreadsheetId;
        var clientProvider = ServiceProviderHelper.GetService<IGoogleClientProvider>()!;
        Client = clientProvider.GetSheetsClient();
    }
}