using MessagePack;

namespace Aco228.GoogleServices.Models;

[Serializable, MessagePackObject]
public class AdsenseStatReportEntry
{
    [Key(0)] public required string Date { get; set; }
    [Key(1)] public required string ChannelId { get; set; }
    [Key(2)] public required string CountryCode { get; set; }
    [Key(3)] public required double Revenue { get; set; }
    [Key(4)] public required int Clicks { get; set; }
}