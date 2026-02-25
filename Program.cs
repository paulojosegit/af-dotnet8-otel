using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Logs;
using OpenTelemetry.Exporter;
using Microsoft.Extensions.Logging;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

builder.Services.AddOpenTelemetry()
    .ConfigureResource(ConfigureResource)
    .WithTracing(ConfigureTracing)
    .WithLogging(ConfigureLogging);

builder.Build().Run();

static void ConfigureResource(ResourceBuilder resourceBuilder)
{
    resourceBuilder.AddService("af-prod-dotnet8-v4-local");
}

static void ConfigureTracing(TracerProviderBuilder tracerProviderBuilder)
{
    tracerProviderBuilder
        .AddSource("af-dotnet8")
        .AddHttpClientInstrumentation()
        .AddConsoleExporter()
        .AddOtlpExporter(ConfigureOtlpExporterOptionsTraces);
}

static void ConfigureLogging(LoggerProviderBuilder loggerProviderBuilder)
{
    loggerProviderBuilder
        .AddConsoleExporter()
        .AddOtlpExporter(ConfigureOtlpExporterOptionsLogs);
}

static void ConfigureOtlpExporterOptionsTraces(OtlpExporterOptions options)
{
    var endpoint = GetRequiredEnv("OTEL_TRACES_EXPORTER");
    var headers = GetRequiredEnv("OTEL_EXPORTER_OTLP_HEADERS");

    options.Endpoint = new Uri(endpoint);
    options.Protocol = OtlpExportProtocol.HttpProtobuf;
    options.Headers = headers;
}

static void ConfigureOtlpExporterOptionsLogs(OtlpExporterOptions options)
{
    var endpoint = GetRequiredEnv("OTEL_LOGS_EXPORTER");
    var headers = GetRequiredEnv("OTEL_EXPORTER_OTLP_HEADERS");

    options.Endpoint = new Uri(endpoint);
    options.Protocol = OtlpExportProtocol.HttpProtobuf;
    options.Headers = headers;
}

static string GetRequiredEnv(string name)
{
    var value = Environment.GetEnvironmentVariable(name);

    if (string.IsNullOrWhiteSpace(value))
        throw new InvalidOperationException($"{name} não configurado.");

    return value;
}