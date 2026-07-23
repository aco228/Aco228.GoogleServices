using System.Text.Json;
using Aco228.Common.Models;
using Google.Apis.Adsense.v2;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Services;
using Google.Apis.Util;

namespace Aco228.GoogleServices.Services;

public interface IGoogleAdsenseStatManager : ITransient
{
    Task PullStats(string tokenJson, string clientId, string clientSecret, string adsensePubAccountId);
}

public class GoogleAdsenseStatManager : IGoogleAdsenseStatManager
{
    public async Task PullStats(string tokenJson, string clientId, string clientSecret, string adsensePubAccountId)
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
                AccountsResource.ReportsResource.GenerateRequest.DimensionsEnum.CUSTOMCHANNELID,
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

        var day = new DateOnly(2026, 7, 22);
        request.StartDateYear = day.Year;
        request.StartDateMonth = day.Month;
        request.StartDateDay = day.Day;
        request.EndDateYear = day.Year;
        request.EndDateMonth = day.Month;
        request.EndDateDay = day.Day;

        var result = await request.ExecuteAsync();

        var headerNames = result.Headers.Select(h => h.Name).ToList();
        if (result.Rows != null)
        {
            foreach (var row in result.Rows)
            {
                var cellValues = row.Cells.Select(c => c.Value).ToList();
                var map = headerNames
                    .Zip(cellValues, (h, v) => new { Header = h, Value = v })
                    .ToDictionary(x => x.Header, x => x.Value);
                
                string date = map.GetValueOrDefault("DATE")?.ToString() ?? "";
                string channelName = map.GetValueOrDefault("CUSTOM_CHANNEL_NAME")?.ToString() ?? "";
                string channelId = map.GetValueOrDefault("CUSTOM_CHANNEL_ID")?.ToString() ?? "";
                string earnings = map.GetValueOrDefault("ESTIMATED_EARNINGS")?.ToString() ?? "0";
                string clicks = map.GetValueOrDefault("CLICKS")?.ToString() ?? "0";
                int a = 0;
            }
        }
        else
        {
            Console.WriteLine("No rows returned.");
        }
    }
}