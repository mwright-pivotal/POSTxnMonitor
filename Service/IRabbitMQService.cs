using RabbitMQ.Stream.Client;

namespace POSTxns.Service
{
    public interface IRabbitMQService {
        public Task<StreamSystem> getStreamSystem();
        public RabbitMQOptions getRabbitConfig();
        public void createConsumer();
        public Task<Producer> getProducer();
    }
}