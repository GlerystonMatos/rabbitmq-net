using Prometheus;
using Prometheus.DotNetRuntime;
using Serilog;
using Serilog.Sinks.Grafana.Loki;

namespace EnvioRabbitMQ
{
    public class Program
    {
        public static void Main(string[] args)
        {
            IList<LokiLabel> labels = new List<LokiLabel>();
            labels.Add(new LokiLabel() { Key = "job", Value = "api" });

            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console(
                    outputTemplate: "datetime={Timestamp:yyyy-mm-dd HH:mm:ss} logLevel=[{Level:u3}] message=\"{Message:lj}\"{NewLine}")
                .WriteTo.GrafanaLoki(
                    "http://loki:3100/",
                    labels: labels,
                    outputTemplate: "datetime={Timestamp:yyyy-mm-dd HH:mm:ss} logLevel=[{Level:u3}] message=\"{Message:lj}\"{NewLine}")
                .CreateLogger();

            Log.Information("Iniciando envio para o RabbitMQ...");

            var builder = WebApplication.CreateBuilder(args);

            var collector = CreateCollector();

            builder.Host
                .ConfigureLogging((_, loggingBuilder) => { })
                    .UseSerilog();

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            app.UseSwagger();
            app.UseSwaggerUI();

            app.UseHttpMetrics();
            app.UseMetricServer();

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }

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