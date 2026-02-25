using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Company.Function;

public class HttpTrigger
{
    private static readonly ActivitySource ActivitySource = new("af-dotnet8-otel");
    private readonly ILogger<HttpTrigger> _logger;

    public HttpTrigger(ILogger<HttpTrigger> logger)
    {
        _logger = logger;
    }

    [Function("HttpTrigger1")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req)
    {
        using var serverActivity = ActivitySource.StartActivity(
            "HttpTrigger1",
            ActivityKind.Server);

        serverActivity?.SetTag("http.request.method", req.Method);
        serverActivity?.SetTag("url.path", req.Path);
        serverActivity?.SetTag("server.address", req.Host.Host);

        var traceId = Activity.Current?.TraceId.ToString();

        _logger.LogInformation(
            "Iniciando HttpTrigger1 | TraceId: {TraceId} | Path: {Path}",
            traceId,
            req.Path);

        try
        {
            using var httpClient = new HttpClient();

            _logger.LogInformation(
                "Chamando API externa /customers | TraceId: {TraceId}",
                traceId);

            var response1 = await httpClient.GetStringAsync("http://localhost:8080/customers");

            _logger.LogInformation(
                "Resposta recebida com sucesso | TraceId: {TraceId} | Tamanho: {Length}",
                traceId,
                response1.Length);

            return new OkObjectResult(response1);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Erro ao processar requisição | TraceId: {TraceId}",
                traceId);

            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }
    }
}