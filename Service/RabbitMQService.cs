using System.Net;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.SignalR;
using POSTxns.Hubs;
using System.Text;
using System.Text.Json;
using System.Buffers;
using RabbitMQ.Stream.Client;
using RabbitMQ.Stream.Client.Reliable;

namespace POSTxns.Service
{
    public class RabbitMQService : IRabbitMQService {
        private StreamSystem streamSystem;

        private readonly RabbitMQOptions rmqOptions;

        public IHubContext<POSTxnsHub, IPOSTxnHubClient> _strongPOSTxnsHubContext { get; }

        public RabbitMQService(IOptions<RabbitMQOptions> opts, IHubContext<POSTxnsHub, IPOSTxnHubClient> clientHubContext) {
            rmqOptions = opts.Value;
            _strongPOSTxnsHubContext = clientHubContext;
        }

        public RabbitMQOptions getRabbitConfig() {
            return rmqOptions;
        } 

        public async void createConsumer() {
            Console.WriteLine("Reliable .NET Cosumer");
        
            var localStreamSystem = await this.getStreamSystem();
            var rConsumer = await ReliableConsumer.CreateReliableConsumer(
                new ReliableConsumerConfig()
                {
                    Reference = Guid.NewGuid().ToString(),
                    Stream = this.getRabbitConfig().StreamName,
                    StreamSystem = localStreamSystem,
                    OffsetSpec = new OffsetTypeFirst(),
                    ClientProvidedName = "My-Reliable-Consumer",
                    MessageHandler = async (_, _, message) =>
                    {
                        Console.WriteLine("Message Received");
                        string jsonString = Encoding.Default.GetString(message.Data.Contents.ToArray());
                        Console.WriteLine(jsonString);
                        
                        POSTxn? txn = JsonSerializer.Deserialize<POSTxn>(jsonString);
                        await _strongPOSTxnsHubContext.Clients.All.SendTxn(txn.storeId,txn.registerId,txn.total.Value);
                        await Task.CompletedTask;
                    }
                });
        }

        public async Task<StreamSystem> getStreamSystem() {
            if (streamSystem == null)
            {
                // var config = new StreamSystemConfig
                // {
                //     UserName = "guest",
                //     Password = "guest",
                //     VirtualHost = "/"
                // };
                var config = new StreamSystemConfig
                {
                    UserName = rmqOptions.Username,
                    Password = rmqOptions.Password,
                    VirtualHost = "/",
                    Endpoints = new List<EndPoint> {new DnsEndPoint(rmqOptions.Host, rmqOptions.StreamPort)}
                };

                Task<StreamSystem> tmp = StreamSystem.Create(config);
                tmp.Wait();
                streamSystem = tmp.Result;
                bool strExists = await streamSystem.StreamExists(rmqOptions.StreamName);

                if (!strExists) {
                    Console.WriteLine("Stream does not exist, creating...");
                    await streamSystem.CreateStream(new StreamSpec(rmqOptions.StreamName)
                    {
                        MaxLengthBytes = 200000,
                    });
                }
            }
            return streamSystem;
        }

        public async Task<Producer> getProducer() {
            var streamSystem = await this.getStreamSystem();
            Producer producer = await streamSystem.CreateProducer(
                    new ProducerConfig
                    {
                        Reference = Guid.NewGuid().ToString(),
                        Stream = rmqOptions.StreamName,
                        // Here you can receive the messages confirmation
                        // it means the message is stored on the server
                        ConfirmHandler = conf =>
                        {
                            Console.WriteLine($"message: {conf.PublishingId} - confirmed");
                        }
                    });
            return producer;
        }
    }
}