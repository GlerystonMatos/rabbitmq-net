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
            if (args.Length < 6)
            {
                Log.Error(
                    "Informe ao menos 6 parametros: " +
                    "no primeiro informe a porta do servidor de métricas, " +
                    "no segundo a fila/queue a que recebera as mensagens, " +
                    "no terceito o host name do RabbitMQ, " +
                    "no quarto o user name para acessar o RabbitMQ," +
                    "no quinto o password para acessar o RabbitMQ," +
                    "no sexto o nome do consumer para identificador o mesmo nos logs,",
                    "no sétimo o nome do exchange que será utilizado,",
                    "no oitavo a routing key que será utilizada pelo exchange.");
                return;
            }

            string consumer = args[5];

            IList<LokiLabel> labels = new List<LokiLabel>();
            labels.Add(new LokiLabel() { Key = "group", Value = "rabbitmq" });
            labels.Add(new LokiLabel() { Key = "job", Value = consumer });

            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console(
                    outputTemplate: "datetime={Timestamp:yyyy-mm-dd HH:mm:ss} logLevel=[{Level:u3}] message=\"{Message:lj}\"{NewLine}")
                .WriteTo.GrafanaLoki(
                    "http://loki:3100/",
                    labels: labels,
                    outputTemplate: "datetime={Timestamp:yyyy-mm-dd HH:mm:ss} logLevel=[{Level:u3}] message=\"{Message:lj}\"{NewLine}")
                .CreateLogger();

            Log.Information("RabbitMQ: Consumidor: {0}: Parâmetros informados: {1} ", consumer, args);
            Log.Information("RabbitMQ: Consumidor: {0}: Iniciando consumo das mensagens...", consumer);

            IDisposable collector = CreateCollector();

            int portaServidorMetricas = int.Parse(args[0]);

            MetricServer metricServer = new MetricServer(port: portaServidorMetricas);
            metricServer.Start();

            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSerilog()
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddSingleton<IParametros, Parametros>();
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