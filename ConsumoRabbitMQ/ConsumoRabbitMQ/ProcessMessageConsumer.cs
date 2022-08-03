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

        private readonly string _queueName = "";
        private readonly string _hostName = "";
        private readonly string _userName = "";
        private readonly string _password = "";
        private readonly string _consumerName = "";
        private readonly string _exchange = "";
        private readonly string _routingKey = "";

        public ProcessMessageConsumer(ILogger<ProcessMessageConsumer> logger, IParametros parametros)
        {
            _logger = logger;

            _queueName = parametros.GetParametros()[2];
            _hostName = parametros.GetParametros()[3];
            _userName = parametros.GetParametros()[4];
            _password = parametros.GetParametros()[5];
            _consumerName = parametros.GetParametros()[6];

            if (parametros.GetParametros().Count() >= 8)
            {
                _exchange = parametros.GetParametros()[7];
            }

            if (parametros.GetParametros().Count() >= 9)
            {
                _routingKey = parametros.GetParametros()[8];
            }

            _factory = new ConnectionFactory();
            _factory.HostName = _hostName;
            _factory.UserName = _userName;
            _factory.Password = _password;

            _logger.LogInformation("RabbitMQ: Consumidor: {0}: Criacao da conexao: HostName: {1}, UserName: {2}, Password: {3}",
                _consumerName, _factory.HostName, _factory.UserName, _factory.Password);

            _connection = _factory.CreateConnection();
            _channel = _connection.CreateModel();

            _channel.QueueDeclare(
                queue: _queueName,
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            _logger.LogInformation("RabbitMQ: Consumidor: {0}: Criacao da fila para receber as mensagens", _consumerName);

            if (_exchange != "")
            {
                _channel.QueueBind(
                    queue: _queueName,
                    exchange: _exchange,
                    routingKey: _routingKey);

                _logger.LogInformation("RabbitMQ: Consumidor: {0}: Vinculando fila ao exchange informado", _consumerName);
            }
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
                Message? message = JsonConvert.DeserializeObject<Message>(contentString);

                if (message != null)
                {
                    _logger.LogInformation("RabbitMQ: Consumidor: {0}: Mensagem recebida: Emissor: {1}, Consumidor: {2}, Conteudo: {3}",
                        _consumerName, message.Emissor, message.Consumidor, message.Conteudo);
                }

                //Realização do Ack, que reconhece a mensagem como entregue.

                _channel.BasicAck(eventArgs.DeliveryTag, false);
            };

            //Início do consumo, utilizando como parâmetros a fila especificada, o reconhecimento automático de entrega (autoAck) como falso,
            //e o objeto consumer de tipo EventingBasicConsumer.

            _channel.BasicConsume(_queueName, false, consumer);

            return Task.CompletedTask;
        }
    }
}