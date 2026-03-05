using Aco228.Common.Models;
using Aco228.GoogleServices.Models;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.PubSub.V1;
using MessagePack;

namespace Aco228.GoogleServices.PubSub;

public interface IGoogleCloudSubscriberBase<T>
    where T : GoogleCloudMessage
{
    Task StartListening(HostMachineContract hostMachineContract, string subscriptionId);
    void StartListeningInBackground(HostMachineContract hostMachineContract);
}

public abstract class GoogleCloudSubscriberBase<T> : IGoogleCloudSubscriberBase<T>
    where T : GoogleCloudMessage
{
    private readonly GoogleSetupOptions _googleSetupOptions;
    protected abstract string TopicName { get; }
    private SubscriberClient? _subscriberClient;

    public GoogleCloudSubscriberBase (
        GoogleSetupOptions googleSetupOptions)
    {
        _googleSetupOptions = googleSetupOptions;
    }
    
    protected async Task<SubscriberClient> GetSubscriberClient(string subscriptionId)
    {
        if (_subscriberClient != null)
            return _subscriberClient;
    
        var topicName = new TopicName(_googleSetupOptions.ProjectId, TopicName);
        var subscriptionName = new SubscriptionName(_googleSetupOptions.ProjectId, subscriptionId);
    
        var credential = GoogleCredential.FromFile(_googleSetupOptions.GetGoogleCredentialsPath())
            .CreateScoped(SubscriberServiceApiClient.DefaultScopes);
    
        var subscriberService = await new SubscriberServiceApiClientBuilder
        {
            Credential = credential
        }.BuildAsync();
    
        try
        {
            await subscriberService.GetSubscriptionAsync(subscriptionName);
        }
        catch (Grpc.Core.RpcException e) when (e.StatusCode == Grpc.Core.StatusCode.NotFound)
        {
            await subscriberService.CreateSubscriptionAsync(
                subscriptionName,
                topicName,
                pushConfig: null,
                ackDeadlineSeconds: 60
            );
        }

        _subscriberClient = await new SubscriberClientBuilder
        {
            SubscriptionName = subscriptionName,
            Credential = credential
        }.BuildAsync();
    
        return _subscriberClient;
    }
    
    public async Task StartListening(HostMachineContract hostMachine, string subscriberId)
    {
        if (string.IsNullOrEmpty(subscriberId))
            throw new InvalidOperationException($"No subscription provided");
        
        var subscriber = await GetSubscriberClient(subscriberId);
        await subscriber.StartAsync(async (msg, cancellationToken) =>
        {
            try
            {
                var message = MessagePackSerializer.Deserialize<T>(msg.Data.ToByteArray());

                if (message == null)
                    throw new ArgumentException("Message is null");
                
                if(message.Sender.ServerName.Equals(hostMachine.MachineName))
                    return SubscriberClient.Reply.Ack;
                    
                if(message.Receiver != null && message.Receiver.ServerName.Equals(hostMachine.MachineName))
                    return SubscriberClient.Reply.Ack;
                
                ImplProcessMessage(message).ConfigureAwait(false);
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Exception {ex}");
            }
            
            return SubscriberClient.Reply.Ack;
        });
    }

    public void StartListeningInBackground(HostMachineContract hostMachine)
    {
        string subscribeId = $"{TopicName}_{hostMachine.MachineName}_{hostMachine.ApplicationName}";
        new Thread(async () =>
        {
            for (;;)
            {
                try
                {
                    await StartListening(hostMachine, subscribeId);
                }
                catch (Exception e)
                {
                    break;
                }
                int a = 0;
            }
        }).Start();
    }

    protected async Task ImplProcessMessage(T message)
    {
        try
        {
            await ProcessMessage(message);
        }
        catch(Exception ex)
        {
            Console.WriteLine($"GoogleCloudSubscriber::Exception on processing message");
            Console.WriteLine(ex);
        }
    }

    protected abstract Task ProcessMessage(T message);

}