using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Logs;
using OpenTelemetry.Exporter;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

builder.Services.AddOpenTelemetry()
    .ConfigureResource(ConfigureResource)
    .WithTracing(ConfigureTracing)
    .WithLogging(ConfigureLogging);

builder.Build().Run();

static void ConfigureResource(ResourceBuilder resourceBuilder)
{
    resourceBuilder.AddService("azf-prod-dotnet8-v1-local");
}

static void ConfigureTracing(TracerProviderBuilder tracerProviderBuilder)
{
    tracerProviderBuilder
        .AddSource("af-dotnet8-otel")
        .AddHttpClientInstrumentation()
        .AddConsoleExporter();

    var endpoint = GetRequiredEnv("OTEL_EXPORTER_OTLP_ENDPOINT");
    var headers = GetRequiredEnv("OTEL_EXPORTER_OTLP_HEADERS");

    tracerProviderBuilder.AddOtlpExporter(options =>
    {
        options.Endpoint = new Uri(endpoint);
        options.Protocol = OtlpExportProtocol.HttpProtobuf;
        options.Headers = headers;
    });
}

static void ConfigureLogging(LoggerProviderBuilder loggerProviderBuilder)
{
    loggerProviderBuilder
        .AddConsoleExporter();

    var endpoint = GetRequiredEnv("OTEL_EXPORTER_OTLP_ENDPOINT");
    var headers = GetRequiredEnv("OTEL_EXPORTER_OTLP_HEADERS");

    loggerProviderBuilder.AddOtlpExporter(options =>
    {
        options.Endpoint = new Uri(endpoint);
        options.Protocol = OtlpExportProtocol.HttpProtobuf;
        options.Headers = headers;
    });
}

static string GetRequiredEnv(string name)
{
    var value = Environment.GetEnvironmentVariable(name);

    if (string.IsNullOrWhiteSpace(value))
        throw new InvalidOperationException($"{name} não configurado.");

    return value;
}