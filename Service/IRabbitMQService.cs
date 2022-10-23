using RabbitMQ.Stream.Client;
using POSTxns.Hubs;

namespace POSTxns.Service
{
    public interface IRabbitMQService {
        public Task<StreamSystem> getStreamSystem();
        public RabbitMQOptions getRabbitConfig();
        public void createConsumer();
        public Task<Producer> getProducer();
        public void setHub(POSTxnsHub hub);
    }
}