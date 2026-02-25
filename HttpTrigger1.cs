using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Company.Function;

public class HttpTrigger
{
    private static readonly ActivitySource ActivitySource = new ("af-otel-dotnet");
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

        using var httpClient = new HttpClient();

        var response1 = await httpClient.GetStringAsync("http://localhost:8080/customers");

        return new OkObjectResult(response1);
    }


}