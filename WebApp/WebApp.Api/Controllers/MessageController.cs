using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RabbitMQ.Client;
using System.Text;

namespace WebApp.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MessageController : ControllerBase
    {
        private readonly ConnectionFactory _factory;

        //A classe ConnectionFactory � utilizada para configurar a conex�o com o RabbitMQ, permitindo criar uma inst�ncia de IConnection,
        //que estabelece a conex�o TCP com o broker do RabbitMQ. Finalmente, com essa conex�o, � criada um canal (ou channel), que nada
        //mais � do que uma conex�o virtual com o RabbitMQ, utilizando o protocolo AMQP. M�ltiplos canais compartilham uma conex�o TCP �nica.

        private const string QUEUE_NAME = "messages";

        public MessageController()
        {
            _factory = new ConnectionFactory();
            _factory.HostName = "10.0.0.131";
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

            //O m�todo QueueDeclare cria uma fila, e � independente. Ou seja, ele s� vai criar a fila caso ela n�o exista,
            //n�o fazendo nada caso ela j� exista.

            //queue: Nome da fila que ser� criada.

            //durable: Se sim, metadados dela s�o armazenados no disco e poder�o ser recuperados ap�s o rein�cio do n� do RabbitMQ.
            //Al�m disso, em caso de mensagens persistentes, elas s�o restauradas ap�s o rein�cio do n� junto a fila dur�vel.
            //Em caso de uma mensagem persistente em uma fila n�o-dur�vel, ela ainda ser� perdida ap�s reiniciar o RabbitMQ.

            //exclusive: Se sim, apenas uma conex�o ser� permitida a ela, e ap�s encerrar, a fila � apagada.

            //autoDelete: Se sim, a fila vai ser apagada caso, ap�s um consumer ter se conectado, todos se desconectaram e ela ficar
            //sem conex�es ativas.

            //Converter mensagem para string
            string stringFieldMessage = JsonConvert.SerializeObject(message);
            //Converter string em array de bytes
            byte[] bytesMessage = Encoding.UTF8.GetBytes(stringFieldMessage);

            channel.BasicPublish(
                exchange: "",
                routingKey: QUEUE_NAME,
                basicProperties: null,
                body: bytesMessage);

            //O m�todo BasicPublish, realiza a publica��o da mensagem. (que est� em formato Array de Bytes)

            //exchange: S�o agentes respons�veis por rotear as mensagens para filas, utilizando atributos de cabe�alho, routing keys, ou bindings.

            //routingKey: Funciona como um relacionamento entre um Exchange e uma fila, descrevendo para qual fila a mensagem deve ser direcionada.

            return Accepted();
        }
    }
}