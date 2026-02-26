using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Company.Function;

public class HttpTrigger(ILogger<HttpTrigger> logger)
{
    private static readonly ActivitySource ActivitySource = new("af-dotnet8");
    private readonly ILogger<HttpTrigger> _logger = logger;

  [Function("HttpTrigger1")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req)
    {
        using var activity = ActivitySource.StartActivity("HttpTrigger1", ActivityKind.Server);

        try
        {
            using var httpClient = new HttpClient();

            var response = await httpClient.GetAsync("http://localhost:8080/customers");

            // 🔎 Se a dependência falhar, marcamos erro no span
            if (!response.IsSuccessStatusCode)
            {
                var message = $"Dependência retornou {(int)response.StatusCode}";

                activity?.SetTag("dependency.status_code", (int)response.StatusCode);
                activity?.SetStatus(ActivityStatusCode.Error, message);

                _logger.LogError("Erro na dependência: {StatusCode}", response.StatusCode);

                // Ainda retornando 200 propositalmente
                return new OkObjectResult("Falha na dependência, mas requisição processada.");
            }

            var content = await response.Content.ReadAsStringAsync();

            return new OkObjectResult(content);
        }
        catch (Exception ex)
        {
            // Marca erro explícito no span
            activity?.AddException(ex);
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);

            _logger.LogError(ex, "Erro inesperado");

            // Aqui você pode manter 200 se quiser
            return new OkObjectResult("Erro interno tratado.");
        }
    }
}