using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RabbitMQ.Client;
using System.Text;

namespace EnvioRabbitMQ.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MessageController : ControllerBase
    {
        private readonly ConnectionFactory _factory;
        private readonly ILogger<MessageController> _logger;

        //A classe ConnectionFactory � utilizada para configurar a conex�o com o RabbitMQ, permitindo criar uma inst�ncia de IConnection,
        //que estabelece a conex�o TCP com o broker do RabbitMQ. Finalmente, com essa conex�o, � criada um canal (ou channel), que nada
        //mais � do que uma conex�o virtual com o RabbitMQ, utilizando o protocolo AMQP. M�ltiplos canais compartilham uma conex�o TCP �nica.

        private const string HOST_NAME = "10.0.0.131";
        private const string USER_NAME = "glerystonmatos";
        private const string PASSWORD = "123456";

        public MessageController(ILogger<MessageController> logger)
        {
            _logger = logger;

            _factory = new ConnectionFactory();
            _factory.HostName = HOST_NAME;
            _factory.UserName = USER_NAME;
            _factory.Password = PASSWORD;

            _logger.LogInformation("RabbitMQ: Criacao da conexao: HostName: {0}, UserName: {1}, Password: {2}",
                _factory.HostName, _factory.UserName, _factory.Password);
        }

        [HttpPost]
        public IActionResult Post([FromBody] MessageInputModel message)
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

                    //O m�todo QueueDeclare cria uma fila, e � independente. Ou seja, ele s� vai criar a fila caso ela n�o exista,
                    //n�o fazendo nada caso ela j� exista.

                    //queue: Nome da fila que ser� criada.

                    //durable: Se sim, metadados dela s�o armazenados no disco e poder�o ser recuperados ap�s o rein�cio do n� do RabbitMQ.
                    //Al�m disso, em caso de mensagens persistentes, elas s�o restauradas ap�s o rein�cio do n� junto a fila dur�vel.
                    //Em caso de uma mensagem persistente em uma fila n�o-dur�vel, ela ainda ser� perdida ap�s reiniciar o RabbitMQ.

                    //exclusive: Se sim, apenas uma conex�o ser� permitida a ela, e ap�s encerrar, a fila � apagada.

                    //autoDelete: Se sim, a fila vai ser apagada caso, ap�s um consumer ter se conectado, todos se desconectaram e ela ficar
                    //sem conex�es ativas.

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

                    //O m�todo BasicPublish, realiza a publica��o da mensagem. (que est� em formato Array de Bytes)

                    //exchange: S�o agentes respons�veis por rotear as mensagens para filas, utilizando atributos de cabe�alho, routing keys, ou bindings.

                    //routingKey: Funciona como um relacionamento entre um Exchange e uma fila, descrevendo para qual fila a mensagem deve ser direcionada.

                    return Accepted();
                }
            }
        }
    }
}