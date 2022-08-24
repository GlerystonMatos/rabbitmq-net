using EnvioRabbitMQ.Controllers;
using Newtonsoft.Json;
using RabbitMQ.Client;
using System.Net.Security;
using System.Security.Authentication;
using System.Text;

namespace EnvioRabbitMQ.Services
{
    public class RabbitMQService : IRabbitMQService
    {
        private readonly ConnectionFactory _factory;
        private readonly ILogger<MessageController> _logger;

        //A classe ConnectionFactory é utilizada para configurar a conexão com o RabbitMQ, permitindo criar uma instância de IConnection,
        //que estabelece a conexão TCP com o broker do RabbitMQ. Finalmente, com essa conexão, é criada um canal (ou channel), que nada
        //mais é do que uma conexão virtual com o RabbitMQ, utilizando o protocolo AMQP. Múltiplos canais compartilham uma conexão TCP única.

        private const int PORT = 5672;
        private const string PASSWORD = "123456";
        private const string HOST_NAME = "10.0.0.131";
        private const string USER_NAME = "glerystonmatos";

        public RabbitMQService(ILogger<MessageController> logger)
        {
            _logger = logger;

            _factory = new ConnectionFactory();
            _factory.Port = PORT;
            _factory.Password = PASSWORD;
            _factory.HostName = HOST_NAME;
            _factory.UserName = USER_NAME;

            // Para uasr amqps no lugar de amqp
            //SslOption sslOption = new SslOption();
            //sslOption.Enabled = true;
            //sslOption.AcceptablePolicyErrors =
            //    SslPolicyErrors.RemoteCertificateNameMismatch |
            //    SslPolicyErrors.RemoteCertificateChainErrors;
            
            //_factory.Ssl = sslOption;

            _logger.LogInformation("RabbitMQ: Criacao da conexao: HostName: {0}, UserName: {1}, Password: {2}, Port: {3}",
                _factory.HostName, _factory.UserName, _factory.Password, _factory.Port);
        }

        public void Enviar(MessageInputModel message)
        {
            using (IConnection connection = _factory.CreateConnection())
            {
                using (IModel channel = connection.CreateModel())
                {
                    if (message.Queue != null)
                    {
                        channel.QueueDeclare(
                            queue: message.Queue,
                            durable: false,
                            exclusive: false,
                            autoDelete: false,
                            arguments: null);

                        _logger.LogInformation("RabbitMQ: Emissor: {0}: Criacao da fila para receber as mensagens", message.Emissor);

                        if (message.Exchange != null)
                        {
                            channel.QueueBind(
                                queue: message.Queue,
                                exchange: message.Exchange,
                                routingKey: (message.RoutingKey != null) ? message.RoutingKey : "");

                            _logger.LogInformation("RabbitMQ: Emissor: {0}: Vinculando fila ao exchange informado", message.Emissor);
                        }
                    }

                    //O método QueueDeclare cria uma fila, e é independente. Ou seja, ele só vai criar a fila caso ela não exista,
                    //não fazendo nada caso ela já exista.

                    //queue: Nome da fila que será criada.

                    //durable: Se sim, metadados dela são armazenados no disco e poderão ser recuperados após o reinício do nó do RabbitMQ.
                    //Além disso, em caso de mensagens persistentes, elas são restauradas após o reinício do nó junto a fila durável.
                    //Em caso de uma mensagem persistente em uma fila não-durável, ela ainda será perdida após reiniciar o RabbitMQ.

                    //exclusive: Se sim, apenas uma conexão será permitida a ela, e após encerrar, a fila é apagada.

                    //autoDelete: Se sim, a fila vai ser apagada caso, após um consumer ter se conectado, todos se desconectaram e ela ficar
                    //sem conexões ativas.

                    Message mensagem = new Message(message.Emissor, message.Consumidor, message.Conteudo);

                    //Converter mensagem para string
                    string stringFieldMessage = JsonConvert.SerializeObject(mensagem);
                    //Converter string em array de bytes
                    byte[] bytesMessage = Encoding.UTF8.GetBytes(stringFieldMessage);

                    _logger.LogInformation("RabbitMQ: Emissor: {0}: Preparacao da mensagem para ser enviada", message.Emissor);

                    string routingKey = "";
                    if (message.RoutingKey != null)
                    {
                        routingKey = message.RoutingKey;
                    }
                    else if (message.Queue != null)
                    {
                        routingKey = message.Queue;
                    }

                    channel.BasicPublish(
                        exchange: (message.Exchange != null) ? message.Exchange : "",
                        routingKey: routingKey,
                        basicProperties: null,
                        body: bytesMessage);

                    _logger.LogInformation("RabbitMQ: Emissor: {0}: Publicacao da mensagem na fila", message.Emissor);

                    //O método BasicPublish, realiza a publicação da mensagem. (que está em formato Array de Bytes)

                    //exchange: São agentes responsáveis por rotear as mensagens para filas, utilizando atributos de cabeçalho, routing keys, ou bindings.

                    //routingKey: Funciona como um relacionamento entre um Exchange e uma fila, descrevendo para qual fila a mensagem deve ser direcionada.                    
                }
            }
        }
    }
}