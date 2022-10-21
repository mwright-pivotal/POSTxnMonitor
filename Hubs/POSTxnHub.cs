using Microsoft.AspNetCore.SignalR;
using RabbitMQ.Stream.Client;
using System.Text;
using System.Text.Json;
using POSTxns.Service;

namespace POSTxns.Hubs
{
    public class POSTxnsHub : Hub<IPOSTxnHubClient>
    {
        private readonly IRabbitMQService rabbitMQService;

        public POSTxnsHub(IRabbitMQService rmqService) {
            rabbitMQService = rmqService;
            rabbitMQService.createConsumer();
        }
        public async Task SendTxn(string storeId, string registerId, decimal total)
        {

            string jsonString = JsonSerializer.Serialize(new POSTxn(storeId,registerId,total));
            var message = new Message(Encoding.UTF8.GetBytes(jsonString));

            var producer = await rabbitMQService.getProducer();
            await producer.Send(0, message);
        }
    }
}