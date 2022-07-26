using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace ConsumoRabbitMQ
{
    public class ProcessMessageConsumer : BackgroundService
    {
        private readonly IModel _channel;
        private readonly IConnection _connection;
        private readonly ConnectionFactory _factory;
        private readonly ILogger<ProcessMessageConsumer> _logger;

        private const string QUEUE_NAME = "messages";
        private const string HOST_NAME = "10.0.0.131";
        private const string USER_NAME = "glerystonmatos";
        private const string PASSWORD = "123456";

        public ProcessMessageConsumer(ILogger<ProcessMessageConsumer> logger)
        {
            _logger = logger;

            _factory = new ConnectionFactory();
            _factory.HostName = HOST_NAME;
            _factory.UserName = USER_NAME;
            _factory.Password = PASSWORD;

            _logger.LogInformation("Criação da conexão com o RabbitMQ: HostName: {0}, UserName: {1}, Password: {2}",
                _factory.HostName, _factory.UserName, _factory.Password);

            _connection = _factory.CreateConnection();
            _channel = _connection.CreateModel();

            _channel.QueueDeclare(
                queue: QUEUE_NAME,
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            _logger.LogInformation("Criação da fila para receber as mensagens");
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            //Criação do objeto do tipo EventingBasicConsumer, que será responsável pela configuração dos eventos relacionados ao consumo,
            //e pelo início efetivo deles.

            EventingBasicConsumer consumer = new EventingBasicConsumer(_channel);

            // Criação do evento Received, onde teremos acesso à mensagem recebida na fila que será especificada,
            // através da propriedade eventArgs.Body.

            consumer.Received += (sender, eventArgs) =>
            {
                //Conversão dos dados contidos no eventArgs.Body, que são do tipo ReadOnlyMemory, para Array,
                //sendo convertido em seguida para string, e finalmente deserializado em objeto de tipo MessageInputModel.

                byte[] contentArray = eventArgs.Body.ToArray();
                string contentString = Encoding.UTF8.GetString(contentArray);
                MessageInputModel? message = JsonConvert.DeserializeObject<MessageInputModel>(contentString);

                if (message != null)
                {
                    _logger.LogInformation("Mensagem recebida: FromId: {0}, ToId: {1}, Content: {2}, CreatedAt: {3}",
                        message.FromId, message.ToId, message.Content, message.CreatedAt);
                }

                //Realização do Ack, que reconhece a mensagem como entregue.

                _channel.BasicAck(eventArgs.DeliveryTag, false);
            };

            //Início do consumo, utilizando como parâmetros a fila especificada, o reconhecimento automático de entrega (autoAck) como falso,
            //e o objeto consumer de tipo EventingBasicConsumer.

            _channel.BasicConsume(QUEUE_NAME, false, consumer);

            return Task.CompletedTask;
        }
    }
}