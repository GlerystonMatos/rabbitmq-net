using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ConsumoRabbitMQ
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Iniciando consumo do RabbitMQ...");
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHostedService<ProcessMessageConsumer>();
                });
    }
}