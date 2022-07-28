using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Prometheus;
using Prometheus.DotNetRuntime;
using Serilog;
using Serilog.Sinks.Grafana.Loki;

namespace ConsumoRabbitMQ
{
    public class Program
    {
        public static void Main(string[] args)
        {
            IList<LokiLabel> labels = new List<LokiLabel>();
            labels.Add(new LokiLabel() { Key = "job", Value = "console" });

            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console(
                    outputTemplate: "datetime={Timestamp:yyyy-mm-dd HH:mm:ss} logLevel=[{Level:u3}] message=\"{Message:lj}\"{NewLine}")
                .WriteTo.GrafanaLoki(
                    "http://loki:3100/",
                    labels: labels,
                    outputTemplate: "datetime={Timestamp:yyyy-mm-dd HH:mm:ss} logLevel=[{Level:u3}] message=\"{Message:lj}\"{NewLine}")
                .CreateLogger();

            Log.Information("Iniciando consumo do RabbitMQ...");

            var collector = CreateCollector();

            var metricServer = new MetricServer(port: 81);
            metricServer.Start();

            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSerilog()
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