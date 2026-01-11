public class LoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<LoggingMiddleware> _logger;

    public LoggingMiddleware(RequestDelegate next, ILogger<LoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Request log
        _logger.LogInformation("Incoming Request: {method} {path}",
            context.Request.Method, context.Request.Path);

        await _next(context);

        // Response log
        _logger.LogInformation("Outgoing Response: {statusCode}",
            context.Response.StatusCode);
    }
}
