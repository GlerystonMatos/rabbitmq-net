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

        //A classe ConnectionFactory é utilizada para configurar a conexão com o RabbitMQ, permitindo criar uma instância de IConnection,
        //que estabelece a conexão TCP com o broker do RabbitMQ. Finalmente, com essa conexão, é criada um canal (ou channel), que nada
        //mais é do que uma conexão virtual com o RabbitMQ, utilizando o protocolo AMQP. Múltiplos canais compartilham uma conexão TCP única.

        private const string QUEUE_NAME = "messages";
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

            _logger.LogInformation("Criação da conexão com o RabbitMQ: HostName: {0}, UserName: {1} Password: {2}",
                _factory.HostName, _factory.UserName, _factory.Password);
        }

        [HttpPost]
        public IActionResult Post([FromBody] MessageInputModel message)
        {
            IConnection connection = _factory.CreateConnection();
            IModel channel = connection.CreateModel();

            channel.QueueDeclare(
                queue: QUEUE_NAME,
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            _logger.LogInformation("Criação da fila para receber as mensagens");

            //O método QueueDeclare cria uma fila, e é independente. Ou seja, ele só vai criar a fila caso ela não exista,
            //não fazendo nada caso ela já exista.

            //queue: Nome da fila que será criada.

            //durable: Se sim, metadados dela são armazenados no disco e poderão ser recuperados após o reinício do nó do RabbitMQ.
            //Além disso, em caso de mensagens persistentes, elas são restauradas após o reinício do nó junto a fila durável.
            //Em caso de uma mensagem persistente em uma fila não-durável, ela ainda será perdida após reiniciar o RabbitMQ.

            //exclusive: Se sim, apenas uma conexão será permitida a ela, e após encerrar, a fila é apagada.

            //autoDelete: Se sim, a fila vai ser apagada caso, após um consumer ter se conectado, todos se desconectaram e ela ficar
            //sem conexões ativas.

            //Converter mensagem para string
            string stringFieldMessage = JsonConvert.SerializeObject(message);
            //Converter string em array de bytes
            byte[] bytesMessage = Encoding.UTF8.GetBytes(stringFieldMessage);

            _logger.LogInformation("Preparação da mensagem para ser enviada");

            channel.BasicPublish(
                exchange: "",
                routingKey: QUEUE_NAME,
                basicProperties: null,
                body: bytesMessage);

            _logger.LogInformation("Publicação da mensagem na fila");

            //O método BasicPublish, realiza a publicação da mensagem. (que está em formato Array de Bytes)

            //exchange: São agentes responsáveis por rotear as mensagens para filas, utilizando atributos de cabeçalho, routing keys, ou bindings.

            //routingKey: Funciona como um relacionamento entre um Exchange e uma fila, descrevendo para qual fila a mensagem deve ser direcionada.

            return Accepted();
        }
    }
}