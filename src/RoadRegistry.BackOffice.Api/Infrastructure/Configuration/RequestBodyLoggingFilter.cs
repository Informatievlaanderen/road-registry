namespace RoadRegistry.BackOffice.Api.Infrastructure.Configuration;

using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

/// <summary>
///     Logs the request body of the HTTP methods that carry one, at debug level.
/// </summary>
/// <remarks>
///     This replaces the framework's own LoggingFilter, which reads the entire request body of every POST and PUT with
///     a synchronous ReadToEnd whether or not anything is going to be logged with it. The body is logged at debug
///     level and this API runs at warning, so in every deployed environment that read is pure cost: it blocks a thread
///     pool thread for as long as the client takes to send, and a handful of concurrent uploads doing that is enough to
///     stop the server from draining its sockets in time - which Kestrel reports as
///     "Reading the request body timed out due to data arriving too slowly. See MinRequestBodyDataRate.".
///     Same output, but the body is only read when someone is listening, and then asynchronously.
/// </remarks>
public sealed class RequestBodyLoggingFilter : IAsyncActionFilter
{
    // The methods the framework filter logged, kept as they were.
    private static readonly string[] MethodsToLog = ["POST", "PUT"];

    private readonly ILogger<RequestBodyLoggingFilter> _logger;

    public RequestBodyLoggingFilter(ILogger<RequestBodyLoggingFilter> logger)
    {
        _logger = logger;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        await LogRequestBody(context.HttpContext.Request);

        await next();
    }

    private async Task LogRequestBody(HttpRequest request)
    {
        if (!_logger.IsEnabled(LogLevel.Debug)
            || !MethodsToLog.Contains(request.Method, StringComparer.OrdinalIgnoreCase))
        {
            return;
        }

        // Without buffering the body can only be read once, and reading it here would leave nothing for the model
        // binder. The request rewind middleware normally provides it; if it is not there, the body is not ours to read.
        if (!request.Body.CanSeek)
        {
            return;
        }

        string httpBody;

        request.Body.Position = 0;
        using (var reader = new StreamReader(request.Body, Encoding.UTF8, true, 1024, true))
        {
            httpBody = await reader.ReadToEndAsync();
        }
        request.Body.Position = 0;

        // The same two templates the framework filter used: destructured when the body is JSON, plain text when it is
        // not, so a reader of the logs sees what was actually sent either way.
        if (IsJson(httpBody))
        {
            _logger.LogDebug("Incoming HTTP {Method}: {@HttpBody}", request.Method, httpBody);
        }
        else
        {
            _logger.LogDebug("Incoming HTTP {Method}: {HttpBody}", request.Method, httpBody);
        }
    }

    private static bool IsJson(string httpBody)
    {
        var trimmed = httpBody.AsSpan().Trim();
        if (trimmed.Length == 0 || (trimmed[0] != '{' && trimmed[0] != '['))
        {
            return false;
        }

        try
        {
            JsonConvert.DeserializeObject(httpBody);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}
