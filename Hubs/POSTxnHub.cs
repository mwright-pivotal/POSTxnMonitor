using Microsoft.AspNetCore.SignalR;
using RabbitMQ.Stream.Client;
using System.Text;
using System.Text.Json;
using POSTxns.Service;
using Microsoft.Extensions.Options;

namespace POSTxns.Hubs
{
    public class POSTxnsHub : Hub
    {
        private readonly IRabbitMQService rabbitMQService;

        private readonly POSTxns.Hubs.POSTxnMonitorOptions appOptions;

        public POSTxnsHub(IRabbitMQService rmqService, IOptions<POSTxns.Hubs.POSTxnMonitorOptions> _posAppOptions) {
            Console.WriteLine("Initializing RMQ Service");
            rabbitMQService = rmqService;
            rabbitMQService.createConsumer();
            rabbitMQService.setHub(this);
            appOptions = _posAppOptions.Value;
        }
        
        public async Task SendTxn(string registerId, string itemId, decimal total)
        {
            string jsonString = JsonSerializer.Serialize(new POSTxn(appOptions.StoreId,registerId,itemId,total));
            var message = new Message(Encoding.UTF8.GetBytes(jsonString));

            var producer = await rabbitMQService.getProducer();
            await producer.Send(0, message);
        }

        public async Task ReceiveTxn(string storeId, string registerId, string itemId, decimal total) {
            Console.WriteLine("Dispatching message to browser");
            await Clients.All.SendAsync("ReceiveTxn", storeId, registerId, itemId, total);
        }
    }
}