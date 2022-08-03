namespace ConsumoRabbitMQ
{
    public class Parametros : IParametros
    {
        public string[] GetParametros()
            => Environment.GetCommandLineArgs();
    }
}