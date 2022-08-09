namespace EnvioRabbitMQ.Services
{
    public interface IRabbitMQService
    {
        void Enviar(MessageInputModel message);
    }
}