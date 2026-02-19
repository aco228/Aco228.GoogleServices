using Aco228.GoogleServices.Models;
using Google.Cloud.PubSub.V1;
using MessagePack;

namespace Aco228.GoogleServices.PubSub;


public interface IGoogleCloudPublisherBase<T> 
    where T : GoogleCloudMessage
{
    Task<string> PublishMessage(T message);
    Task TryShutdownPublisher();
}

public abstract class GoogleCloudPublisherBase<T> : IGoogleCloudPublisherBase<T> 
    where T : GoogleCloudMessage
{
    private readonly GoogleSetupOptions _googleSetupOptions;
    protected abstract string TopicId { get; }
    private PublisherClient? _publisherClient;

    protected GoogleCloudPublisherBase (GoogleSetupOptions googleSetupOptions)
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

    public async Task TryShutdownPublisher()
    {
        if(_publisherClient == null)
            return;
        
        await _publisherClient.ShutdownAsync(TimeSpan.FromSeconds(15));
    }

    public Task<string> PublishMessage(T message)
    {
        var bytes = MessagePackSerializer.Serialize(message, GooglePubSubService.MsgPackOptions);
        return PublishString(bytes);
    }

    protected async Task<string> PublishString(byte[] message)
    {
        var client = await GetPublisherClient();
        return await client.PublishAsync(message);
    }
}