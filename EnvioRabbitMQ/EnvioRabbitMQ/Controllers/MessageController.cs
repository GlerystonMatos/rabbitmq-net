using EnvioRabbitMQ.Services;
using Microsoft.AspNetCore.Mvc;

namespace EnvioRabbitMQ.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MessageController : ControllerBase
    {
        private readonly IRabbitMQService _rabbitMQService;

        public MessageController(IRabbitMQService rabbitMQService)
            => _rabbitMQService = rabbitMQService;

        [HttpPost]
        public IActionResult Post([FromBody] MessageInputModel message)
        {
            _rabbitMQService.Enviar(message);
            return Accepted();
        }
    }
}