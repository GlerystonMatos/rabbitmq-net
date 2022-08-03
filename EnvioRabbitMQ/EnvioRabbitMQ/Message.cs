namespace EnvioRabbitMQ
{
    public class Message
    {
        public Message(string emissor, string consumidor, string conteudo)
        {
            Emissor = emissor;
            Conteudo = conteudo;
            Consumidor = consumidor;
        }

        public string Emissor { get; set; }

        public string Consumidor { get; set; }

        public string Conteudo { get; set; }
    }
}