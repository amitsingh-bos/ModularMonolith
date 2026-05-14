using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace ModularMonolith.BuildingBlocks.Infrastructure.Observability;

public static class ObservabilityExtensions
{
    public static IServiceCollection AddObservability(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var cfg = configuration.GetSection("OpenTelemetry");

        if (!cfg.GetValue("Enabled", true))
            return services;

        var serviceName    = cfg["ServiceName"]    ?? "ModularMonolith";
        var serviceVersion = cfg["ServiceVersion"] ?? "1.0.0";
        var otlpEndpoint   = cfg["OtlpEndpoint"]   ?? "http://localhost:4317";

        services.AddOpenTelemetry()
            .ConfigureResource(r => r
                .AddService(
                    serviceName: serviceName,
                    serviceVersion: serviceVersion,
                    autoGenerateServiceInstanceId: true))
            .WithTracing(tracing => tracing
                .AddAspNetCoreInstrumentation(o =>
                {
                    o.RecordException = true;
                    // skip the Prometheus scrape endpoint itself
                    o.Filter = ctx => ctx.Request.Path != "/metrics";
                })
                .AddHttpClientInstrumentation()
                .AddEntityFrameworkCoreInstrumentation(o =>
                {
                    // includes the SQL text in spans — disable in prod if you want to hide queries
                    o.SetDbStatementForText = true;
                })
                .AddOtlpExporter(o =>
                {
                    o.Endpoint = new Uri(otlpEndpoint);
                    o.Protocol = OtlpExportProtocol.Grpc;
                }))
            .WithMetrics(metrics => metrics
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddRuntimeInstrumentation()
                .AddPrometheusExporter());

        return services;
    }
}
