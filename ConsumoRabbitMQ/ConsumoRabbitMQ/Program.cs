using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Prometheus;
using Prometheus.DotNetRuntime;

namespace ConsumoRabbitMQ
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Iniciando consumo do RabbitMQ...");

            var collector = CreateCollector();

            var metricServer = new MetricServer(port: 81);
            metricServer.Start();

            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHostedService<ProcessMessageConsumer>();
                });

        public static IDisposable CreateCollector()
        {
            DotNetRuntimeStatsBuilder.Builder builder = DotNetRuntimeStatsBuilder.Default();

            builder = DotNetRuntimeStatsBuilder.Customize()
                .WithContentionStats(CaptureLevel.Informational)
                .WithGcStats(CaptureLevel.Verbose)
                .WithThreadPoolStats(CaptureLevel.Informational)
                .WithExceptionStats(CaptureLevel.Errors)
                .WithJitStats();

            builder.RecycleCollectorsEvery(new TimeSpan(0, 20, 0));

            return builder.StartCollecting();
        }
    }
}