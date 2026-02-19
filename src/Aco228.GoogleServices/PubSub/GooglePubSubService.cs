using Aco228.GoogleServices.Models;
using Google.Cloud.PubSub.V1;
using MessagePack;

namespace Aco228.GoogleServices.PubSub;

public abstract class GooglePubSubService
{

    internal static readonly MessagePackSerializerOptions MsgPackOptions =
        MessagePackSerializerOptions.Standard
            .WithCompression(MessagePackCompression.None)
            .WithResolver(MessagePack.Resolvers.ContractlessStandardResolver.Instance);
    
    private readonly GoogleSetupOptions _googleSetupOptions;
    protected abstract string TopicId { get; }
    protected abstract string SubscriptionId { get; }
    private PublisherClient? _publisherClient;
    private SubscriberClient? _subscriberClient;

    public GooglePubSubService(GoogleSetupOptions googleSetupOptions)
    {
        _googleSetupOptions = googleSetupOptions;
    }

     protected async Task<PublisherClient> GetPublisherClient()
    {
        if (_publisherClient != null)
            return _publisherClient;

        var topicName = new TopicName(_googleSetupOptions.ProjectId, TopicId);
        _publisherClient = await PublisherClient.CreateAsync(topicName);
        return _publisherClient;
    }

    protected async Task<SubscriberClient> GetSubscriberClient()
    {
        if (_subscriberClient != null)
            return _subscriberClient;
        
        var topicName = new TopicName(_googleSetupOptions.ProjectId, TopicId);
        var subscriptionName = new SubscriptionName(_googleSetupOptions.ProjectId, SubscriptionId);
        _subscriberClient = await SubscriberClient.CreateAsync(subscriptionName);
        var subscriberService = await SubscriberServiceApiClient.CreateAsync();
        
        try
        {
            // Try to get it (check if it already exists)
            await subscriberService.GetSubscriptionAsync(subscriptionName);
        }
        catch (Grpc.Core.RpcException e) when (e.StatusCode == Grpc.Core.StatusCode.NotFound)
        {
            // Create if not found
            await subscriberService.CreateSubscriptionAsync(
                subscriptionName,
                topicName,
                pushConfig: null,
                ackDeadlineSeconds: 60
            );
        }
        return _subscriberClient;
    }

    protected async Task TryShutdownPublisher()
    {
        if(_publisherClient == null)
            return;
        
        await _publisherClient.ShutdownAsync(TimeSpan.FromSeconds(15));
    }

    public async Task<string> PublishString(string message)
    {
        var client = await GetPublisherClient();
        return await client.PublishAsync(message);
    }

    public async Task StartListening()
    {
        var subscriber = await GetSubscriberClient();
        await subscriber.StartAsync((msg, cancellationToken) =>
        {
            Console.WriteLine($"Received message {msg.MessageId} published at {msg.PublishTime.ToDateTime()}");
            Console.WriteLine($"Text: '{msg.Data.ToStringUtf8()}'");
            
            return Task.FromResult(SubscriberClient.Reply.Ack);
        });

    }
    
}