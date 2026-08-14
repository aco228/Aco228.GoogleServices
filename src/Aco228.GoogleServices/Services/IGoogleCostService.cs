using Aco228.AIGen.Documents;
using Aco228.AIGen.Services;
using Aco228.Common.Models;
using Aco228.MongoDb.Services;
using Google.Cloud.BigQuery.V2;

namespace Aco228.GoogleServices.Services;

public interface IGoogleCostService : IAIGenCostService, ITransient
{
}

public class GoogleCostService : AiGenCostBaseService, IGoogleCostService
{
    private readonly BigQueryClient _client;
    private readonly string _billingTable;
    private readonly string _billingId;
    private readonly string _billingDatasetId;
    
    public GoogleCostService(IMongoRepo<CostDocument> costRepo, IGoogleClientProvider googleClientProvider) : base(costRepo)
    {
        _client = googleClientProvider.GetBigQueryClient();
        _billingId = googleClientProvider.Setup.BillingId.Replace("-", "_");
        _billingDatasetId = googleClientProvider.Setup.BillingDatasetId;
        _billingTable = $"{googleClientProvider.ProjectId}.{_billingDatasetId}.gcp_billing_export_resource_v1_{_billingId}";
    }

    public override string ProviderName => "Google";
    protected override async Task<List<CostDocumentElement>> FillData(DateTime dateUtc)
    {
        var res = await GetCostByComponentForDay(_client, _billingTable, dateUtc);
        var result = new List<CostDocumentElement>();
        foreach (var costRow in res)
        {
            if (costRow.Cost < 0.01)
                continue;
            
            result.Add(new()
            {
                Name = costRow.Sku,
                Count = 1,
                Spend = costRow.Cost,
            });
        }

        return result;
    }

    public async Task Run()
    {
        var datasets = _client.ListDatasets();
        foreach (var ds in datasets)
        {
            Console.WriteLine($"{ds.Reference.DatasetId}  (location: {ds.Resource.Location})");
            var tables = ds.ListTables();
            foreach (var t in tables)
                Console.WriteLine(t.Reference.TableId);
        }
        
        var res = await GetCostByComponentForDay(_client, _billingTable, DateTime.Today.AddDays(-1));
        var a = 0;
    }
    
    public static async Task<List<CostRow>> GetCostByComponentForDay(BigQueryClient client, string billingId, DateTime day)
    {
        string sql = $@"
            SELECT
              service.description AS service,
              sku.description AS sku,
              SUM(cost) AS cost,
              currency
            FROM `{billingId}`
            WHERE DATE(usage_start_time) = @day
            GROUP BY service, sku, currency
            ORDER BY cost DESC";

        var parameters = new[]
        {
            new BigQueryParameter("day", BigQueryDbType.Date, day.Date)
        };

        var result = await client.ExecuteQueryAsync(sql, parameters);

        var rows = new List<CostRow>();
        foreach (var row in result)
        {
            rows.Add(new CostRow
            {
                Service = row["service"]?.ToString() ?? "",
                Sku = row["sku"]?.ToString() ?? "",
                Cost = Convert.ToDouble(row["cost"]),
                Currency = row["currency"]?.ToString() ?? ""
            });
        }
        return rows;
    }
    
    public class CostRow
    {
        public string Service { get; set; }
        public string Sku { get; set; }
        public double Cost { get; set; }
        public string Currency { get; set; }
    }
}