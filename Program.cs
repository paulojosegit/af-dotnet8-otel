using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Exporter;


var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

builder.Services.AddOpenTelemetry()
    .ConfigureResource(ConfigureResource)
    .WithTracing(ConfigureTracing);

builder.Build().Run();

static void ConfigureResource(ResourceBuilder resourceBuilder)
{
    resourceBuilder.AddService("azf-prod-dotnet8-v1-local");
}

static void ConfigureTracing(TracerProviderBuilder tracerProviderBuilder)
{
    tracerProviderBuilder
        .AddSource("af-otel-dotnet")
        .AddHttpClientInstrumentation()
        .AddConsoleExporter();

    var endpoint = Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_ENDPOINT");
    var apiToken = Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_HEADERS");

    if (string.IsNullOrWhiteSpace(endpoint))
        throw new InvalidOperationException("OTEL_EXPORTER_OTLP_ENDPOINT não configurado.");

    if (string.IsNullOrWhiteSpace(apiToken))
        throw new InvalidOperationException("OTEL_EXPORTER_OTLP_HEADERS não configurado.");

    var otlpOptions = new OtlpExporterOptions
    {
        Endpoint = new Uri(endpoint),
        Protocol = OtlpExportProtocol.HttpProtobuf,
        Headers = apiToken
    };

    var exporter = new OtlpTraceExporter(otlpOptions);

    tracerProviderBuilder.AddProcessor(
        new SimpleActivityExportProcessor(exporter));
}