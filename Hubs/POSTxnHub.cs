using Microsoft.AspNetCore.SignalR;
using RabbitMQ.Stream.Client;
using RabbitMQ.Stream.Client.Reliable;
using System.Buffers;
using System.Text;
using System.Text.Json;
using System.Net;

namespace POSTxns.Hubs
{
    public class POSTxnsHub : Hub
    {
        private readonly IConfiguration Configuration;
        private StreamSystem? rmqSystem;
        private const string stream = "my-reliable-pos-txns";
        private readonly IHubContext<POSTxnsHub> _hubContext;

        public async Task<StreamSystem> GetStreamSystem()
        {
            if (rmqSystem == null)
            {
                var rabbitMQOptions = new RabbitMQOptions();
                Configuration.GetSection(RabbitMQOptions.RabbitMQ).Bind(rabbitMQOptions);
                // var config = new StreamSystemConfig
                // {
                //     UserName = "guest",
                //     Password = "guest",
                //     VirtualHost = "/"
                // };
                var config = new StreamSystemConfig
                {
                    UserName = rabbitMQOptions.Username,
                    Password = rabbitMQOptions.Password,
                    VirtualHost = "/",
                    Endpoints = new List<EndPoint> {new DnsEndPoint(rabbitMQOptions.Host, rabbitMQOptions.StreamPort)}
                };

                Task<StreamSystem> tmp = StreamSystem.Create(config);
                tmp.Wait();
                rmqSystem = tmp.Result;
                bool strExists = await rmqSystem.StreamExists(stream);

                if (!strExists) {
                    Console.WriteLine("Stream does not exist, creating...");
                    await rmqSystem.CreateStream(new StreamSpec(stream)
                    {
                        MaxLengthBytes = 200000,
                    });
                }
            }

            return rmqSystem;
        }
        public async Task SendTxn(string store, string register, decimal total)
        {
            //await Clients.All.SendAsync("ReceiveTxn", store, register, total);
            var system = await this.GetStreamSystem();

            var producer = await system.CreateProducer(
                new ProducerConfig
                {
                    Reference = Guid.NewGuid().ToString(),
                    Stream = stream,
                    // Here you can receive the messages confirmation
                    // it means the message is stored on the server
                    ConfirmHandler = conf =>
                    {
                        Console.WriteLine($"message: {conf.PublishingId} - confirmed");
                    }
                });
            POSTxn txnInfo = new POSTxn(store,register,total);
            string jsonString = JsonSerializer.Serialize(txnInfo);
            var message = new Message(Encoding.UTF8.GetBytes(jsonString));
            await producer.Send(0, message);
        }

        public async Task SendMessage(string publishingId, string jsonString)
        {
            POSTxn txnInfo = JsonSerializer.Deserialize<POSTxn>(jsonString)!;
            Console.WriteLine(jsonString);
            await Clients.All.SendAsync("ReceiveTxn", txnInfo.storeId, txnInfo.registerId, txnInfo.total);
        }
        public POSTxnsHub()
        {
            Console.WriteLine("Reliable .NET Cosumer");
            var getTask = this.GetStreamSystem();
            getTask.Wait();
            var system = getTask.Result;
            var rConsumer = ReliableConsumer.CreateReliableConsumer(new ReliableConsumerConfig()
            {
                Stream = stream,
                StreamSystem = system,
                OffsetSpec = new OffsetTypeFirst(),
                ClientProvidedName = "My-Reliable-Consumer",
                MessageHandler = async (_, _, message) =>
                {
                    Console.WriteLine("Message Received");
                    string jsonString = Encoding.Default.GetString(message.Data.Contents.ToArray());
                    
                    await this.SendMessage("",jsonString);
                    await Task.CompletedTask;
                }
            });
        }
    }
}