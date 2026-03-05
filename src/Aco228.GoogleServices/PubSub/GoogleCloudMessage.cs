using MessagePack;

namespace Aco228.GoogleServices.PubSub;

[MessagePackObject]
public class GoogleCloudMessage
{
    [Key(0)] public ushort Type { get; set; }
    [Key(1)] public GoogleCloudIdentifier Sender { get; set; }
    [Key(2)] public GoogleCloudIdentifier? Receiver { get; set; }
    [Key(3)] public DateTime CreatedUtc { get; set; }
}

[MessagePackObject]
public class GoogleCloudIdentifier
{
    [Key(0)] public string ServerName { get; set; }

    public GoogleCloudIdentifier() { }
    public GoogleCloudIdentifier(string name)
    {
        ServerName = name;
    }
}

[MessagePackObject]
public class CommandGoogleCloudMessage : GoogleCloudMessage
{
    [Key(4)] public required string Commmand { get; set; }
    [Key(5)] public string? Arg1 { get; set; }
    [Key(6)] public string? Arg2 { get; set; }
}