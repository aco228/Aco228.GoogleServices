using System.Text.Json;
using Aco228.Common.Models;
using Aco228.GoogleServices.Models;
using Google.Apis.Adsense.v2;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Services;
using Google.Apis.Util;

namespace Aco228.GoogleServices.Services;

public interface IGoogleAdsenseStatService : ITransient
{
    IAsyncEnumerable<AdsenseStatReportEntry> PullStats(string tokenJson, string clientId, string clientSecret, string adsensePubAccountId, DateTime date);
}

public class GoogleAdsenseStatService : IGoogleAdsenseStatService
{
    public async IAsyncEnumerable<AdsenseStatReportEntry> PullStats(string tokenJson, string clientId, string clientSecret, string adsensePubAccountId, DateTime date)
    {
        var doc = JsonDocument.Parse(tokenJson).RootElement;
        var tokenResponse = new TokenResponse
        {
            AccessToken = doc.GetProperty("token").GetString(),
            RefreshToken = doc.GetProperty("refresh_token").GetString(),
            TokenType = "Bearer",
            Scope = "https://www.googleapis.com/auth/adsense.readonly",
            IssuedUtc = DateTime.UtcNow
        };

        var flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
        {
            ClientSecrets = new ClientSecrets
            {
                ClientId = clientId,
                ClientSecret = clientSecret
            },
            Scopes = new[] { "https://www.googleapis.com/auth/adsense.readonly" }
        });

        var credential = new UserCredential(flow, "user", tokenResponse);

        var service = new AdsenseService(new BaseClientService.Initializer
        {
            HttpClientInitializer = credential,
            ApplicationName = "AdCompiler"
        });

        // Full resource name — NOT "accounts/pub-..." for the account arg used to construct the request,
        // just the account path segment itself
        var request = service.Accounts.Reports.Generate("accounts/" + adsensePubAccountId);

        // IMPORTANT: the property for multiple values is "DimensionsList" / "MetricsList" (type Repeatable<T>),
        // NOT "Dimensions"/"Metrics" — those are for a SINGLE value only and are separate properties.
        request.DimensionsList = new Repeatable<AccountsResource.ReportsResource.GenerateRequest.DimensionsEnum>(
            new[]
            {
                AccountsResource.ReportsResource.GenerateRequest.DimensionsEnum.DATE,
                AccountsResource.ReportsResource.GenerateRequest.DimensionsEnum.CUSTOMCHANNELNAME,
                AccountsResource.ReportsResource.GenerateRequest.DimensionsEnum.COUNTRYCODE,
            });

        request.MetricsList = new Repeatable<AccountsResource.ReportsResource.GenerateRequest.MetricsEnum>(
            new[]
            {
                AccountsResource.ReportsResource.GenerateRequest.MetricsEnum.ESTIMATEDEARNINGS,
                AccountsResource.ReportsResource.GenerateRequest.MetricsEnum.CLICKS,
                AccountsResource.ReportsResource.GenerateRequest.MetricsEnum.IMPRESSIONS
            });

        var day = new DateOnly(date.Year, date.Month, date.Day);
        request.StartDateYear = day.Year;
        request.StartDateMonth = day.Month;
        request.StartDateDay = day.Day;
        request.EndDateYear = day.Year;
        request.EndDateMonth = day.Month;
        request.EndDateDay = day.Day;

        var response = await request.ExecuteAsync();

        var headerNames = response.Headers.Select(h => h.Name).ToList();
        
        if (response.Rows != null)
        {
            foreach (var row in response.Rows)
            {
                var cellValues = row.Cells.Select(c => c.Value).ToList();
                var map = headerNames
                    .Zip(cellValues, (h, v) => new { Header = h, Value = v })
                    .ToDictionary(x => x.Header, x => x.Value);
                
                string mapDate = map.GetValueOrDefault("DATE")?.ToString() ?? "";
                string channelName = map.GetValueOrDefault("CUSTOM_CHANNEL_NAME")?.ToString() ?? "";
                string earnings = map.GetValueOrDefault("ESTIMATED_EARNINGS")?.ToString() ?? "0";
                string clicks = map.GetValueOrDefault("CLICKS")?.ToString() ?? "0";
                string countryCode = map.GetValueOrDefault("COUNTRY_CODE")?.ToString() ?? "0";
                
                yield return new()
                {
                    ChannelId = channelName,
                    Clicks = int.TryParse(clicks, out var click) ? click : 0,
                    CountryCode = countryCode,
                    Revenue = double.TryParse(earnings, out var revenue) ? revenue : 0,
                    Date = mapDate,
                };
            }
        }
        else
        {
            Console.WriteLine("No rows returned.");
        }
    }
}