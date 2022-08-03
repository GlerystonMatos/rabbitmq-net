namespace EnvioRabbitMQ
{
    public class MessageInputModel
    {
        public MessageInputModel(string emissor, string consumidor, string conteudo, string? queue, string? exchange, string? routingKey)
        {
            Queue = queue;
            Emissor = emissor;
            Conteudo = conteudo;
            Exchange = exchange;
            Consumidor = consumidor;
            RoutingKey = routingKey;
        }

        public string Emissor { get; set; }

        public string Consumidor { get; set; }

        public string Conteudo { get; set; }

        public string? Queue { get; set; }

        public string? Exchange { get; set; }

        public string? RoutingKey { get; set; }
    }
}